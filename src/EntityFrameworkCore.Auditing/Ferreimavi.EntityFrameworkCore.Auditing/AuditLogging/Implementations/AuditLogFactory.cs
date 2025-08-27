// ReSharper disable once CheckNamespace
namespace Mango.Auditing.AuditLogging
{
    using Enrichers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Security;
    using Telemetry;

    public class AuditLogCreator(AuditingOptions auditOptions,
        ICurrentUserProvider currentUserProvider,
        IEnumerable<IAuditEnricher> enrichers,
        IServiceProvider serviceProvider,
        ISensitiveDataFilter sensitiveDataFilter,
        IPerformanceMonitor monitor) : IAuditLogCreator
    {
        private readonly IEnumerable<IAuditEnricher> _enrichers = enrichers.OrderBy(e => e.Order);

        public async Task<AuditLog> CreateAsync(EntityEntry entry, AuditLevel auditLevel, CancellationToken cancellationToken)
        {
            var operation = GetOperationName(entry);

            monitor.LogVerbose(
                $"[AuditFactory::Creation] Creating audit log for operation '{operation}' on entity '{entry.Entity.GetType().Name}' with ID '{entry.GetEntityId()}'.");

            var auditEntry = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entry.Entity.GetType().Name,
                EntityId = entry.GetEntityId(),
                Operation = operation,
                Timestamp = DateTime.UtcNow,
                TableName = entry.Metadata.GetTableName(),
                PrimaryKey = entry.GetPrimaryKey(),
                AffectedColumns = entry.GetAffectedColumns(),
                OldValues = entry.GetOldValues(sensitiveDataFilter, auditOptions.Features.IsEnabled(AuditFeature.IncludeEntityValues)),
                NewValues = entry.GetNewValues(sensitiveDataFilter, auditOptions.Features.IsEnabled(AuditFeature.IncludeEntityValues)),
                Level = auditLevel
            };

            await EnrichAuditEntryAsync(entry, cancellationToken, auditEntry);

            return auditEntry;
        }

        private async Task EnrichAuditEntryAsync(EntityEntry entry, CancellationToken cancellationToken, AuditLog auditEntry)
        {
            if (auditOptions.Enrichment is { IncludeUserInfo: false, IncludeMetadata: false } && !_enrichers.Any())
            {
                monitor.LogVerbose(
                    $"[AuditFactory::Enrichment] No enrichment options enabled and no enrichers registered for entity '{entry.Entity.GetType().Name}'. Skipping enrichment.");
                return;
            }

            if (auditOptions.Enrichment.IncludeUserInfo)
            {
                monitor.LogVerbose(
                    $"[AuditFactory::Enrichment] IncludeUserInfo enabled. Enriching audit log with user info for entity '{entry.Entity.GetType().Name}'.");

                auditEntry.UserId = currentUserProvider.GetCurrentUserId();
                auditEntry.UserName = currentUserProvider.GetCurrentUserName();
            }

            if (auditOptions.Enrichment.IncludeMetadata)
            {
                monitor.LogVerbose(
                    $"[AuditFactory::Enrichment] IncludeMetadata enabled. Enriching audit log with additional metadata for entity '{entry.Entity.GetType().Name}'.");
                auditEntry.Metadata = currentUserProvider.GetAdditionalUserInfo();
            }

            var enricherNames = _enrichers.Select(e => e.GetType().Name).ToList();
            monitor.LogVerbose(
                $"[AuditFactory::Enrichment] Enriching audit log with enrichers: {string.Join(", ", enricherNames)} for entity '{entry.Entity.GetType().Name}'.");

            // Allow enrichers to append further contextual info
            await auditEntry.EnrichAuditLogAsync(entry, _enrichers, serviceProvider, cancellationToken);
        }

        private static string GetOperationName(EntityEntry entry)
        {
            var operation = entry.State.ToString();
            if (entry is not { State: EntityState.Modified, Entity: ISoftDeletableEntity softDeletableEntity }) return operation;

            var deleted = entry.OriginalValues.GetValue<bool>(nameof(ISoftDeletableEntity.IsDeleted));
            var propertyModified = entry.Properties
                .First(p => p.Metadata.Name == nameof(ISoftDeletableEntity.IsDeleted))
                .IsModified;

            if (propertyModified && !deleted && softDeletableEntity.IsDeleted) operation = "SoftDeleted";

            return operation;
        }
    }
}
