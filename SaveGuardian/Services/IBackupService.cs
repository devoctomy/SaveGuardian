using SaveGuardian.Model;

namespace SaveGuardian.Services;

public interface IBackupService
{
    bool Process(
        VersionFolder versionFolder,
        string path);
}
