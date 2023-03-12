namespace SaveGuardian.Services;

public interface IGuardianService
{
    public Task<bool> Initialise();
    public void SetupWatchers();
}
