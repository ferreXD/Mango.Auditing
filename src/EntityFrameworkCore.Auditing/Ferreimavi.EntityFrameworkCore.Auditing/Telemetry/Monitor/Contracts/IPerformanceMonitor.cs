// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Telemetry
{
    using System.Diagnostics;

    /// <summary>
    /// Provides a high-level interface for performance monitoring that aggregates tracing, metrics,
    /// and telemetry ingestion functionalities.
    /// </summary>
    public interface IPerformanceMonitor
    {
        #region Tracing

        IDisposable BeginOperation(string name, ActivityKind kind = ActivityKind.Internal, IDictionary<string, object>? tags = null);
        Task<IDisposable> BeginOperationAsync(string name, ActivityKind kind = ActivityKind.Internal, IDictionary<string, object>? tags = null, CancellationToken cancellationToken = default);
        void RecordEvent(string message, IDictionary<string, object>? properties = null);
        void RecordException(Exception ex, IDictionary<string, object>? tags = null);
        void SetBaggage(IDictionary<string, string> items);
        string? GetCurrentTraceId();
        string? GetCurrentSpanId();

        #endregion

        #region Metrics

        void RecordMetric(string name, double value, MetricType type = MetricType.Histogram, IDictionary<string, object>? tags = null);
        AggregatedMetrics? GetMetrics(string name, MetricType type, TimeSpan window);
        IDictionary<MetricKey, AggregatedMetrics> GetAggregatedMetrics(TimeSpan window);
        void ResetMetrics(string? name = null, MetricType type = MetricType.Histogram);

        #endregion

        #region Logging

        void Verbose(string message, params (string Key, object? Value)[] properties);
        void Info(string message, params (string Key, object? Value)[] properties);
        void Debug(string message, params (string Key, object? Value)[] properties);
        void Warn(string message, params (string Key, object? Value)[] properties);
        void Error(string message, Exception ex, params (string Key, object? Value)[] properties);

        #endregion
    }
}