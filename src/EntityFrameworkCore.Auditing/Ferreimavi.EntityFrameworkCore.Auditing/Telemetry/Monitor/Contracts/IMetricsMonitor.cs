// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    public interface IMetricsMonitor
    {
        void Record(string name, double value, MetricType type = MetricType.Histogram, IDictionary<string, object>? tags = null);
        AggregatedMetrics? Get(string name, MetricType type, TimeSpan window);
        IDictionary<MetricKey, AggregatedMetrics> Aggregate(TimeSpan window);
        void Reset(string? name = null, MetricType type = MetricType.Histogram);
    }
}
