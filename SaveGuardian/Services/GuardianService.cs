using SaveGuardian.Model;

namespace SaveGuardian.Services;

public class GuardianService : IGuardianService
{
    private readonly ILogger<GuardianService> _logger;
    private readonly IGuardianServiceConfigurator _guardianServiceConfigurator;
    private readonly IBackupService _backupService;
    private Dictionary<FileSystemWatcher, VersionFolder> _watchers;
    private List<BackupOperation> _pendingBackups;

    public GuardianService(
        ILogger<GuardianService> logger,
        IGuardianServiceConfigurator guardianServiceConfigurator,
        IBackupService backupService)
    {
        _watchers = new Dictionary<FileSystemWatcher, VersionFolder>();
        _pendingBackups = new List<BackupOperation>();
        _logger = logger;
        _guardianServiceConfigurator = guardianServiceConfigurator;
        _backupService = backupService;
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
  
        foreach (var curFolder in _guardianServiceConfigurator.VersionFolders)
        {
            var backupRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var backupRootDI = new DirectoryInfo(backupRoot);
            var appDirDI = backupRootDI.CreateSubdirectory("SaveGuardian");
            var versionFolderDir = appDirDI.CreateSubdirectory(curFolder.Name);

            var fileSystemWatcher = new FileSystemWatcher(
                curFolder.Path,
                curFolder.Filter)
            {
                IncludeSubdirectories = curFolder.IncludeSubdirectories,
                NotifyFilter = NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.FileName |
                NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Size |
                NotifyFilters.Security
            };
            fileSystemWatcher.Created += FileSystemWatcher_Created;
            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            _watchers.Add(fileSystemWatcher, curFolder);
        }
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
                backupOperation.Changed();
                return;
            }

            _pendingBackups.Remove(backupOperation);
        }
    }

    private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        var versionFolder = _watchers[(FileSystemWatcher)sender];
        if(versionFolder == null)
        {
            return;
        }

        var relativePath = e.FullPath.Replace(versionFolder.Path, string.Empty);
        _logger.LogInformation($"File '{relativePath}' changed in version folder '{versionFolder.Name}'");
        QueueBackup(versionFolder, e.FullPath);
    }

    private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
    {
        var versionFolder = _watchers[(FileSystemWatcher)sender];
        if (versionFolder == null)
        {
            return;
        }

        var relativePath = e.FullPath.Replace(versionFolder.Path, string.Empty);
        _logger.LogInformation($"File '{relativePath}' created in version folder '{versionFolder.Name}'");
        QueueBackup(versionFolder, e.FullPath);
    }
}
