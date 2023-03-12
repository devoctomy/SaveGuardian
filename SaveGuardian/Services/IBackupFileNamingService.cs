using SaveGuardian.Model;

namespace SaveGuardian.Services;

public interface IBackupFileNamingService
{
    public string Rename(
        VersionFolder versionFolder,
        string fullPath,
        string extension);
}
