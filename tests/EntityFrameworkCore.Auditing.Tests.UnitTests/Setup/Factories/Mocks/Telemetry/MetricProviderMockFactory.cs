// ReSharper disable once CheckNamespace

namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup
{
    using Mango.Auditing.Telemetry;
    using Moq;

    /// <summary>
    /// Factory class for creating a pre-configured Moq instance of IMetricProvider.
    /// </summary>
    public static class MetricProviderMockFactory
    {
        /// <summary>
        /// Creates and returns a new mock of IMetricProvider with default setups.
        /// </summary>
        /// <returns>A configured <see cref="Mock{IMetricProvider}" /> instance.</returns>
        public static MetricProviderMock Create()
        {
            var mock = new Mock<IMetricProvider>();

            // A list to capture recorded measurements for later verification.
            var recordedMeasurements = new List<(string MetricName, double Value, DateTime Timestamp, MetricType MetricType, IDictionary<string, object>? Tags)>();

            // Setup for RecordMeasurement overload that doesn't specify timestamp.
            mock.Setup(m => m.RecordMeasurement(
                    It.IsAny<string>(),
                    It.IsAny<double>(),
                    It.IsAny<MetricType>(),
                    It.IsAny<IDictionary<string, object>>()))
                .Callback<string, double, MetricType, IDictionary<string, object>?>((metricName, value, metricType, tags) =>
                {
                    recordedMeasurements.Add((metricName, value, DateTime.UtcNow, metricType, tags));
                });

            // Setup for GetMetric: For demonstration, return an AggregatedMetrics with count equal
            // to the number of recordings matching the metric name and type, or null if no measurements.
            mock.Setup(m => m.GetMetric(It.IsAny<string>(), It.IsAny<MetricType>(), It.IsAny<TimeSpan>()))
                .Returns((string metricName, MetricType metricType, TimeSpan window) =>
                {
                    var count = recordedMeasurements.Count(x =>
                        x.MetricName == metricName &&
                        x.MetricType == metricType);
                    if (count == 0) return null;

                    // For simplicity, return dummy aggregated metrics.
                    var lastMeasurement = recordedMeasurements.LastOrDefault(x => x.MetricName == metricName && x.MetricType == metricType);
                    return new AggregatedMetrics
                    {
                        Name = metricName,
                        Count = count,
                        LastValue = lastMeasurement.Value,
                        // Additional fields could be computed based on recordedMeasurements here.
                        MinValue = recordedMeasurements.Where(x => x.MetricName == metricName && x.MetricType == metricType).Min(x => x.Value),
                        MaxValue = recordedMeasurements.Where(x => x.MetricName == metricName && x.MetricType == metricType).Max(x => x.Value),
                        AverageValue = recordedMeasurements.Where(x => x.MetricName == metricName && x.MetricType == metricType).Average(x => x.Value),
                        StandardDeviation = 0, // Omitted for brevity.
                        P95Value = 0, // Omitted for brevity.
                        P99Value = 0, // Omitted for brevity.
                        FirstTimestamp = recordedMeasurements.Where(x => x.MetricName == metricName && x.MetricType == metricType).Min(x => x.Timestamp),
                        LastTimestamp = recordedMeasurements.Where(x => x.MetricName == metricName && x.MetricType == metricType).Max(x => x.Timestamp)
                    };
                });

            // Setup for GetAggregatedMetrics: For demonstration, return an empty dictionary,
            // or you could aggregate based on recordedMeasurements.
            mock.Setup(m => m.GetAggregatedMetrics(It.IsAny<TimeSpan>()))
                .Returns((TimeSpan window) => new Dictionary<MetricKey, AggregatedMetrics>());

            // Setup for Clear() to clear the captured measurements.
            mock.Setup(m => m.Clear())
                .Callback(() =>
                {
                    recordedMeasurements.Clear();
                });

            // Setup for Clear(metricName, metricType)
            mock.Setup(m => m.Clear(It.IsAny<string>(), It.IsAny<MetricType>()))
                .Callback<string, MetricType>((metricName, metricType) =>
                {
                    recordedMeasurements.RemoveAll(x => x.MetricName == metricName && x.MetricType == metricType);
                });

            return new MetricProviderMock(mock, recordedMeasurements);
        }

        public class MetricProviderMock(
            Mock<IMetricProvider> mock,
            List<(string MetricName, double Value, DateTime Timestamp, MetricType MetricType, IDictionary<string, object>? Tags)> recordedMeasurements)
        {
            public Mock<IMetricProvider> Mock { get; set; } = mock;
            public List<(string MetricName, double Value, DateTime Timestamp, MetricType MetricType, IDictionary<string, object>? Tags)> RecordedMeasurements { get; set; } = recordedMeasurements;
        }
    }
}