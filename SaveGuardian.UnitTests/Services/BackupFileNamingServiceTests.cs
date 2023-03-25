using Moq;
using SaveGuardian.Model;
using SaveGuardian.Services;

namespace SaveGuardian.UnitTests.Services;

public class BackupFileNamingServiceTests
{
    [Theory]
    [InlineData(
        "10-30-05 01/01/2023",
        "Some Game",
        "c:\\somefolder\\",
        "c:\\somefolder\\somesubfolder\\file.ext",
        "bak",
        true,
        "C:\\Users\\nickp\\AppData\\Local",
        "C:/Users/nickp/AppData/Local/SaveGuardian/Some Game/somesubfolder/file.ext_01012023-103005.bak")]
    [InlineData(
        "10-30-05 01/01/2023",
        "Some Game",
        "c:\\somefolder\\",
        "c:\\somefolder\\file.ext",
        "bak",
        true,
        "C:\\Users\\nickp\\AppData\\Local",
        "C:/Users/nickp/AppData/Local/SaveGuardian/Some Game/file.ext_01012023-103005.bak")]
    [InlineData(
        "10-30-05 01/01/2023",
        "Some Game",
        "opt/someapp/",
        "opt/someapp/somefolder/file.ext",
        "bak",
        true,
        "home/.local/share",
        "home/.local/share/SaveGuardian/Some Game/somefolder/file.ext_01012023-103005.bak")]
    [InlineData(
        "10-30-05 01/01/2023",
        "Some Game",
        "opt/someapp/",
        "opt/someapp/file.ext",
        "bak",
        true,
        "home/.local/share",
        "home/.local/share/SaveGuardian/Some Game/file.ext_01012023-103005.bak")]
    [InlineData(
        "10-30-05 01/01/2023",
        "Some Game",
        "opt/someapp/",
        "opt/someapp/file.ext",
        "bak",
        false,
        "home/.local/share",
        "home/.local/share/SaveGuardian/Some Game/file.ext.bak")]
    public void GivenVersionFolder_AndFullPath_AndExtension_AndIncludeDate_WhenRename_ThenExpectedFileNameReturned(
        string dateTime,
        string versionFolderName,
        string versionFolderPath,
        string fullPath,
        string extension,
        bool includeDate,
        string localApplicationDataPath,
        string expectedResult)
    {
        // Arrange
        var mockDateTimeService = new Mock<IDateTimeService>();
        var mockSpecialFolderService = new Mock<ISpecialFolderService>();
        var versionFolder = new VersionFolder
        {
            Name = versionFolderName,
            Path = versionFolderPath
        };
        var sut = new BackupFileNamingService(
            mockDateTimeService.Object,
            mockSpecialFolderService.Object);

        mockDateTimeService.SetupGet(x => x.Now)
            .Returns(DateTime.ParseExact(
                dateTime,
                "HH-mmm-ss dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture));

        mockSpecialFolderService.SetupGet(x => x.LocalApplicationData)
            .Returns(localApplicationDataPath);

        // Act
        var result = sut.Rename(
            versionFolder,
            fullPath,
            extension,
            includeDate);

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
