// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TumblingWindowStrategy(TimeSpan bucketSize) : BaseAggregationStrategy, IAggregationWindowStrategy
    {
        public AggregatedMetrics? Aggregate(
            MetricKey key,
            IEnumerable<(double Value, DateTime Timestamp)> samples,
            DateTime now)
        {
            var alignedBucketStart = new DateTime((now.Ticks / bucketSize.Ticks) * bucketSize.Ticks);
            var alignedBucketEnd = alignedBucketStart + bucketSize;

            var values = samples
                .Where(s => s.Timestamp >= alignedBucketStart && s.Timestamp < alignedBucketEnd)
                .OrderBy(s => s.Timestamp)
                .ToList();

            return Aggregate(key, values);
        }
    }
}
