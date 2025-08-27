// ReSharper disable once CheckNamespace
namespace Mango.Auditing
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Telemetry;

    public class DefaultAuditLogAttacher<T>(IPerformanceMonitor monitor) : IAuditLogAttacher
    {
        private const int BatchSize = 1000;

        /// <inheritdoc />
        public async Task AttachAsync(DbContext dbContext, IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken = default)
        {
            var auditLogList = auditLogs.ToList();

            if (!auditLogList.Any())
            {
                monitor.LogDebug("No audit logs to save.");
                return;
            }

            using var op = await monitor.BeginOperationAsync("audit.save_logs", ActivityKind.Internal, new Dictionary<string, object>
            {
                {"log.count", auditLogList.Count},
                {"db.context", typeof(T).Name}
            }, cancellationToken);

            if (dbContext.ChangeTracker.Entries<AuditLog>().Any())
            {
                monitor.LogWarning("Detected already tracked AuditLog entries. Ensure audit logs are detached or new.");
                monitor.RecordMetric("audit.tracked_entities", 1, MetricType.Counter, new Dictionary<string, object>
                {
                    {"count", dbContext.ChangeTracker.Entries<AuditLog>().Count()}
                });
            }

            if (auditLogList.Count > BatchSize)
            {
                monitor.LogWarning("High volume audit log insert: {AuditLogCount}. Using batch strategy.", ("AuditLogCount", auditLogList.Count));
                monitor.RecordMetric("audit.bulk_warning", 1, MetricType.Counter, new Dictionary<string, object>
                {
                    {"count", auditLogList.Count}
                });
            }

            int totalSaved = 0, batchIndex = 1;

            var innerStopWatch = new Stopwatch();
            var outerStopWatch = Stopwatch.StartNew();

            var saveMode = auditLogList.Count > BatchSize ? "bulk" : "standard";

            for (var i = 0; i < auditLogList.Count; i += BatchSize)
            {
                var chunk = auditLogList.Skip(i).Take(BatchSize).ToList();
                monitor.LogDebug("Saving audit log batch {BatchIndex} with {BatchSize} entries.", ("BatchIndex", ++batchIndex - 1), ("BatchSize", chunk.Count));

                innerStopWatch.Restart();
                await dbContext.Set<AuditLog>().AddRangeAsync(chunk, cancellationToken);
                innerStopWatch.Stop();

                monitor.RecordMetric("audit.batch_insert_duration", innerStopWatch.ElapsedMilliseconds, MetricType.Histogram, new Dictionary<string, object>
                {
                    {"batch.size", chunk.Count}
                });

                totalSaved += chunk.Count;
            }

            outerStopWatch.Stop();
            monitor.RecordMetric("audit.insert_duration", outerStopWatch.ElapsedMilliseconds, MetricType.Histogram, new Dictionary<string, object>
            {
                {"count", auditLogList.Count},
                { "save_mode", saveMode }
            });

            monitor.RecordMetric("audit.logs.saved", totalSaved, MetricType.Counter, new Dictionary<string, object>
            {
                { "context", typeof(T).Name},
                { "save_mode", saveMode }
            });

            monitor.RecordEvent("AuditLog persistence completed", new Dictionary<string, object>
            {
                {"total_saved", totalSaved},
                {"db_context", typeof(T).Name}
            });

            monitor.LogDebug("Queued {Count} audit logs for persistence.", ("Count", auditLogList.Count));
        }
    }
}
