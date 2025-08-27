// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    public class DefaultRootMetricProvider(IAggregatedMetricProvider aggregatedMetricProvider, IMetricSamplingStrategy samplingStrategy, TelemetryOptions options, string meterName = "DefaultMeter")
        : DefaultMetricProvider(aggregatedMetricProvider, samplingStrategy, options, meterName), IRootMetricProvider;
}
