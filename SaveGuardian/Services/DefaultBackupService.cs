using SaveGuardian.Model;

namespace SaveGuardian.Services;

public class DefaultBackupService : IBackupService
{
    private readonly ILogger<DefaultBackupService> _logger;
    private readonly IBackupFileNamingService _backupFileNamingService;

    public DefaultBackupService(
        ILogger<DefaultBackupService> logger,
        IBackupFileNamingService backupFileNamingService)
    {
        _logger = logger;
        _backupFileNamingService = backupFileNamingService;
    }

    public bool Process(
        VersionFolder versionFolder,
        string path)
    {
        var block = versionFolder.BackupBlockingProcesses.Any(x => System.Diagnostics.Process.GetProcessesByName(x).Length > 0);
        if (block)
        {
            return false;
        }

        var backupFullPath = _backupFileNamingService.Rename(
            versionFolder,
            path,
            ".bak");
        var backupFileInfo = new FileInfo(backupFullPath);

        backupFileInfo?.Directory?.Create();
        File.Copy(
            path,
            backupFullPath);

        _logger.LogInformation($"Creating versioned copy of file '{path}' for version folder '{versionFolder.Name}'");
        return true;
    }
}
