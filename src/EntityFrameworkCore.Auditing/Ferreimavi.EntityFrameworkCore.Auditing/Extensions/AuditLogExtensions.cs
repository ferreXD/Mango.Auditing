namespace Mango.Auditing
{
    using Auditing.Enrichers.Models;
    using Enrichers;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using System.Text.Json;

    public static class AuditLogExtensions
    {
        public static void EnrichAuditLog(this AuditLog auditLog, EntityEntry entry, IEnumerable<IAuditEnricher> enrichers, IServiceProvider serviceProvider)
        {
            var context = new EnrichmentContext(entry, serviceProvider, CancellationToken.None);

            foreach (var enricher in enrichers)
                try
                {
                    enricher.Enrich(auditLog, context);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other enrichers
                    auditLog.Metadata[$"EnricherError_{enricher.GetType().Name}"] = ex.Message;
                }
        }

        public static async Task EnrichAuditLogAsync(this AuditLog auditLog, EntityEntry entry, IEnumerable<IAuditEnricher> enrichers, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var context = new EnrichmentContext(entry, serviceProvider, cancellationToken);

            foreach (var enricher in enrichers)
                try
                {
                    await enricher.EnrichAsync(auditLog, context);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other enrichers
                    auditLog.Metadata[$"EnricherError_{enricher.GetType().Name}"] = ex.Message;
                }
        }

        // TODO: Changes masking comparison, it can be different based on the masking strategy used
        public static async Task<IEnumerable<AuditChange>> GetChangesAsync(this AuditLog auditLog)
        {
            string? oldValues = null;
            string? newValues = null;

            oldValues = auditLog.OldValues;
            newValues = auditLog.NewValues;

            var oldValuesDict = !string.IsNullOrWhiteSpace(oldValues) ? JsonSerializer.Deserialize<Dictionary<string, object>>(oldValues) : new Dictionary<string, object>();
            var newValuesDict = !string.IsNullOrWhiteSpace(newValues) ? JsonSerializer.Deserialize<Dictionary<string, object>>(newValues) : new Dictionary<string, object>();

            var affectedColumns = auditLog.AffectedColumns?.Split(',') ?? [];

            return await Task.FromResult(affectedColumns
                .Select(column => new AuditChange
                {
                    PropertyName = column,
                    OriginalValue = oldValuesDict!.GetValueOrDefault(column),
                    NewValue = newValuesDict!.GetValueOrDefault(column),
                    IsSensitive = oldValuesDict?.GetValueOrDefault(column)?.ToString() == "***MASKED***" || newValuesDict?.GetValueOrDefault(column)?.ToString() == "***MASKED***"
                })
                .Where(x => x is { NewValue: not null, OriginalValue: not null }));
        }

        public static async Task<IDictionary<string, string>> GetMetadataAsync(this AuditLog auditLog) => await Task.FromResult(auditLog.Metadata ?? new Dictionary<string, string>());
    }
}