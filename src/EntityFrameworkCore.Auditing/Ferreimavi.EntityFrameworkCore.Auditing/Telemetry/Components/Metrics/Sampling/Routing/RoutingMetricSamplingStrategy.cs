// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using Microsoft.Extensions.Logging;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class RoutingMetricSamplingStrategy(TelemetryOptions options, ILogger<RoutingMetricSamplingStrategy> logger) : BaseSampleStrategy(options), IMetricSamplingStrategy
    {
        private readonly ConcurrentDictionary<string, IMetricSamplingStrategy> _resolvedStrategies = new();

        public override bool ShouldSample(string metricName, double value, MetricType type, IDictionary<string, object>? tags)
        {
            var strategy = _resolvedStrategies.GetOrAdd(metricName, ResolveStrategy);
            return strategy.ShouldSample(metricName, value, type, tags);
        }

        private IMetricSamplingStrategy ResolveStrategy(string metricName)
        {
            if (Options.PerMetricRules.TryGetValue(metricName, out var rule))
            {
                return rule.StrategyType switch
                {
                    SamplingStrategyType.Always => new AlwaysSampleStrategy(Options),
                    SamplingStrategyType.Never => new NeverSampleStrategy(Options),
                    SamplingStrategyType.RateLimited when rule.MaxSamplesPerSecond is not null =>
                        new RateLimitedSamplingStrategy(Options, rule.MaxSamplesPerSecond.Value),
                    SamplingStrategyType.Threshold when rule.ValueThreshold is not null =>
                        new ThresholdSamplingStrategy(Options, rule.ValueThreshold.Value),
                    SamplingStrategyType.Custom when rule.CustomStrategy is not null =>
                        rule.CustomStrategy,
                    _ => LogAndFallback(metricName)
                };
            }

            return Options.DefaultStrategy ?? new AlwaysSampleStrategy(Options); // Default fallback if even global isn't defined
        }

        private IMetricSamplingStrategy LogAndFallback(string metricName)
        {
            if (Options.EnableDiagnostics)
            {
                logger.LogWarning($"[Telemetry] Invalid or incomplete rule for metric '{metricName}'. Falling back to default.");
            }

            return Options.DefaultStrategy ?? new AlwaysSampleStrategy(Options);
        }
    }
}
