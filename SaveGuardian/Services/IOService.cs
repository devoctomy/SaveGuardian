using System.Diagnostics.CodeAnalysis;

namespace SaveGuardian.Services;

[ExcludeFromCodeCoverage]
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

    public Task<string> ReadAllTextAsync(
        string fullPath,
        CancellationToken cancellationToken)
    {
        return File.ReadAllTextAsync(fullPath, cancellationToken);
    }
}
