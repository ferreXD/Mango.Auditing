// ReSharper disable once CheckNamespace
namespace EntityFrameworkCore.Auditing.Tests.UnitTests
{
    using Mango.Auditing;
    using Mango.Auditing.Logging;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;

    public class LogLevelMappingExtensions
    {
        [Theory]
        [InlineData(LogLevel.Information, AuditLevel.Information)]
        [InlineData(LogLevel.Debug, AuditLevel.Debug)]
        [InlineData(LogLevel.Warning, AuditLevel.Warning)]
        [InlineData(LogLevel.Error, AuditLevel.Error)]
        [InlineData(LogLevel.Critical, AuditLevel.Critical)]
        public void ToAuditLevel_Should_MapCorrectly(LogLevel logLevel, AuditLevel expected)
        {
            // Act
            var result = logLevel.ToAuditLevel();

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void ToAuditLevel_Should_Throw_When_Invalid()
        {
            // Arrange
            var invalid = (LogLevel)123456;

            // Act
            var act = () => invalid.ToAuditLevel();

            // Assert
            act.Should().Throw<UnhandledEnumValueException<LogLevel>>();
        }

        [Theory]
        [InlineData(AuditLevel.Information, LogLevel.Information)]
        [InlineData(AuditLevel.Debug, LogLevel.Debug)]
        [InlineData(AuditLevel.Warning, LogLevel.Warning)]
        [InlineData(AuditLevel.Error, LogLevel.Error)]
        [InlineData(AuditLevel.Critical, LogLevel.Critical)]
        public void ToLogLevel_Should_MapCorrectly(AuditLevel logLevel, LogLevel expected)
        {
            // Act
            var result = logLevel.ToLogLevel();

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void ToLogLevel_Should_Throw_When_Invalid()
        {
            // Arrange
            var invalid = (AuditLevel)123456;

            // Act
            var act = () => invalid.ToLogLevel();

            // Assert
            act.Should().Throw<UnhandledEnumValueException<AuditLevel>>();
        }
    }
}
