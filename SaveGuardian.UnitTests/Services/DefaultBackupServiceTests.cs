using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using SaveGuardian.Model;
using SaveGuardian.Services;

namespace SaveGuardian.UnitTests.Services;

public class DefaultBackupServiceTests
{
    [Fact]
    public void GivenVersionFolder_AndPath_AndNoProcessesRunning_WhenProcess_ThenCheckProcessesNotRunning_AndCreateBackupFileName_AndCreateDirectory_AndCopyFile_AndReturnTrue()
    {
        // Arrange
        var mockBackupFileNamingService = new Mock<IBackupFileNamingService>();
        var mockIOService = new Mock<IIOService>();
        var mockProcessService = new Mock<IProcessService>();
        var sut = new DefaultBackupService(
            Mock.Of<ILogger<DefaultBackupService>>(),
            mockBackupFileNamingService.Object,
            mockIOService.Object,
            mockProcessService.Object);

        var versionFolder = new VersionFolder
        {
            Name = "Hello World",
            BackupBlockingProcesses = new List<string>
            {
                "Apple",
                "Orange"
            }
        };
        var path = "c:/folder1/folder2/file.ext";
        var backupFile = "c:/bob/backups/file.bak";

        mockProcessService.Setup(x => x.IsRunning(
            It.IsAny<List<string>>()))
            .Returns(false);

        mockBackupFileNamingService.Setup(x => x.Rename(
            It.IsAny<VersionFolder>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns(backupFile);

        // Act
        var result = sut.Process(
            versionFolder,
            path);

        // Assert
        Assert.True(result);
        mockProcessService.Verify(x => x.IsRunning(
            It.Is<List<string>>(x => x.All(y => versionFolder.BackupBlockingProcesses.Contains(y)))), Times.Once);
        mockBackupFileNamingService.Verify(x => x.Rename(
            It.Is<VersionFolder>(y => y == versionFolder),
            It.Is<string>(y => y == path),
            It.Is<string>(y => y == "bak")), Times.Once);
        mockIOService.Verify(x => x.CreateDirectory(
            It.Is<string>(y => y == "c:/bob/backups")), Times.Once);
        mockIOService.Verify(x => x.CopyFile(
            It.Is<string>(y => y == path),
            It.Is<string>(y => y == backupFile),
            It.Is<bool>(y => y == true)), Times.Once);
    }

    [Fact]
    public void GivenVersionFolder_AndPath_AndProcessesRunning_WhenProcess_ThenCheckProcessesRunning_AndReturnFalse()
    {
        // Arrange
        var mockBackupFileNamingService = new Mock<IBackupFileNamingService>();
        var mockIOService = new Mock<IIOService>();
        var mockProcessService = new Mock<IProcessService>();
        var sut = new DefaultBackupService(
            Mock.Of<ILogger<DefaultBackupService>>(),
            mockBackupFileNamingService.Object,
            mockIOService.Object,
            mockProcessService.Object);

        var versionFolder = new VersionFolder
        {
            Name = "Hello World",
            BackupBlockingProcesses = new List<string>
            {
                "Apple",
                "Orange"
            }
        };
        var path = "c:/folder1/folder2/file.ext";

        mockProcessService.Setup(x => x.IsRunning(
            It.IsAny<List<string>>()))
            .Returns(true);

        // Act
        var result = sut.Process(
            versionFolder,
            path);

        // Assert
        Assert.False(result);
        mockProcessService.Verify(x => x.IsRunning(
            It.Is<List<string>>(x => x.All(y => versionFolder.BackupBlockingProcesses.Contains(y)))), Times.Once);
    }
}
