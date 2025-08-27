namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup.Serilog.Sinks
{
    using global::Serilog.Core;
    using global::Serilog.Events;

    public class CustomInMemorySink : ILogEventSink
    {
        // Store log events in a collection for inspection.
        public List<LogEvent> LogEvents { get; } = new();

        // This method is called for every log event.
        public void Emit(LogEvent logEvent)
        {
            LogEvents.Add(logEvent);
        }
    }
}