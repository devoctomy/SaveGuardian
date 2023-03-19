using SaveGuardian.Model;

namespace SaveGuardian.Services;

public class GuardianService : IGuardianService
{
    private readonly ILogger<GuardianService> _logger;
    private readonly IGuardianServiceConfigurator _guardianServiceConfigurator;
    private readonly IBackupService _backupService;
    private readonly IMultiFileSystemWatcherService _multiFileSystemWatcherService;
    private List<BackupOperation> _pendingBackups;

    public GuardianService(
        ILogger<GuardianService> logger,
        IGuardianServiceConfigurator guardianServiceConfigurator,
        IBackupService backupService,
        IMultiFileSystemWatcherService multiFileSystemWatcherService)
    {
        _pendingBackups = new List<BackupOperation>();
        _logger = logger;
        _guardianServiceConfigurator = guardianServiceConfigurator;
        _backupService = backupService;
        _multiFileSystemWatcherService = multiFileSystemWatcherService;
        _multiFileSystemWatcherService.ChangeOccurred += _multiFileSystemWatcherService_ChangeOccurred;
    }

    public async Task<bool> InitialiseAsync(CancellationToken cancellationToken)
    {
        return await _guardianServiceConfigurator.InitialiseAsync(cancellationToken);
    }

    public void SetupWatchers()
    {
        if(_guardianServiceConfigurator.VersionFolders == null ||
            _guardianServiceConfigurator.VersionFolders.Count == 0)
        {
            return;
        }

        _multiFileSystemWatcherService.Initialise(_guardianServiceConfigurator.VersionFolders);
    }

    public void Start()
    {
        _multiFileSystemWatcherService.Start();
    }

    public void Stop()
    {
        _multiFileSystemWatcherService.Stop();
    }

    private void QueueBackup(
        VersionFolder versionFolder,
        string path)
    {
        lock (_pendingBackups)
        {
            var existing = _pendingBackups.SingleOrDefault(x =>
            x.VersionFolder == versionFolder &&
            x.Path == path);
            if (existing != null)
            {
                existing.Changed();
                return;
            }

            var backupOperation = new BackupOperation(
                versionFolder,
                path);
            _pendingBackups.Add(backupOperation);
            backupOperation.Ready += BackupOperation_Ready;
            backupOperation.Changed();
        }
    }

    private void BackupOperation_Ready(object? sender, EventArgs e)
    {
        lock (_pendingBackups)
        {
            var backupOperation = sender as BackupOperation;
            if(backupOperation == null)
            {
                return;
            }

            var backedUp = _backupService.Process(
                backupOperation.VersionFolder,
                backupOperation.Path);
            if (!backedUp)
            {
                _logger.LogInformation("Backup process returned false, queuing for another attempt.");
                backupOperation.Changed();
                return;
            }

            _logger.LogInformation("Backup process was successful.");
            _pendingBackups.Remove(backupOperation);
        }
    }

    private void _multiFileSystemWatcherService_ChangeOccurred(object? sender, MultiFileSystemWatcherServiceChangeOccurredEventArgs e)
    {
        var relativePath = e.FullPath.Replace(e.VersionFolder.Path, string.Empty);
        _logger.LogInformation($"File '{relativePath}' changed in version folder '{e.VersionFolder.Name}'");
        QueueBackup(e.VersionFolder, e.FullPath);
    }
}
