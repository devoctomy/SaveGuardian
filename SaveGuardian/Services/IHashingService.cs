namespace SaveGuardian.Services;

public interface IHashingService
{
    public Task<string> HashFileAsync(
        string path,
        CancellationToken cancellationToken);
}
