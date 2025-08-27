// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Interceptors
{
    using AuditLogging;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using System.Diagnostics;
    using System.Threading;
    using Telemetry;

    public class AuditLoggingInterceptor(
        AuditingOptions auditOptions,
        IAuditLogCreator auditLogCreator,
        IAuditTracker auditTracker,
        ICurrentUserProvider currentUserProvider,
        IPerformanceMonitor monitor)
        : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null || !auditOptions.Features.IsEnabled(AuditFeature.Auditing))
            {
                var message = context == null
                    ? "DbContext is null, skipping auditing processing."
                    : "Auditing feature is disabled, skipping auditing processing.";

                monitor.LogInformation(message);
                return await base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            var auditLevel = AuditLevel.Information;
            if (context is AuditableDbContext auditableContext) auditLevel = auditableContext.CurrentAuditLevel;

            monitor.LogInformation($"[AuditLoggingInterceptor::SavingChangesAsync] Starting audit logging for DbContext '{context.GetType().Name}' with audit level '{auditLevel}'.");

            var operationTags = new Dictionary<string, object>
            {
                { "DbContextType", context.GetType().Name },
                { "DbContextHashCode", context.GetHashCode() },
                { "UserId", currentUserProvider.GetCurrentUserId() ?? string.Empty }
            };

            using var operation = await monitor.BeginOperationAsync(
                "AuditLoggingInterceptor.SavingChangesAsync",
                ActivityKind.Internal,
                operationTags, cancellationToken);

            var activity = Activity.Current;

            var auditEntries = new List<AuditLog>();

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Detached && e.State != EntityState.Unchanged);

            var count = entries.Count();
            monitor.LogInformation($"[AuditLoggingInterceptor::SavingChangesAsync] Found {count} change tracker entries to process for auditing.");

            // Prepare audit logs from change tracker entries
            foreach (var entry in entries)
            {
                var auditEntry = await auditLogCreator.CreateAsync(entry, auditLevel, cancellationToken);
                auditEntries.Add(auditEntry);
            }

            // Store the audit entry in the database or any other storage
            await auditTracker.TrackAsync(context, auditEntries, cancellationToken);

            var response = await base.SavingChangesAsync(eventData, result, cancellationToken);

            activity?.SetTag("AuditEntriesCount", auditEntries.Count);

            monitor.RecordMetric(
                "AuditLogging.TotalCreatedAudit",
                count,
                MetricType.Histogram,
                new Dictionary<string, object>
                {
                    { "UserId", currentUserProvider.GetCurrentUserId() ?? string.Empty }
                });

            monitor.LogInformation($"[AuditLoggingInterceptor::SavingChangesAsync] Completed audit logging for DbContext '{context.GetType().Name}'. Processed {auditEntries.Count} entries.");

            return response;
        }
    }
}