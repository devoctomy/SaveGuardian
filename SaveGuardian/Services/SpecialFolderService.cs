namespace SaveGuardian.Services;

public class SpecialFolderService : ISpecialFolderService
{
    public string LocalApplicationData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
}
