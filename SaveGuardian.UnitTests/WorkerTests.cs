using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using SaveGuardian.Services;

namespace SaveGuardian.UnitTests;

public class WorkerTests
{
    [Fact]
    public void GivenCancellationToken_WhenStartAsync_ThenInitialiseAsync_AndSetupWatchers_AndLoopUntilIsCancellationRequested()
    {
        // Arrange
        var mockGuardianService = new Mock<IGuardianService>();
        var sut = new Worker(
            Mock.Of<ILogger<Worker>>(),
            mockGuardianService.Object);

        mockGuardianService.Setup(x => x.InitialiseAsync(
            It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        // Act
        sut.StartAsync(token);
        cancellationTokenSource.Cancel();

        // Assert
        mockGuardianService.Verify(x => x.InitialiseAsync(
            It.IsAny<CancellationToken>()), Times.Once);
        mockGuardianService.Verify(x => x.SetupWatchers(), Times.Once);
    }

    [Fact]
    public void GivenCancellationToken_WhenStartAsync_ThenInitialiseAsyncFailed_AndAborted()
    {
        // Arrange
        var mockGuardianService = new Mock<IGuardianService>();
        var sut = new Worker(
            Mock.Of<ILogger<Worker>>(),
            mockGuardianService.Object);

        mockGuardianService.Setup(x => x.InitialiseAsync(
            It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        // Act
        sut.StartAsync(token);
        cancellationTokenSource.Cancel();

        // Assert
        mockGuardianService.Verify(x => x.InitialiseAsync(
            It.IsAny<CancellationToken>()), Times.Once);
        mockGuardianService.Verify(x => x.SetupWatchers(), Times.Never);
    }
}
