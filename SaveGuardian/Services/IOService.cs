namespace SaveGuardian.Services;

public class IOService : IIOService
{
    public void CreateDirectory(string fullPath)
    {
        Directory.CreateDirectory(fullPath);
    }
}
