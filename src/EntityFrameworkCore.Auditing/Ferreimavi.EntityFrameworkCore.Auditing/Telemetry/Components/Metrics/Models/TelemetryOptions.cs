// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System.Collections.Generic;

    public class TelemetryOptions
    {
        public bool MetricsEnabled { get; set; } = true;

        /// <summary>
        /// Global max samples per second for *any* metric (if no rule is defined).
        /// </summary>
        public int MaxGlobalSamplesPerSecond { get; set; } = 1000;

        /// <summary>
        /// Per-metric overrides. Each key is the metric name.
        /// </summary>
        public Dictionary<string, MetricSamplingRule> PerMetricRules { get; } = new();

        /// <summary>
        /// Optional global prefix for all metrics.
        /// </summary>
        public string MetricNamePrefix { get; set; } = string.Empty;

        /// <summary>
        /// Whether to emit internal diagnostics (e.g. dropped metrics).
        /// </summary>
        public bool EnableDiagnostics { get; set; } = false;

        /// <summary>
        /// Optional fallback strategy when a metric has no rule defined.
        /// </summary>
        public IMetricSamplingStrategy? DefaultStrategy { get; set; }
    }

}
