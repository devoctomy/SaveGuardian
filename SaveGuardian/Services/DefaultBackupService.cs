using SaveGuardian.Model;

namespace SaveGuardian.Services;

public class DefaultBackupService : IBackupService
{
    private readonly ILogger<DefaultBackupService> _logger;
    private readonly IBackupFileNamingService _backupFileNamingService;
    private readonly IIOService _ioService;
    private readonly IProcessService _processService;

    public DefaultBackupService(
        ILogger<DefaultBackupService> logger,
        IBackupFileNamingService backupFileNamingService,
        IIOService ioService,
        IProcessService processService)
    {
        _logger = logger;
        _backupFileNamingService = backupFileNamingService;
        _ioService = ioService;
        _processService = processService;
    }

    public bool Process(
        VersionFolder versionFolder,
        string path)
    {
        if (_processService.IsRunning(versionFolder.BackupBlockingProcesses))
        {
            return false;
        }

        var backupFullPath = _backupFileNamingService.Rename(
            versionFolder,
            path,
            "bak");
        var backDirectory = backupFullPath[..backupFullPath.LastIndexOf('/')];
        _ioService.CreateDirectory(backDirectory);
        _ioService.CopyFile(
            path,
            backupFullPath,
            true); // !!! overwrite? should never conflict

        _logger.LogInformation($"Creating versioned copy of file '{path}' for version folder '{versionFolder.Name}' using the backup filename '{backupFullPath}'.");
        return true;
    }
}
