using Moq;
using SaveGuardian.Model;
using SaveGuardian.Services;

namespace SaveGuardian.UnitTests.Services
{
    public class BackupFileNamingServiceTests
    {
        [Theory]
        [InlineData(
            "10-30-05 01/01/2023",
            "c:\\somefolder\\somesubfolder\\file.ext",
            "bak",
            "C:/Users/nickp/AppData/Local/SaveVersioningPoc/Some Game/somesubfolder/file.ext_01012023-103005.bak")]
        [InlineData(
            "10-30-05 01/01/2023",
            "c:\\somefolder\\file.ext",
            "bak",
            "C:/Users/nickp/AppData/Local/SaveVersioningPoc/Some Game/file.ext_01012023-103005.bak")]
        public void GivenVersionFolder_AndFullPath_AndExtension_WhenRename_ThenExpectedFileNameReturned(
            string dateTime,
            string fullPath,
            string extension,
            string expectedResult)
        {
            // Arrange
            var mockDateTimeService = new Mock<IDateTimeService>();
            var versionFolder = new VersionFolder
            {
                Name = "Some Game",
                Path = "c:\\somefolder\\"
            };
            var sut = new BackupFileNamingService(mockDateTimeService.Object);

            mockDateTimeService.Setup(x => x.Now).Returns(DateTime.ParseExact(
                dateTime,
                "HH-mmm-ss dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture));

            // Act
            var result = sut.Rename(
                versionFolder,
                fullPath,
                extension);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
