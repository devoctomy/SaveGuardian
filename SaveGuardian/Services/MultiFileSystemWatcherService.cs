using SaveGuardian.Model;

namespace SaveGuardian.Services;

public class MultiFileSystemWatcherService : IMultiFileSystemWatcherService
{
    public event EventHandler<MultiFileSystemWatcherServiceChangeOccurredEventArgs>? ChangeOccurred;

    private Dictionary<FileSystemWatcher, VersionFolder> _watchers;

    public MultiFileSystemWatcherService()
    {
        _watchers = new Dictionary<FileSystemWatcher, VersionFolder>();
    }

    public void Initialise(IReadOnlyList<VersionFolder> versionFolders)
    {
        foreach (var curFolder in versionFolders)
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

    public void Start()
    {
        _watchers.Keys.ToList().ForEach(x => x.EnableRaisingEvents = true);
    }

    public void Stop()
    {
        _watchers.Keys.ToList().ForEach(x => x.EnableRaisingEvents = false);
    }

    private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        var versionFolder = _watchers[(FileSystemWatcher)sender];
        if (versionFolder == null)
        {
            return;
        }

        ChangeOccurred?.Invoke(
            this,
            new MultiFileSystemWatcherServiceChangeOccurredEventArgs(versionFolder, e.FullPath));
    }

    private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
    {
        var versionFolder = _watchers[(FileSystemWatcher)sender];
        if (versionFolder == null)
        {
            return;
        }

        ChangeOccurred?.Invoke(
            this,
            new MultiFileSystemWatcherServiceChangeOccurredEventArgs(versionFolder, e.FullPath));
    }
}
