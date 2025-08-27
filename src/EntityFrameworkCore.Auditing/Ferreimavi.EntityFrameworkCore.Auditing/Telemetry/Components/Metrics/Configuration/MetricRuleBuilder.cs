// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    public class MetricRuleBuilder(string metricName)
    {
        private readonly MetricSamplingRule _rule = new() { MetricName = metricName };

        public MetricRuleBuilder Always()
        {
            _rule.StrategyType = SamplingStrategyType.Always;
            return this;
        }

        public MetricRuleBuilder Never()
        {
            _rule.StrategyType = SamplingStrategyType.Never;
            return this;
        }

        public MetricRuleBuilder RateLimit(int maxPerSecond)
        {
            _rule.StrategyType = SamplingStrategyType.RateLimited;
            _rule.MaxSamplesPerSecond = maxPerSecond;
            return this;
        }

        public MetricRuleBuilder Threshold(double threshold)
        {
            _rule.StrategyType = SamplingStrategyType.Threshold;
            _rule.ValueThreshold = threshold;
            return this;
        }

        public MetricRuleBuilder Use(IMetricSamplingStrategy strategy)
        {
            _rule.StrategyType = SamplingStrategyType.Custom;
            _rule.CustomStrategy = strategy;
            return this;
        }

        public MetricSamplingRule Build() => _rule;
    }
}
