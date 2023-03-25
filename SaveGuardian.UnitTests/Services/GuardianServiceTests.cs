using Microsoft.Extensions.Logging;
using Moq;
using SaveGuardian.Model;
using SaveGuardian.Services;

namespace SaveGuardian.UnitTests.Services;

public class GuardianServiceTests
{
    [Fact]
    public async Task GivenCancellationToken_WhenInitialiseAsync_ThenConfiguratorInitialised_AndResultReturned()
    {
        // Arrange
        var mockGuardianServiceConfigurator = new Mock<IGuardianServiceConfigurator>();
        var mockBackupService = new Mock<IBackupService>();
        var mockMultiFileSystemWatcherService = new Mock<IMultiFileSystemWatcherService>();
        var sut = new GuardianService(
            Mock.Of<ILogger<GuardianService>>(),
            mockGuardianServiceConfigurator.Object,
            mockBackupService.Object,
            mockMultiFileSystemWatcherService.Object);

        mockGuardianServiceConfigurator.Setup(x => x.InitialiseAsync(
            It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        var result = await sut.InitialiseAsync(cancellationTokenSource.Token);

        // Assert
        Assert.True(result);
        mockGuardianServiceConfigurator.Verify(x => x.InitialiseAsync(
            It.Is<CancellationToken>(y => y == cancellationTokenSource.Token)), Times.Once);
    }

    [Fact]
    public void GivenNoVersionFoldersConfigured_WhenSetupWatchers_ThenWatcherServiceNotInitialised()
    {
        // Arrange
        var mockGuardianServiceConfigurator = new Mock<IGuardianServiceConfigurator>();
        var mockBackupService = new Mock<IBackupService>();
        var mockMultiFileSystemWatcherService = new Mock<IMultiFileSystemWatcherService>();
        var sut = new GuardianService(
            Mock.Of<ILogger<GuardianService>>(),
            mockGuardianServiceConfigurator.Object,
            mockBackupService.Object,
            mockMultiFileSystemWatcherService.Object);

        var versionFolders = new List<VersionFolder>();

        mockGuardianServiceConfigurator.SetupGet(x => x.VersionFolders).Returns(versionFolders);

        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        sut.SetupWatchers();

        // Assert
        mockMultiFileSystemWatcherService.Verify(x => x.Initialise(
            It.Is<List<VersionFolder>>(y => y == versionFolders)), Times.Never);
    }

    [Fact]
    public void GivenVersionFolderConfigured_WhenSetupWatchers_ThenWatcherServiceNotInitialised()
    {
        // Arrange
        var mockGuardianServiceConfigurator = new Mock<IGuardianServiceConfigurator>();
        var mockBackupService = new Mock<IBackupService>();
        var mockMultiFileSystemWatcherService = new Mock<IMultiFileSystemWatcherService>();
        var sut = new GuardianService(
            Mock.Of<ILogger<GuardianService>>(),
            mockGuardianServiceConfigurator.Object,
            mockBackupService.Object,
            mockMultiFileSystemWatcherService.Object);

        var versionFolders = new List<VersionFolder>
        {
            new VersionFolder()
        };

        mockGuardianServiceConfigurator.SetupGet(x => x.VersionFolders).Returns(versionFolders);

        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        sut.SetupWatchers();

        // Assert
        mockMultiFileSystemWatcherService.Verify(x => x.Initialise(
            It.Is<List<VersionFolder>>(y => y == versionFolders)), Times.Once);
    }

    [Fact]
    public async Task GivenVersionFolderConfigured_WhenChangeOccurs_ThenAfterGracePeriodBackupServiceCalled_AndBackupSuccessOnFirstAttempt_AndBackupNotProcessedAgain()
    {
        // Arrange
        var mockGuardianServiceConfigurator = new Mock<IGuardianServiceConfigurator>();
        var mockBackupService = new Mock<IBackupService>();
        var mockMultiFileSystemWatcherService = new Mock<IMultiFileSystemWatcherService>();
        var sut = new GuardianService(
            Mock.Of<ILogger<GuardianService>>(),
            mockGuardianServiceConfigurator.Object,
            mockBackupService.Object,
            mockMultiFileSystemWatcherService.Object);

        var versionFolder = new VersionFolder
        {
            Path = "c:/somefolder/",
            ChangeGracePeriod = TimeSpan.FromSeconds(5)
        };
        var eventArgs = new MultiFileSystemWatcherServiceChangeOccurredEventArgs(
            versionFolder,
            "c:/somefolder/subfolder/file.ext");

        mockBackupService.Setup(x => x.ProcessAsync(
            It.IsAny<VersionFolder>(),
            It.IsAny<string>())).ReturnsAsync(true);

        // Act
        mockMultiFileSystemWatcherService.Raise(x => x.ChangeOccurred += null,
            mockMultiFileSystemWatcherService.Object,
            eventArgs);

        await Task.Delay(TimeSpan.FromSeconds(6));
        await Task.Delay(TimeSpan.FromSeconds(6));

        // Assert
        mockBackupService.Verify(x => x.ProcessAsync(
            It.Is<VersionFolder>(y => y == eventArgs.VersionFolder),
            It.Is<string>(y => y == eventArgs.FullPath)), Times.Once);
    }

    [Fact]
    public async Task GivenVersionFolderConfigured_WhenChangeOccurs_ThenAfterGracePeriodBackupServiceCalled_AndBackupFailsOnFirstAttempt_AndBackupSuccessOnSecondAttempt_AndBackupNotProcessedAgain()
    {
        // Arrange
        var mockGuardianServiceConfigurator = new Mock<IGuardianServiceConfigurator>();
        var mockBackupService = new Mock<IBackupService>();
        var mockMultiFileSystemWatcherService = new Mock<IMultiFileSystemWatcherService>();
        var sut = new GuardianService(
            Mock.Of<ILogger<GuardianService>>(),
            mockGuardianServiceConfigurator.Object,
            mockBackupService.Object,
            mockMultiFileSystemWatcherService.Object);

        var attempts = 0;

        var versionFolder = new VersionFolder
        {
            Path = "c:/somefolder/",
            ChangeGracePeriod = TimeSpan.FromSeconds(5)
        };
        var eventArgs = new MultiFileSystemWatcherServiceChangeOccurredEventArgs(
            versionFolder,
            "c:/somefolder/subfolder/file.ext");

        mockBackupService.Setup(x => x.ProcessAsync(
                It.IsAny<VersionFolder>(),
                It.IsAny<string>()))
            .Callback((VersionFolder versionFolder, string path) =>
            {
                attempts++;
            })
            .ReturnsAsync(() =>
            {
                return attempts == 2;
            });

        // Act
        mockMultiFileSystemWatcherService.Raise(x => x.ChangeOccurred += null,
            mockMultiFileSystemWatcherService.Object,
            eventArgs);

        await Task.Delay(TimeSpan.FromSeconds(6));
        await Task.Delay(TimeSpan.FromSeconds(6));
        await Task.Delay(TimeSpan.FromSeconds(6));

        // Assert
        mockBackupService.Verify(x => x.ProcessAsync(
            It.Is<VersionFolder>(y => y == eventArgs.VersionFolder),
            It.Is<string>(y => y == eventArgs.FullPath)), Times.Exactly(2));
    }
}
