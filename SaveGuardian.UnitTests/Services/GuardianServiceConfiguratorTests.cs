using Moq;
using Newtonsoft.Json;
using SaveGuardian.Model;
using SaveGuardian.Services;

namespace SaveGuardian.UnitTests.Services;

public class GuardianServiceConfiguratorTests
{
    [Fact]
    public async Task GivenCancellationToken_AndConfigEmpty_WhenInitialiseAsync_ThenConfigFileLoaded_AndFalseReturned()
    {
        // Act
        var mockIOService = new Mock<IIOService>();
        var sut = new GuardianServiceConfigurator(mockIOService.Object);
        var config = new
        {
            VersionFolders = new List<VersionFolder>()
        };

        var cancellationTokenSource = new CancellationTokenSource();

        mockIOService.Setup(x => x.ReadAllTextAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(string.Empty);

        // Arrange
        var result = await sut.InitialiseAsync(cancellationTokenSource.Token);

        // Assert
        Assert.False(result);
        Assert.Empty(config.VersionFolders);
        mockIOService.Verify(x => x.ReadAllTextAsync(
            It.Is<string>(y => y == "Config/folders.json"),
            It.Is<CancellationToken>(y => y == cancellationTokenSource.Token)), Times.Once);
    }

    [Fact]
    public async Task GivenCancellationToken_AndVersionFoldersInConfig_WhenInitialiseAsync_ThenConfigFileLoaded_AndJsonParsed_AndTrueReturned()
    {
        // Act
        var mockIOService = new Mock<IIOService>();
        var sut = new GuardianServiceConfigurator(mockIOService.Object);
        var config = new
        {
            VersionFolders = new List<VersionFolder>
            {
                new VersionFolder
                {
                    Name = "Hello World"
                }
            }
        };

        var cancellationTokenSource = new CancellationTokenSource();

        mockIOService.Setup(x => x.ReadAllTextAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(JsonConvert.SerializeObject(config));

        // Arrange
        var result = await sut.InitialiseAsync(cancellationTokenSource.Token);

        // Assert
        Assert.True(result);
        Assert.Equal(config.VersionFolders[0].Name, sut.VersionFolders[0].Name);
        mockIOService.Verify(x => x.ReadAllTextAsync(
            It.Is<string>(y => y == "Config/folders.json"),
            It.Is<CancellationToken>(y => y == cancellationTokenSource.Token)), Times.Once);
    }

    [Fact]
    public async Task GivenCancellationToken_AndNoFoldersInConfig_WhenInitialiseAsync_ThenConfigFileLoaded_AndFalseReturned()
    {
        // Act
        var mockIOService = new Mock<IIOService>();
        var sut = new GuardianServiceConfigurator(mockIOService.Object);
        var config = new
        {
            VersionFolders = new List<VersionFolder>()
        };

        var cancellationTokenSource = new CancellationTokenSource();

        mockIOService.Setup(x => x.ReadAllTextAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(JsonConvert.SerializeObject(config));

        // Arrange
        var result = await sut.InitialiseAsync(cancellationTokenSource.Token);

        // Assert
        Assert.False(result);
        Assert.Empty(config.VersionFolders);
        mockIOService.Verify(x => x.ReadAllTextAsync(
            It.Is<string>(y => y == "Config/folders.json"),
            It.Is<CancellationToken>(y => y == cancellationTokenSource.Token)), Times.Once);
    }
}
