// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Telemetry
{
    using System.Collections.Concurrent;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultAggregatedMetricProvider" /> class.
    /// </summary>
    /// <param name="maxDataPoints">The maximum number of data points to keep per metric.</param>
    public class DefaultAggregatedMetricProvider(IAggregationWindowStrategy aggregation, int maxDataPoints = 10000) : IAggregatedMetricProvider
    {
        // Stores measurement data for each metric for local aggregation.
        private readonly ConcurrentDictionary<MetricKey, ConcurrentQueue<(double Value, DateTime Timestamp)>> _timeSeriesData = new();

        /// <inheritdoc />
        public void RecordMeasurement(string metricName, double value, MetricType metricType = MetricType.Histogram, IDictionary<string, object>? tags = null)
        {
            // Safeguards already handled by the default metric provider.

            // Store the measurement for local aggregation. 
            var key = new MetricKey(metricName, metricType);
            var queue = _timeSeriesData.GetOrAdd(key, _ => new ConcurrentQueue<(double, DateTime)>());
            queue.Enqueue((value, DateTime.UtcNow));

            CleanupOldData(queue);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _timeSeriesData.Clear();
        }

        /// <inheritdoc />
        public IDictionary<MetricKey, AggregatedMetrics> GetAggregatedMetrics(TimeSpan timeWindow)
        {
            var result = new Dictionary<MetricKey, AggregatedMetrics>();
            var now = DateTime.UtcNow;

            foreach (var (key, queue) in _timeSeriesData)
            {
                var agg = aggregation.Aggregate(key, queue, now);
                if (agg is { Count: > 0 }) result[key] = agg;
            }

            return result;
        }

        /// <inheritdoc />
        public void Clear(string metricName, MetricType metricType)
        {
            var key = new MetricKey(metricName, metricType);
            _timeSeriesData.TryRemove(key, out _);
        }

        /// <inheritdoc />
        public AggregatedMetrics? GetMetric(string metricName, MetricType metricType, TimeSpan timeWindow)
        {
            var key = new MetricKey(metricName, metricType);
            if (!_timeSeriesData.TryGetValue(key, out var queue)) return null;

            var now = DateTime.UtcNow;
            return aggregation.Aggregate(key, queue, now);
        }

        #region Helper Methods for Aggregation

        /// <summary>
        /// Removes old data from the queue if the number of points exceeds the limit.
        /// </summary>
        private void CleanupOldData(ConcurrentQueue<(double Value, DateTime Timestamp)> queue)
        {
            while (queue.Count > maxDataPoints)
            {
                queue.TryDequeue(out _);
            }
        }

        #endregion
    }
}