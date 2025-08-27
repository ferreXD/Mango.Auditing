// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System.Diagnostics;

    public interface ITraceMonitor
    {
        IDisposable BeginOperation(string name, ActivityKind kind = ActivityKind.Internal, IDictionary<string, object>? tags = null);
        Task<IDisposable> BeginOperationAsync(string name, ActivityKind kind = ActivityKind.Internal, IDictionary<string, object>? tags = null, CancellationToken cancellationToken = default);
        void RecordEvent(string message, IDictionary<string, object>? properties = null);
        void RecordException(Exception ex, IDictionary<string, object>? tags = null);
        void SetBaggage(IDictionary<string, string> items);
        string? GetCurrentTraceId();
        string? GetCurrentSpanId();
    }
}
