using SaveGuardian.Model;

namespace SaveGuardian.Services;

public interface IGuardianServiceConfigurator
{
    public IReadOnlyList<VersionFolder>? VersionFolders { get; }
    public Task<bool> InitialiseAsync(CancellationToken cancellationToken);
}
