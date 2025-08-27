// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    public abstract class BaseAggregationStrategy
    {
        protected AggregatedMetrics Aggregate(
            MetricKey key,
            List<(double Value, DateTime Timestamp)> samples)
        {
            if (!samples.Any()) return new AggregatedMetrics { Name = key.Name, Type = key.Type, Count = 0 };

            var doubleValues = samples.Select(s => s.Value).ToList();
            var count = doubleValues.Count;
            var sum = doubleValues.Sum();
            var average = sum / count;
            var min = doubleValues.Min();
            var max = doubleValues.Max();
            var sumOfSquares = doubleValues.Sum(x => Math.Pow(x - average, 2));
            var stddev = Math.Sqrt(sumOfSquares / count);

            var sorted = doubleValues.OrderBy(x => x).ToList();
            var p95 = sorted[(int)Math.Floor(count * 0.95)];
            var p99 = sorted[(int)Math.Floor(count * 0.99)];

            return new AggregatedMetrics
            {
                Name = key.Name,
                Type = key.Type,
                Count = count,
                AverageValue = average,
                MinValue = min,
                MaxValue = max,
                StandardDeviation = stddev,
                P95Value = p95,
                P99Value = p99,
                LastValue = samples.Last().Value,
                FirstTimestamp = samples.First().Timestamp,
                LastTimestamp = samples.Last().Timestamp
            };
        }
    }
}
