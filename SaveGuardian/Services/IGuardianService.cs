namespace SaveGuardian.Services;

public interface IGuardianService
{
    public Task<bool> InitialiseAsync(CancellationToken cancellationToken);
    public void SetupWatchers();
    public void Start();
    public void Stop();
}
