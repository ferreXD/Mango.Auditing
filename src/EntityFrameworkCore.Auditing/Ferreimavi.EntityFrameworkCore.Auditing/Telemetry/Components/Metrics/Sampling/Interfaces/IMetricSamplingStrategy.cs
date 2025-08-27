// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    public interface IMetricSamplingStrategy
    {
        bool ShouldSample(string metricName, double value, MetricType type, IDictionary<string, object>? tags);
        bool IsEnabled(string metricName);
    }
}
