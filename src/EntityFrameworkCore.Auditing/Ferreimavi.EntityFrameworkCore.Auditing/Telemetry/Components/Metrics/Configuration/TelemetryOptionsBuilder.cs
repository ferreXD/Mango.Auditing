// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System;

    public class TelemetryOptionsBuilder
    {
        private readonly TelemetryOptions _options = new();

        public TelemetryOptionsBuilder EnableMetrics(bool enable = true)
        {
            _options.MetricsEnabled = enable;
            return this;
        }

        public TelemetryOptionsBuilder EnableDiagnostics(bool enable = true)
        {
            _options.EnableDiagnostics = enable;
            return this;
        }

        public TelemetryOptionsBuilder SetGlobalRateLimit(int maxPerSecond)
        {
            _options.MaxGlobalSamplesPerSecond = maxPerSecond;
            return this;
        }

        public TelemetryOptionsBuilder PrefixMetricsWith(string prefix)
        {
            _options.MetricNamePrefix = prefix;
            return this;
        }

        public TelemetryOptionsBuilder SetDefaultSamplingStrategy(IMetricSamplingStrategy strategy)
        {
            _options.DefaultStrategy = strategy;
            return this;
        }

        public TelemetryOptionsBuilder ForMetric(string metricName, Action<MetricRuleBuilder> configureRule)
        {
            var ruleBuilder = new MetricRuleBuilder(metricName);
            configureRule(ruleBuilder);
            _options.PerMetricRules[metricName] = ruleBuilder.Build();
            return this;
        }

        public TelemetryOptions Build() => _options;
    }
}
