// ReSharper disable once CheckNamespace

namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup
{
    using Mango.Auditing.Telemetry;
    using Moq;
    using System.Diagnostics;

    /// <summary>
    /// Factory class for creating a pre-configured Moq instance of IActivityLogger.
    /// </summary>
    public static class ActivityLoggerMockFactory
    {
        public static Mock<IActivityLogger> WithGenericTracing<T>(T result)
        {
            var mock = Create();

            mock.Setup(x => x.TraceScope(It.IsAny<string>(), It.IsAny<Func<T>>()))
                .Returns((string name, Func<T> func) => func());

            mock.Setup(x => x.TraceScopeAsync(It.IsAny<string>(), It.IsAny<Func<Task<T>>>()))
                .Returns((string name, Func<Task<T>> func) => func());

            return mock;
        }

        public static Mock<IActivityLogger> Create()
        {
            var mock = new Mock<IActivityLogger>();

            // Mock TraceScope<T>
            mock.Setup(x => x.TraceScope(It.IsAny<string>(), It.IsAny<Func<object>>()))
                .Returns((string name, Func<object> func) => func());

            // Mock TraceScopeAsync<T>
            mock.Setup(x => x.TraceScopeAsync(It.IsAny<string>(), It.IsAny<Func<Task<object>>>()))
                .Returns((string name, Func<Task<object>> func) => func());

            // Mock StartActivity — return dummy IDisposable
            mock.Setup(x => x.StartActivity(It.IsAny<string>(), It.IsAny<ActivityKind>(), It.IsAny<IDictionary<string, object?>>()))
                .Returns(() => new Mock<IDisposable>().Object);

            // Mock RecordEvent, RecordException, SetBaggage — just NoOps
            mock.Setup(x => x.RecordEvent(It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()));
            mock.Setup(x => x.RecordException(It.IsAny<Exception>(), It.IsAny<IDictionary<string, object?>>()));
            mock.Setup(x => x.SetBaggage(It.IsAny<IDictionary<string, string>>()));

            // Mock GetCurrentActivity — returns dummy Activity or null
            mock.Setup(x => x.GetCurrentActivity())
                .Returns(() => Activity.Current);

            // Mock TraceId & SpanId
            mock.Setup(x => x.GetCurrentTraceId()).Returns(() => Activity.Current?.TraceId.ToString());
            mock.Setup(x => x.GetCurrentSpanId()).Returns(() => Activity.Current?.SpanId.ToString());

            // No-op log context
            mock.Setup(x => x.LogCurrentContext());

            return mock;
        }
    }
}