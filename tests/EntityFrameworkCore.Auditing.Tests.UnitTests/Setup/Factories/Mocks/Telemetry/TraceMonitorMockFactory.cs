namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup.Factories.Mocks.Telemetry
{
    using Mango.Auditing.Telemetry;
    using Moq;
    using System.Diagnostics;

    public class TraceMonitorMockFactory
    {
        public static Mock<ITraceMonitor> Create()
        {
            var mock = new Mock<ITraceMonitor>();

            var disposableMock = new Mock<IDisposable>();
            disposableMock.Setup(x => x.Dispose()).Callback(() => Console.WriteLine("Disposed"));

            // BeginOperation (sync) — return dummy disposable
            mock.Setup(x => x.BeginOperation(
                    It.IsAny<string>(),
                    It.IsAny<ActivityKind>(),
                    It.IsAny<IDictionary<string, object>>()))
                .Returns(() => disposableMock.Object);

            // BeginOperationAsync — return dummy disposable wrapped in Task
            mock.Setup(x => x.BeginOperationAsync(
                    It.IsAny<string>(),
                    It.IsAny<ActivityKind>(),
                    It.IsAny<IDictionary<string, object>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(disposableMock.Object);

            // RecordEvent, RecordException, SetBaggage — NoOp by default
            mock.Setup(x => x.RecordEvent(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()));
            mock.Setup(x => x.RecordException(It.IsAny<Exception>(), It.IsAny<IDictionary<string, object>>()));
            mock.Setup(x => x.SetBaggage(It.IsAny<IDictionary<string, string>>()));

            // GetCurrentTraceId / SpanId — return whatever Activity.Current provides (or null)
            mock.Setup(x => x.GetCurrentTraceId())
                .Returns(() => Activity.Current?.TraceId.ToString());

            mock.Setup(x => x.GetCurrentSpanId())
                .Returns(() => Activity.Current?.SpanId.ToString());

            return mock;
        }
    }
}
