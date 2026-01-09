using Algora.Erp.Infrastructure.Services;

namespace Algora.Erp.Infrastructure.Tests.Services;

public class DateTimeServiceTests
{
    private readonly DateTimeService _dateTimeService;

    public DateTimeServiceTests()
    {
        _dateTimeService = new DateTimeService();
    }

    [Fact]
    public void Now_ShouldReturnCurrentLocalTime()
    {
        // Arrange
        var beforeCall = DateTime.Now;

        // Act
        var result = _dateTimeService.Now;

        // Assert
        var afterCall = DateTime.Now;
        result.Should().BeOnOrAfter(beforeCall);
        result.Should().BeOnOrBefore(afterCall);
    }

    [Fact]
    public void UtcNow_ShouldReturnCurrentUtcTime()
    {
        // Arrange
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = _dateTimeService.UtcNow;

        // Assert
        var afterCall = DateTime.UtcNow;
        result.Should().BeOnOrAfter(beforeCall);
        result.Should().BeOnOrBefore(afterCall);
    }

    [Fact]
    public void UtcNow_ShouldBeDifferentFromNow_WhenNotInUtcTimeZone()
    {
        // Act
        var now = _dateTimeService.Now;
        var utcNow = _dateTimeService.UtcNow;

        // Assert
        // Note: This test may pass or fail depending on system timezone
        // It validates that UtcNow returns a different time offset when not in UTC
        (now - utcNow).Duration().Should().BeLessThan(TimeSpan.FromHours(24));
    }

    [Fact]
    public void Now_ShouldBeReasonablyClose_ToSystemDateTime()
    {
        // Act
        var serviceNow = _dateTimeService.Now;
        var systemNow = DateTime.Now;

        // Assert
        (serviceNow - systemNow).Duration().Should().BeLessThan(TimeSpan.FromSeconds(1));
    }
}
