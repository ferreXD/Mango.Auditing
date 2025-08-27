// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Telemetry
{
    /// <summary>
    /// Provides methods to record and retrieve metric data.
    /// </summary>
    public interface IMetricProvider
    {
        /// <summary>
        /// Records a measurement for a given metric.
        /// </summary>
        /// <param name="metricName">The name of the metric.</param>
        /// <param name="value">The measurement value.</param>
        /// <param name="metricType"></param>
        /// <param name="tags">Optional tags to add context to the metric.</param>
        void RecordMeasurement(string metricName, double value, MetricType metricType = MetricType.Histogram, IDictionary<string, object>? tags = null);

        /// <summary>
        /// Retrieves aggregated metrics for a specific metric name over the specified time window.
        /// </summary>
        /// <param name="metricName">The name of the metric.</param>
        /// <param name="timeWindow">The time window for aggregation.</param>
        /// <returns>The aggregated metrics for the given metric, or null if not found.</returns>
        AggregatedMetrics? GetMetric(string metricName, MetricType metricType, TimeSpan timeWindow);

        /// <summary>
        /// Retrieves aggregated metrics for the specified time window.
        /// </summary>
        /// <param name="timeWindow">The time window for aggregation.</param>
        /// <returns>A dictionary of aggregated metrics keyed by metric name.</returns>
        IDictionary<MetricKey, AggregatedMetrics> GetAggregatedMetrics(TimeSpan timeWindow);

        /// <summary>
        /// Clears all metric data.
        /// </summary>
        void Clear();

        /// <summary>
        /// Clears metric data for a specific metric.
        /// </summary>
        /// <param name="metricName">The name of the metric to clear.</param>
        void Clear(string metricName, MetricType metricType);
    }
}