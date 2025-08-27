namespace EntityFrameworkCore.Auditing.Tests.UnitTests
{
    using Mango.Auditing;
    using FluentAssertions;
    using System;

    public class CronExpressionConverterTests
    {
        [Theory]
        [InlineData(1, "*/1 * * * *")]    // exactly 1 minute
        [InlineData(15, "*/15 * * * *")]   // quarter‐hour
        [InlineData(59, "*/59 * * * *")]   // just under an hour
        public void ToCronExpression_MinuteIntervals(int minutes, string expectedCron)
        {
            // Arrange
            var ts = TimeSpan.FromMinutes(minutes);

            // Act
            var cron = ts.ToCronExpression();

            // Assert
            cron.Should().Be(expectedCron);
        }

        [Theory]
        [InlineData(60, "0 */1 * * *")]    // exactly 1 hour
        [InlineData(120, "0 */2 * * *")]    // two hours
        [InlineData(23 * 60, "0 */23 * * *")]   // 23 hours in minutes
        public void ToCronExpression_HourIntervals(int totalMinutes, string expectedCron)
        {
            // Arrange
            var ts = TimeSpan.FromMinutes(totalMinutes);

            // Act
            var cron = ts.ToCronExpression();

            // Assert
            cron.Should().Be(expectedCron);
        }

        [Theory]
        [InlineData(24, "0 0 */1 * *")]    // exactly 1 day
        [InlineData(48, "0 0 */2 * *")]    // two days
        [InlineData(365 * 24, "0 0 */365 * *")]  // many days
        public void ToCronExpression_DayIntervals(int totalHours, string expectedCron)
        {
            // Arrange
            var ts = TimeSpan.FromHours(totalHours);

            // Act
            var cron = ts.ToCronExpression();

            // Assert
            cron.Should().Be(expectedCron);
        }

        [Fact]
        public void ToCronExpression_LessThanOneMinute_Throws()
        {
            // Arrange
            var ts = TimeSpan.FromSeconds(59);

            // Act
            Action act = () => ts.ToCronExpression();

            // Assert
            act.Should()
               .Throw<ArgumentException>()
               .WithMessage("Interval must be at least 1 minute*")
               .And
               .ParamName.Should().Be("interval");
        }

        [Fact]
        public void ToCronExpression_Negative_Throws()
        {
            // Arrange
            var ts = TimeSpan.FromMinutes(-5);

            // Act
            Action act = () => ts.ToCronExpression();

            // Assert
            act.Should()
               .Throw<ArgumentException>()
               .WithMessage("Interval must be at least 1 minute*")
               .And
               .ParamName.Should().Be("interval");
        }

        [Theory]
        [InlineData(90, "0 */1,5 * * *")]   // 1.5 hours
        [InlineData(60 * 24, "0 0 */2,5 * *")]   // 2.5 days (converted via TotalDays)
        public void ToCronExpression_FractionalIntervals(double totalUnits, string expectedCron)
        {
            // Arrange
            // First: interpret as hours; second: interpret as days
            var ts = totalUnits == 90
                ? TimeSpan.FromMinutes(totalUnits)
                : TimeSpan.FromHours(totalUnits / 24);

            // Act
            var cron = ts.ToCronExpression();

            // Assert
            cron.Should().Be(expectedCron);
        }
    }
}
