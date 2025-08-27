// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System.Collections.Generic;

    public interface IDiagnosticMetricReporter
    {
        void RecordDiagnostic(string diagnosticName, double value, MetricType type, IDictionary<string, object?>? tags = null);
    }
}
