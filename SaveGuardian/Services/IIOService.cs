namespace SaveGuardian.Services;

public interface IIOService
{
    public void CreateDirectory(string fullPath);
    public void CopyFile(string source, string destination, bool overwrite);
    public Task<string> ReadAllTextAsync(string fullPath, CancellationToken cancellationToken);
    public Task WriteAllTextAsync(string fullPath, string contents, CancellationToken cancellationToken);
}
