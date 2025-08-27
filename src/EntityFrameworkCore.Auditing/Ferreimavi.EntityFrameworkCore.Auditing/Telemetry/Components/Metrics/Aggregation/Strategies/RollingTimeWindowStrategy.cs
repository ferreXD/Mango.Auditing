// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RollingTimeWindowStrategy(TimeSpan window) : BaseAggregationStrategy, IAggregationWindowStrategy
    {
        public AggregatedMetrics? Aggregate(MetricKey key, IEnumerable<(double Value, DateTime Timestamp)> samples, DateTime now)
        {
            var cutoff = now - window;
            var values = samples
                .Where(s => s.Timestamp >= cutoff)
                .OrderBy(s => s.Timestamp)
                .ToList();

            return Aggregate(key, values);
        }
    }
}
