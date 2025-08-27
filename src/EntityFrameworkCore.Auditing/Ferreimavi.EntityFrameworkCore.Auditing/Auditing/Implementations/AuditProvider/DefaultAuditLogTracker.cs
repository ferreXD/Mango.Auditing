// ReSharper disable once CheckNamespace
namespace Mango.Auditing
{
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using System.Diagnostics;
    using System.Threading;
    using Telemetry;

    public class DefaultAuditLogTracker<T>(IAuditLogAttacher attacher, IAuditLogDetacher detacher, IAuditLogger logger, IPerformanceMonitor monitor) : IAuditTracker
    {
        /// <inheritdoc />
        public async Task TrackAsync(DbContext dbContext, IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken = default)
        {
            var auditList = auditLogs.ToList();
            using var op = await monitor.BeginOperationAsync("audit.tracker.validate_and_track", ActivityKind.Internal,
                new Dictionary<string, object>()
                {
                    { "count", auditList.Count },
                    { "context", typeof(T).Name }
                }, cancellationToken: cancellationToken);

            try
            {
                foreach (var entry in auditList)
                {
                    if (string.IsNullOrWhiteSpace(entry.EntityId))
                        throw new InvalidOperationException($"AuditLog EntityId cannot be null or empty (EntityType: {entry.EntityType})");

                    if (string.IsNullOrWhiteSpace(entry.Operation))
                        throw new InvalidOperationException($"AuditLog Operation must be set (EntityId: {entry.EntityId})");

                    if (entry.Timestamp == default)
                        entry.Timestamp = DateTime.UtcNow;
                }

                await attacher.AttachAsync(dbContext, auditLogs, cancellationToken);

                foreach (var entry in auditList)
                {
                    var properties = new (string Key, object? Value)[]
                    {
                        ("AuditLogId", entry.Id),
                        ("EntityType", entry.EntityType),
                        ("EntityId", entry.EntityId),
                        ("Action", entry.Operation)
                    };

                    logger.Log(entry.Level, "Audit log tracked successfully. ID: {AuditLogId}, Entity: {EntityType}.{EntityId}, Action: {Action}", properties);
                }
            }
            catch (Exception ex)
            {
                monitor.RecordException(ex, new Dictionary<string, object>
                {
                    { "Count", auditList.Count },
                    { "Error", ex.Message }
                });

                monitor.LogError(ex, "Failed to track audit logs");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task TrackAsync(DbContext dbContext, AuditLog auditLog, CancellationToken cancellationToken = default)
            => await TrackAsync(dbContext, [auditLog], cancellationToken);

        /// <inheritdoc />
        public async Task UntrackAsync(DbContext dbContext, IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            using var op = await monitor.BeginOperationAsync("audit.tracker.untrack_logs", ActivityKind.Internal, new Dictionary<string, object>
            {
                { "count", ids.Count() }
            }, cancellationToken);

            try
            {
                await detacher.DetachAsync(dbContext, ids, cancellationToken);
            }
            catch (Exception ex)
            {
                monitor.RecordException(ex, new Dictionary<string, object>
                {
                    { "Ids", string.Join(", ", ids) },
                    { "Error", ex.Message }
                });

                monitor.LogError(ex, "Failed to untrack audit logs", ("AuditLogIds", string.Join(", ", ids)));
                throw;
            }
        }
    }
}
