// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Enrichers
{
    using Auditing.Enrichers.Models;

    public interface IAuditEnricher
    {
        int Order { get; }
        void Enrich(AuditLog auditLog, EnrichmentContext context);
        Task EnrichAsync(AuditLog auditLog, EnrichmentContext context);
    }
}