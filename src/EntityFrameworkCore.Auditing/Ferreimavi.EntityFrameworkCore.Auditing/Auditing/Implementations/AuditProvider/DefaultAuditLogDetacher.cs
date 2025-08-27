// ReSharper disable once CheckNamespace
namespace Mango.Auditing
{
    using Microsoft.EntityFrameworkCore;
    using System.Diagnostics;
    using System.Threading;
    using Telemetry;

    public class DefaultAuditLogDetacher<T>(IPerformanceMonitor monitor) : IAuditLogDetacher
        where T : DbContext
    {
        private const int BatchSize = 1000;

        public async Task DetachAsync(DbContext dbContext, IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var auditLogList = await dbContext
                .Set<AuditLog>()
                .AsNoTracking()
                .Where(a => ids.Contains(a.Id))
                .ToListAsync(cancellationToken);

            if (!auditLogList.Any())
            {
                monitor.LogDebug("No audit logs to delete.");
                return;
            }

            using var op = await monitor.BeginOperationAsync("audit.provider.delete_logs", ActivityKind.Internal, new Dictionary<string, object>
            {
                { "count", auditLogList.Count() }
            }, cancellationToken);

            if (auditLogList.Count < ids.Count())
            {
                var missingIds = ids.Except(auditLogList.Select(a => a.Id)).ToList();

                monitor.LogWarning("Some audit logs not found for deletion",
                    ("requested_ids", string.Join(", ", ids)),
                    ("found_count", auditLogList.Count)
                );

                monitor.RecordEvent("Audit logs not found for deletion", new Dictionary<string, object>
                {
                    { "requested_ids", ids.Count() },
                    { "found_count", auditLogList.Count },
                    { "missing_ids", string.Join(", ", missingIds) }
                });

                monitor.RecordMetric("audit.logs_not_found", ids.Count() - auditLogList.Count, MetricType.Counter, new Dictionary<string, object>
                {
                    { "requested_count", ids.Count() },
                    { "found_count", auditLogList.Count }
                });
            }

            if (dbContext.ChangeTracker.Entries<AuditLog>().Any())
            {
                monitor.RecordEvent("AuditLog already tracked entries detected", new Dictionary<string, object>
                {
                    {"count", dbContext.ChangeTracker.Entries<AuditLog>().Count()}
                });

                monitor.LogWarning("Detected already tracked AuditLog entries. Ensure audit logs are detached or new.");
            }

            if (auditLogList.Count > BatchSize)
            {
                monitor.LogWarning("High volume audit log deletion: {AuditLogCount}. Using batch strategy.", ("AuditLogCount", auditLogList.Count));
                monitor.RecordMetric("audit.bulk_warning", 1, MetricType.Counter, new Dictionary<string, object>
                {
                    {"count", auditLogList.Count}
                });
            }


            int totalSaved = 0, batchIndex = 0;

            var outerStopWatch = Stopwatch.StartNew();

            for (var i = 0; i < auditLogList.Count; i += BatchSize)
            {
                var chunk = auditLogList.Skip(i).Take(BatchSize).ToList();

                using var batchSpan = await monitor.BeginOperationAsync("audit.delete_logs.batch", ActivityKind.Internal, new Dictionary<string, object>
                {
                    {"batch.index", batchIndex++},
                    {"batch.size", chunk.Count}
                }, cancellationToken);

                dbContext.Set<AuditLog>().RemoveRange(chunk);
                totalSaved += chunk.Count;
            }

            outerStopWatch.Stop();
            monitor.RecordMetric("audit.delete_duration_ms", outerStopWatch.ElapsedMilliseconds, MetricType.Histogram, new Dictionary<string, object>
            {
                {"count", auditLogList.Count}
            });

            monitor.RecordMetric("audit.logs.deleted", totalSaved, MetricType.Counter, new Dictionary<string, object>
            {
                { "context", typeof(T).Name}
            });

            monitor.RecordEvent("AuditLog persistence completed", new Dictionary<string, object>
            {
                {"total_deleted", totalSaved},
                {"db_context", typeof(T).Name}
            });
        }
    }
}
