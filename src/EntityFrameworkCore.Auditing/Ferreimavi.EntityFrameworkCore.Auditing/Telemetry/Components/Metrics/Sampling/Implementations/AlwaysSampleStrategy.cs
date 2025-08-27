// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System.Collections.Generic;

    public class AlwaysSampleStrategy(TelemetryOptions options) : BaseSampleStrategy(options), IMetricSamplingStrategy
    {
        public override bool ShouldSample(string _, double __, MetricType ___, IDictionary<string, object>? ____) => true;
    }
}
