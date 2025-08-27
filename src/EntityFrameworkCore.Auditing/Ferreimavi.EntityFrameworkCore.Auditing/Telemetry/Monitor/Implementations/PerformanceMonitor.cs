namespace Mango.Auditing.Telemetry
{
    using System.Diagnostics;

    /// <summary>
    /// A default implementation of IPerformanceMonitor that delegates tracing to IActivityLogger
    /// and metric recording to IMetricProvider.
    /// </summary>
    public class PerformanceMonitor(
        ITraceMonitor trace,
        IMetricsMonitor metrics,
        ILogMonitor logs,
        IPerformanceContextProvider contextProvider) : IPerformanceMonitor
    {
        #region Tracing

        public IDisposable BeginOperation(string name, ActivityKind kind = ActivityKind.Internal, IDictionary<string, object>? tags = null)
            => trace.BeginOperation(name, kind, contextProvider.GetTags().Merge(tags));

        public async Task<IDisposable> BeginOperationAsync(string name, ActivityKind kind = ActivityKind.Internal, IDictionary<string, object>? tags = null, CancellationToken cancellationToken = default)
            => await trace.BeginOperationAsync(name, kind, tags, cancellationToken);

        public void RecordEvent(string message, IDictionary<string, object>? props = null)
            => trace.RecordEvent(message, contextProvider.GetTags().Merge(props));

        public void RecordException(Exception ex, IDictionary<string, object>? tags = null)
            => trace.RecordException(ex, contextProvider.GetTags().Merge(tags));

        public void SetBaggage(IDictionary<string, string> items)
            => trace.SetBaggage(items);

        public string? GetCurrentTraceId() => trace.GetCurrentTraceId();

        public string? GetCurrentSpanId() => trace.GetCurrentSpanId();

        #endregion

        #region Metrics

        public void RecordMetric(string name, double value, MetricType type = MetricType.Histogram, IDictionary<string, object>? tags = null)
            => metrics.Record(name, value, type, contextProvider.GetTags().Merge(tags));

        public IDictionary<MetricKey, AggregatedMetrics> GetAggregatedMetrics(TimeSpan window)
            => metrics.Aggregate(window);

        public AggregatedMetrics? GetMetrics(string name, MetricType type, TimeSpan window)
            => metrics.Get(name, type, window);

        public void ResetMetrics(string? name = null, MetricType type = MetricType.Histogram)
            => metrics.Reset(name, type);

        #endregion

        #region Logging

        public void Verbose(string message, params (string Key, object? Value)[] properties)
            => logs.Verbose(message, properties);

        public void Info(string message, params (string Key, object? Value)[] properties)
            => logs.Info(message, properties);

        public void Warn(string message, params (string Key, object? Value)[] properties)
            => logs.Warn(message, properties);

        public void Debug(string message, params (string Key, object? Value)[] properties)
            => logs.Debug(message, properties);

        public void Error(string message, Exception ex, params (string Key, object? Value)[] properties)
            => logs.Error(message, ex, properties);

        #endregion
    }
}