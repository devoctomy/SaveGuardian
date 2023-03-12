using SaveGuardian.Services;

namespace SaveGuardian.UnitTests.Services;

public class DateTimeServiceTests
{
    [Fact]
    public void GivenInstance_WhenNow_ThenNowReturned()
    {
        // Arrange
        var sut = new DateTimeService();

        // Act
        var before = DateTime.Now;
        var result = sut.Now;
        var after = DateTime.Now;

        // Assert
        Assert.True(before < result);
        Assert.True(after > result);
    }
}
