namespace SaveGuardian.Services;

public class IOService : IIOService
{
    public void CopyFile(
        string source,
        string destination,
        bool overwrite)
    {
        File.Copy(source, destination, overwrite);
    }

    public void CreateDirectory(string fullPath)
    {
        Directory.CreateDirectory(fullPath);
    }
}
