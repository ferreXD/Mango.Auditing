// ReSharper disable once CheckNamespace

namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup
{
    using Mango.Auditing.Telemetry;
    using Microsoft.Extensions.Logging;
    using Moq;

    public static class TelemetryLoggerMockFactory
    {
        /// <summary>
        /// Creates and returns a new mock of ITelemetryLogger with default setups.
        /// </summary>
        /// <returns>A TelemetryLoggerMock containing the mock and captured log lists.</returns>
        public static TelemetryLoggerMock Create()
        {
            var mock = new Mock<ITelemetryLogger>();

            var recordedLogs = new List<(LogLevel, string, IDictionary<string, object?>)>();
            mock
                .Setup(m => m.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<string>(),
                    It.IsAny<(string Key, object? Value)[]>()))
                .Callback<LogLevel, string, (string Key, object? Value)[]>((logLevel, message, properties) =>
                {
                    var propsDict = properties.ToDictionary(p => p.Key, p => p.Value);
                    recordedLogs.Add((logLevel, message, propsDict));
                });

            var recordedErrors = new List<(Exception, string, IDictionary<string, object?>)>();
            mock
                .Setup(m => m.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<(string Key, object? Value)[]>()))
                .Callback<Exception, string, (string Key, object? Value)[]>((exception, message, properties) =>
                {
                    var propsDict = properties.ToDictionary(p => p.Key, p => p.Value);
                    recordedErrors.Add((exception, message, propsDict));
                });

            return new TelemetryLoggerMock(mock, recordedLogs, recordedErrors);
        }

        public class TelemetryLoggerMock(
            Mock<ITelemetryLogger> mock,
            List<(LogLevel, string, IDictionary<string, object?>)> logs,
            List<(Exception, string, IDictionary<string, object?>)> errors)
        {
            public Mock<ITelemetryLogger> Mock { get; } = mock;
            public List<(LogLevel LogLevel, string Message, IDictionary<string, object?> Properties)> RecordedLogs { get; } = logs;
            public List<(Exception Exception, string Message, IDictionary<string, object?> Properties)> RecordedErrors { get; } = errors;
        }
    }
}