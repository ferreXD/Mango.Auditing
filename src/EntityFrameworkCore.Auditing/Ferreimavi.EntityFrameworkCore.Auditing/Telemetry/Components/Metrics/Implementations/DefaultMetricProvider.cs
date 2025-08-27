// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Telemetry
{
    using System.Collections.Concurrent;
    using System.Diagnostics.Metrics;

    /// <summary>
    ///     Provides a Meter-based implementation of IMetricProvider that records measurements using Histograms
    ///     and keeps an in-memory store for aggregation.
    /// </summary>
    public class DefaultMetricProvider(
        IAggregatedMetricProvider aggregatedMetricProvider,
        IMetricSamplingStrategy samplingStrategy,
        TelemetryOptions options,
        string meterName = "DefaultMeter") : IMetricProvider
    {
        private readonly IAggregatedMetricProvider _aggregatedMetricProvider = aggregatedMetricProvider ?? throw new ArgumentNullException(nameof(aggregatedMetricProvider));
        private readonly IMetricSamplingStrategy _samplingStrategy = samplingStrategy ?? throw new ArgumentNullException(nameof(samplingStrategy));
        private readonly Meter _meter = new Meter(meterName);

        private readonly string _metricPrefix = options.MetricNamePrefix;

        public readonly ConcurrentDictionary<string, Counter<double>> Counters = new();
        public readonly ConcurrentDictionary<string, Histogram<double>> Histograms = new();
        public readonly ConcurrentDictionary<string, UpDownCounter<double>> UpDownCounters = new();

        /// <inheritdoc />
        public void RecordMeasurement(string metricName, double value, MetricType metricType = MetricType.Histogram, IDictionary<string, object>? tags = null)
        {
            if (string.IsNullOrEmpty(metricName)) throw new ArgumentNullException(nameof(metricName));

            var fullMetricName = NamingConventions.GetFullName(_metricPrefix, metricName);

            // Sampling decision
            if (!_samplingStrategy.IsEnabled(metricName) || !_samplingStrategy.ShouldSample(fullMetricName, value, metricType, tags)) return;

            switch (metricType)
            {
                case MetricType.Histogram:
                    RecordHistogram(fullMetricName, value, tags);
                    break;
                case MetricType.Counter:
                    RecordCounter(fullMetricName, value, tags);
                    break;
                case MetricType.UpDownCounter:
                    RecordUpDownCounter(fullMetricName, value, tags);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(metricType), metricType, null);
            }

            _aggregatedMetricProvider.RecordMeasurement(fullMetricName, value, metricType, tags);
        }

        /// <inheritdoc />
        public void Clear()
        {
            Histograms.Clear();
            Counters.Clear();
            UpDownCounters.Clear();

            _aggregatedMetricProvider.Clear();
        }

        /// <inheritdoc />
        public IDictionary<MetricKey, AggregatedMetrics> GetAggregatedMetrics(TimeSpan timeWindow) => _aggregatedMetricProvider.GetAggregatedMetrics(timeWindow);

        /// <inheritdoc />
        public void Clear(string metricName, MetricType metricType)
        {
            var fullMetricName = NamingConventions.GetFullName(_metricPrefix, metricName);
            Histograms.TryRemove(fullMetricName, out _);
            Counters.TryRemove(fullMetricName, out _);
            UpDownCounters.TryRemove(fullMetricName, out _);

            _aggregatedMetricProvider.Clear(fullMetricName, metricType);
        }

        /// <inheritdoc />
        public AggregatedMetrics? GetMetric(string metricName, MetricType metricType, TimeSpan timeWindow)
        {
            var fullMetricName = NamingConventions.GetFullName(_metricPrefix, metricName);
            return _aggregatedMetricProvider.GetMetric(fullMetricName, metricType, timeWindow);
        }

        #region Instrument Recording Methods

        private void RecordHistogram(string metricName, double value, IDictionary<string, object>? tags)
        {
            var histogram = Histograms.GetOrAdd(metricName, name => _meter.CreateHistogram<double>(name));
            var spanTags = tags?.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)).ToArray()
                           ?? ReadOnlySpan<KeyValuePair<string, object?>>.Empty;
            histogram.Record(value, spanTags);
        }

        private void RecordCounter(string metricName, double value, IDictionary<string, object>? tags)
        {
            var counter = Counters.GetOrAdd(metricName, name => _meter.CreateCounter<double>(name));
            var spanTags = tags?.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)).ToArray()
                           ?? ReadOnlySpan<KeyValuePair<string, object?>>.Empty;
            counter.Add(value, spanTags);
        }

        private void RecordUpDownCounter(string metricName, double value, IDictionary<string, object>? tags)
        {
            var upDownCounter = UpDownCounters.GetOrAdd(metricName, name => _meter.CreateUpDownCounter<double>(name));
            var spanTags = tags?.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)).ToArray()
                           ?? ReadOnlySpan<KeyValuePair<string, object?>>.Empty;
            upDownCounter.Add(value, spanTags);
        }

        #endregion
    }
}