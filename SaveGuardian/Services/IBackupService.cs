using SaveGuardian.Model;

namespace SaveGuardian.Services;

public interface IBackupService
{
    Task<bool> ProcessAsync(
        VersionFolder versionFolder,
        string path);
}
