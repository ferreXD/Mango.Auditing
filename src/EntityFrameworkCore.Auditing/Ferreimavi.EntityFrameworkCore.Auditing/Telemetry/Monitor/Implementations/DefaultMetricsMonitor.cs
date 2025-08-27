// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    public class DefaultMetricsMonitor(IRootMetricProvider metricProvider, IPerformanceContextProvider context) : IMetricsMonitor
    {
        private readonly IRootMetricProvider _metricProvider = metricProvider ?? throw new ArgumentNullException(nameof(metricProvider));

        /// <inheritdoc />
        public void Record(string metricName, double value, MetricType metricType = MetricType.Histogram, IDictionary<string, object>? tags = null) => _metricProvider.RecordMeasurement(metricName, value, metricType, context.GetTags().Merge(tags));

        public AggregatedMetrics? Get(string name, MetricType type, TimeSpan window) => _metricProvider.GetMetric(name, type, window);

        /// <inheritdoc />
        public IDictionary<MetricKey, AggregatedMetrics> Aggregate(TimeSpan timeWindow) => _metricProvider.GetAggregatedMetrics(timeWindow);

        /// <inheritdoc />
        public void Reset(string? metricName = null, MetricType metricType = MetricType.Histogram)
        {
            if (string.IsNullOrEmpty(metricName))
            {
                _metricProvider.Clear();
                return;
            }

            _metricProvider.Clear(metricName, metricType);
        }
    }
}
