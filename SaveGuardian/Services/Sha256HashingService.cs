using System.Security.Cryptography;

namespace SaveGuardian.Services;

public class Sha256HashingService : IHashingService, IDisposable
{
    private SHA256? _sha256;
    private bool disposedValue;

    public Sha256HashingService()
    {
        _sha256 = SHA256.Create();
    }

    ~Sha256HashingService()
    {
        Dispose(false);
    }

    public async Task<string> HashFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if(_sha256 == null)
        {
            throw new ObjectDisposedException("Hashing service has already been disposed.");
        }

        using var stream = new BufferedStream(File.OpenRead(path), 1048576 * 5 /* 5Mb */);
        byte[] checksum = await _sha256.ComputeHashAsync(stream, cancellationToken);
        return BitConverter.ToString(checksum).Replace("-", String.Empty);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if(_sha256!= null)
                {
                    _sha256.Dispose();
                    _sha256 = null;
                }
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
