using SaveGuardian.Model;

namespace SaveGuardian.Services;

public class MultiFileSystemWatcherServiceChangeOccurredEventArgs : EventArgs
{
    public VersionFolder VersionFolder { get; set; }
    public string FullPath { get; set; }

    public MultiFileSystemWatcherServiceChangeOccurredEventArgs(
        VersionFolder versionFolder,
        string fullPath)
    {
        VersionFolder = versionFolder;
        FullPath = fullPath;
    }
}
