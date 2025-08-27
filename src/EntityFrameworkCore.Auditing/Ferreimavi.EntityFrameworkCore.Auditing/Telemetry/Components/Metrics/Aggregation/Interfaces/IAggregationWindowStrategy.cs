// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System;
    using System.Collections.Generic;

    public interface IAggregationWindowStrategy
    {
        AggregatedMetrics? Aggregate(MetricKey key, IEnumerable<(double Value, DateTime Timestamp)> samples, DateTime now);
    }
}
