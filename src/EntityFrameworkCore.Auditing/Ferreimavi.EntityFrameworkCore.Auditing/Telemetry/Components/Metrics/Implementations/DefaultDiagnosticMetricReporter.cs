// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System.Collections.Generic;

    public class DefaultDiagnosticMetricReporter(TelemetryOptions options, IMetricProvider rawProvider) : IDiagnosticMetricReporter
    {
        public void RecordDiagnostic(string name, double value, MetricType type = MetricType.Histogram, IDictionary<string, object?>? tags = null)
        {
            var diagnosticTags = new Dictionary<string, object?>(tags ?? new Dictionary<string, object?>())
            {
                ["meta"] = true,
                ["diagnostic"] = true,
                ["source"] = nameof(DiagnosticMetricProviderDecorator)
            };

            var metricName = NamingConventions.GetFullName(options.MetricNamePrefix, $"internal.telemetry.{name}");
            rawProvider.RecordMeasurement(metricName, value, type, diagnosticTags!);
        }
    }
}
