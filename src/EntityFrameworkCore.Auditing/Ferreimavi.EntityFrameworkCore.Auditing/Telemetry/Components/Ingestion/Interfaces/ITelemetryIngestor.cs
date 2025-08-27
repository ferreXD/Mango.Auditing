// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Telemetry
{
    using System.Diagnostics;

    /// <summary>
    /// Defines methods to ingest telemetry data (metrics and traces) to external telemetry systems.
    /// </summary>
    public interface ITelemetryIngestor
    {
        /// <summary>
        /// Ingests aggregated metric data into an external system.
        /// </summary>
        /// <param name="metrics">A dictionary of aggregated metrics.</param>
        void IngestMetrics(IDictionary<string, AggregatedMetrics> metrics);

        /// <summary>
        /// Ingests trace data (activities) into an external system.
        /// </summary>
        /// <param name="activities">A collection of activities representing trace data.</param>
        void IngestTraces(IEnumerable<Activity> activities);
    }
}