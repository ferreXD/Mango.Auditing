// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System.Collections.Generic;

    public abstract class BaseSampleStrategy(TelemetryOptions options) : IMetricSamplingStrategy
    {
        protected TelemetryOptions Options { get; } = options;

        public abstract bool ShouldSample(string metricName, double value, MetricType type, IDictionary<string, object>? tags);

        public bool IsEnabled(string metricName) => Options.MetricsEnabled && (!Options.PerMetricRules.TryGetValue(metricName, out var rule) || rule.IsEnabled);
    }
}
