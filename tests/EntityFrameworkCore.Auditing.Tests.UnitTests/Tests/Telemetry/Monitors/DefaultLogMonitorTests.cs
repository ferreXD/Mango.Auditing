// ReSharper disable once CheckNamespace
namespace EntityFrameworkCore.Auditing.Tests.UnitTests
{
    using Mango.Auditing.Telemetry;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Setup.Factories.Mocks.Telemetry;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DefaultLogMonitorTests
    {
        private readonly Mock<ITelemetryLogger> _telemetryLogger;
        private readonly Mock<IPerformanceContextProvider> _contextProvider;
        private readonly DefaultLogMonitor _sut;

        public DefaultLogMonitorTests()
        {
            _telemetryLogger = new Mock<ITelemetryLogger>(MockBehavior.Strict);
            _contextProvider = PerformanceContextProviderMockFactory.Create();
            _sut = new DefaultLogMonitor(_telemetryLogger.Object, _contextProvider.Object);
        }

        public static IEnumerable<object[]> LogMethods =>
            new[]
            {
                new object[] { "msg1", LogLevel.Trace,  (Action<DefaultLogMonitor, string, (string, object?)[]>)((s,m,p) => s.Verbose(m,p)) },
                new object[] { "msg2", LogLevel.Debug,  (Action<DefaultLogMonitor, string, (string, object?)[]>)((s,m,p) => s.Debug(m,p)) },
                new object[] { "msg3", LogLevel.Information, (Action<DefaultLogMonitor, string, (string, object?)[]>)((s,m,p) => s.Info(m,p)) },
                new object[] { "msg4", LogLevel.Warning, (Action<DefaultLogMonitor, string, (string, object?)[]>)((s,m,p) => s.Warn(m,p)) },
            };

        [Theory]
        [MemberData(nameof(LogMethods))]
        public void LogLevelMethods_Should_MergeContextAndCallUnderlyingLog(
            string message,
            LogLevel expectedLevel,
            Action<DefaultLogMonitor, string, (string, object?)[]> invoke)
        {
            // Arrange: set up context tags
            var ctxTags = new Dictionary<string, object> { ["ctxKey"] = 99 };
            _contextProvider.Object.SetTags(ctxTags);

            // Extra props
            var extra = new (string, object?)[] { ("foo", "bar") };

            // Expectation: underlying .Log(level, message, mergedProps)
            _telemetryLogger
                .Setup(log => log.Log(
                    expectedLevel,
                    message,
                    It.Is<(string Key, object? Value)[]>(
                        arr => arr.Length == 2
                            && arr.Any(kv => kv.Key == "ctxKey" && (int)kv.Value! == 99)
                            && arr.Any(kv => kv.Key == "foo" && (string)kv.Value! == "bar")
                    )
                ))
                .Verifiable();

            // Act
            invoke(_sut, message, extra);

            // Assert
            _telemetryLogger.Verify();
        }

        [Fact]
        public void LogMethods_WithNoExtraProps_Should_PassOnlyContextTags()
        {
            // Arrange
            var ctxTags = new Dictionary<string, object> { ["only"] = "ctx" };
            _contextProvider.Object.SetTags(ctxTags);

            _telemetryLogger
                .Setup(log => log.Log(
                    LogLevel.Information,
                    "hello",
                    It.Is<(string Key, object? Value)[]>(arr =>
                        arr.Length == 1
                        && arr[0].Key == "only"
                        && (string)arr[0].Value! == "ctx"
                    )
                ))
                .Verifiable();

            // Act
            _sut.Info("hello");

            // Assert
            _telemetryLogger.Verify();
        }

        [Fact]
        public void Error_Should_MergeContextAndCallLogError()
        {
            // Arrange
            var ctxTags = new Dictionary<string, object> { ["user"] = "bob" };
            _contextProvider.Object.SetTags(ctxTags);

            var ex = new InvalidOperationException("fail");
            var extra = new (string, object?)[] { ("x", 1) };

            _telemetryLogger
                .Setup(log => log.LogError(
                    ex,
                    "oops",
                    It.Is<(string Key, object? Value)[]>(
                        arr => arr.Length == 2
                            && arr.Any(kv => kv.Key == "user" && (string)kv.Value! == "bob")
                            && arr.Any(kv => kv.Key == "x" && (int)kv.Value! == 1)
                    )
                ))
                .Verifiable();

            // Act
            _sut.Error("oops", ex, extra);

            // Assert
            _telemetryLogger.Verify();
        }

        [Fact]
        public void Error_WithNoExtraProps_Should_PassOnlyContextTags()
        {
            // Arrange
            var ctxTags = new Dictionary<string, object> { ["uid"] = 123 };
            _contextProvider.Object.SetTags(ctxTags);

            var ex = new Exception("boom");

            _telemetryLogger
                .Setup(log => log.LogError(
                    ex,
                    "boom",
                    It.Is<(string Key, object? Value)[]>(arr =>
                        arr.Length == 1
                        && arr[0].Key == "uid"
                        && (int)arr[0].Value! == 123
                    )
                ))
                .Verifiable();

            // Act
            _sut.Error("boom", ex);

            // Assert
            _telemetryLogger.Verify();
        }
    }
}
