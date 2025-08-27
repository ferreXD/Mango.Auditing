// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System.Collections.Generic;

    public class ThresholdSamplingStrategy(TelemetryOptions options, double threshold) : BaseSampleStrategy(options), IMetricSamplingStrategy
    {
        public override bool ShouldSample(string _, double value, MetricType ___, IDictionary<string, object>? ____) => value >= threshold;
    }
}
