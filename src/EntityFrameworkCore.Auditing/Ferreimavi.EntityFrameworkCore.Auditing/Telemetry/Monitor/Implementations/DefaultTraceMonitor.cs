// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System.Diagnostics;

    public class DefaultTraceMonitor(IActivityLogger activityLogger, IPerformanceContextProvider context) : ITraceMonitor
    {
        private readonly IActivityLogger _activityLogger = activityLogger ?? throw new ArgumentNullException(nameof(activityLogger));

        /// <inheritdoc />
        public IDisposable BeginOperation(string operationName, ActivityKind activityKind = ActivityKind.Internal, IDictionary<string, object>? tags = null)
            => _activityLogger.StartActivity(operationName, activityKind, context.GetTags().Merge(tags)!);

        public Task<IDisposable> BeginOperationAsync(string name, ActivityKind kind = ActivityKind.Internal, IDictionary<string, object>? tags = null, CancellationToken cancellationToken = default)
            => Task.FromResult(BeginOperation(name, kind, tags));

        /// <inheritdoc />
        public void RecordEvent(string message, IDictionary<string, object>? properties = null)
            => _activityLogger.RecordEvent(message, context.GetTags().Merge(properties)!);

        public void RecordException(Exception ex, IDictionary<string, object>? tags = null)
            => _activityLogger.RecordException(ex, context.GetTags().Merge(tags)!);

        public void SetBaggage(IDictionary<string, string> items)
            => _activityLogger.SetBaggage(items);

        public string? GetCurrentTraceId()
            => activityLogger.GetCurrentTraceId();

        public string? GetCurrentSpanId()
            => activityLogger.GetCurrentSpanId();
    }
}
