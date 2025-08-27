// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Telemetry
{
    /// <summary>
    /// Represents aggregated metrics data.
    /// </summary>
    public class AggregatedMetrics
    {
        public string Name { get; set; } = string.Empty;
        public MetricType Type { get; set; }
        public double LastValue { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double AverageValue { get; set; }
        public double StandardDeviation { get; set; }
        public double P95Value { get; set; }
        public double P99Value { get; set; }
        public int Count { get; set; }
        public DateTime FirstTimestamp { get; set; }
        public DateTime LastTimestamp { get; set; }
        public TimeSpan TimeRange => LastTimestamp - FirstTimestamp;
        public double ValuesPerSecond => Count / TimeRange.TotalSeconds;
    }
}