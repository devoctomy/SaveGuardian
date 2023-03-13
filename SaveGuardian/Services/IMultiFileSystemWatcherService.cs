using SaveGuardian.Model;

namespace SaveGuardian.Services;

public interface IMultiFileSystemWatcherService
{
    event EventHandler<MultiFileSystemWatcherServiceChangeOccurredEventArgs>? ChangeOccurred;

    void Initialise(IReadOnlyList<VersionFolder> versionFolders);
    public void Start();
    public void Stop();
}
