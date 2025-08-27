// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System.Collections.Generic;

    public class DiagnosticMetricProviderDecorator(
        IMetricProvider inner,
        IEnumerable<IDiagnosticMetricReporter> diagnostics,
        TelemetryOptions options) : IRootMetricProvider
    {
        public void RecordMeasurement(string name, double value, MetricType type, IDictionary<string, object>? tags = null)
        {
            inner.RecordMeasurement(name, value, type, tags);

            if (!options.EnableDiagnostics) return;

            foreach (var reporter in diagnostics)
            {
                reporter.RecordDiagnostic(
                    "metrics.recorded.total",
                    1,
                    MetricType.Counter,
                    new Dictionary<string, object?>
                    {
                        ["metricName"] = name,
                        ["type"] = type.ToString()
                    });
            }
        }

        public AggregatedMetrics? GetMetric(string metricName, MetricType metricType, TimeSpan timeWindow)
        {
            var result = inner.GetMetric(metricName, metricType, timeWindow);
            if (!options.EnableDiagnostics) return result;

            foreach (var reporter in diagnostics)
            {
                reporter.RecordDiagnostic(
                    "metrics.queries.single",
                    1,
                    MetricType.Counter,
                    new Dictionary<string, object?>
                    {
                        ["metricName"] = metricName,
                        ["type"] = metricType.ToString(),
                        ["windowSeconds"] = (int)timeWindow.TotalSeconds
                    });
            }

            return result;
        }

        public IDictionary<MetricKey, AggregatedMetrics> GetAggregatedMetrics(TimeSpan timeWindow)
        {
            var result = inner.GetAggregatedMetrics(timeWindow);
            if (!options.EnableDiagnostics) return result;

            foreach (var reporter in diagnostics)
            {
                reporter.RecordDiagnostic(
                    "metrics.queries.bulk",
                    1,
                    MetricType.Counter,
                    new Dictionary<string, object?>
                    {
                        ["count"] = result.Count,
                        ["windowSeconds"] = (int)timeWindow.TotalSeconds
                    });
            }

            return result;
        }

        public void Clear()
        {
            inner.Clear();

            if (options.EnableDiagnostics)
            {
                foreach (var reporter in diagnostics)
                {
                    reporter.RecordDiagnostic("metrics.cleared.all", 1, MetricType.Counter);
                }
            }
        }

        public void Clear(string metricName, MetricType metricType)
        {
            inner.Clear(metricName, metricType);

            if (options.EnableDiagnostics)
            {
                foreach (var reporter in diagnostics)
                {
                    reporter.RecordDiagnostic(
                        "metrics.cleared.specific",
                        1,
                        MetricType.Counter,
                        new Dictionary<string, object?>
                        {
                            ["metricName"] = metricName,
                            ["type"] = metricType.ToString()
                        });
                }
            }
        }
    }
}
