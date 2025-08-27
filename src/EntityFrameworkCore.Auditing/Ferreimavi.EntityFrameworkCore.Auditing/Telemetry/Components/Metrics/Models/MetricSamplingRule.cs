// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    public class MetricSamplingRule
    {
        public bool IsEnabled { get; set; } = true;
        public string MetricName { get; set; } = string.Empty;
        public SamplingStrategyType StrategyType { get; set; } = SamplingStrategyType.Always;

        // For RateLimited strategy
        public int? MaxSamplesPerSecond { get; set; }

        // For Threshold strategy
        public double? ValueThreshold { get; set; }

        // Custom strategy
        public IMetricSamplingStrategy? CustomStrategy { get; set; }
    }

    public enum SamplingStrategyType
    {
        Always,
        Never,
        RateLimited,
        Threshold,
        Custom
    }
}
