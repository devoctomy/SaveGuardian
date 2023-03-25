using SaveGuardian.Services;

namespace SaveGuardian.UnitTests.Services;

public class Sha256HashingServiceTests
{
    [Fact]
    public async Task GivenDisposedService_WhenHashFileAsync_ThenObjectDisposedExceptionThrownAsync()
    {
        // Arrange
        var sut = new Sha256HashingService();
        var path = Guid.NewGuid().ToString() + ".tmp";
        sut.Dispose();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () =>
        {
            var hash = await sut.HashFileAsync(
                path,
                CancellationToken.None);
        });
    }

    [Fact]
    public async Task GivenFilePath_AndFileExists_WhenHashFileAsync_ThenCorrectHashReturned()
    {
        // Arrange
        var sut = new Sha256HashingService();
        var path = Guid.NewGuid().ToString() + ".tmp";
        await File.WriteAllTextAsync(path, "Hello World!");

        // Act
        var hash = await sut.HashFileAsync(
                path,
                CancellationToken.None);
        File.Delete(path);

        // Assert
        Assert.Equal("7F83B1657FF1FC53B92DC18148A1D65DFC2D4B1FA3D677284ADDD200126D9069", hash);
    }

    [Fact]
    public async Task Given2ConsecurivePaths_WhenHashFileAsync_ThenCorrectHashesReturned()
    {
        // Arrange
        var sut = new Sha256HashingService();
        var path1 = Guid.NewGuid().ToString() + ".tmp";
        await File.WriteAllTextAsync(path1, "Hello World!");
        var path2 = Guid.NewGuid().ToString() + ".tmp";
        await File.WriteAllTextAsync(path2, "Bob Hoskins!");

        // Act
        var hash1 = await sut.HashFileAsync(
                path1,
                CancellationToken.None);
        File.Delete(path1);
        var hash2 = await sut.HashFileAsync(
                path2,
                CancellationToken.None);
        File.Delete(path2);

        // Assert
        Assert.Equal("7F83B1657FF1FC53B92DC18148A1D65DFC2D4B1FA3D677284ADDD200126D9069", hash1);
        Assert.Equal("ADF9DF2F98DF417F6A6B3272E4654DE5E7A03053CF8888F92C48763AC3A50850", hash2);
    }

}
