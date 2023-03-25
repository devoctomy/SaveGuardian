using Microsoft.VisualBasic;
using SaveGuardian.Model;

namespace SaveGuardian.Services;

public class DefaultBackupService : IBackupService
{
    private readonly ILogger<DefaultBackupService> _logger;
    private readonly IBackupFileNamingService _backupFileNamingService;
    private readonly IIOService _ioService;
    private readonly IProcessService _processService;
    private readonly IHashingService _hashingService;

    public DefaultBackupService(
        ILogger<DefaultBackupService> logger,
        IBackupFileNamingService backupFileNamingService,
        IIOService ioService,
        IProcessService processService,
        IHashingService hashingService)
    {
        _logger = logger;
        _backupFileNamingService = backupFileNamingService;
        _ioService = ioService;
        _processService = processService;
        _hashingService = hashingService;
    }

    public async Task<bool> ProcessAsync(
        VersionFolder versionFolder,
        string path)
    {
        if (_processService.IsRunning(versionFolder.BackupBlockingProcesses))
        {
            return false;
        }

        var nextHash = await _hashingService.HashFileAsync(
            path,
            CancellationToken.None);
        var currentHashFullPath = _backupFileNamingService.Rename(
            versionFolder,
            path,
            "hash",
            false);

        if(File.Exists(currentHashFullPath)) 
        {
            var currentHash = await File.ReadAllTextAsync(currentHashFullPath);
            if(currentHash == nextHash)
            {
                return true; // Contents of file hasn't changed since last backup
            }
        }

        var backupFullPath = _backupFileNamingService.Rename(
            versionFolder,
            path,
            "bak",
            true);
        var backDirectory = backupFullPath[..backupFullPath.LastIndexOf('/')];
        _ioService.CreateDirectory(backDirectory);
        _ioService.CopyFile(
            path,
            backupFullPath,
            true); // !!! overwrite? should never conflict

        await _ioService.WriteAllTextAsync(
            currentHashFullPath,
            nextHash,
            CancellationToken.None);

        _logger.LogInformation($"Creating versioned copy of file '{path}' for version folder '{versionFolder.Name}' using the backup filename '{backupFullPath}'.");
        return true;
    }
}
