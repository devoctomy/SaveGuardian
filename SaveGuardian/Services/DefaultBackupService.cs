using SaveGuardian.Model;

namespace SaveGuardian.Services;

public class DefaultBackupService : IBackupService
{
    private readonly ILogger<DefaultBackupService> _logger;
    private readonly IBackupFileNamingService _backupFileNamingService;
    private readonly IIOService _ioService;

    public DefaultBackupService(
        ILogger<DefaultBackupService> logger,
        IBackupFileNamingService backupFileNamingService,
        IIOService ioService)
    {
        _logger = logger;
        _backupFileNamingService = backupFileNamingService;
        _ioService = ioService;
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
        if(backupFileInfo?.Directory?.FullName != null)
        {
            _ioService.CreateDirectory(backupFileInfo.Directory.FullName);
        }
        _ioService.CopyFile(
            path,
            backupFullPath,
            true); // !!! overwrite? should never conflict

        _logger.LogInformation($"Creating versioned copy of file '{path}' for version folder '{versionFolder.Name}'");
        return true;
    }
}
