// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Enrichers
{
    using Auditing.Enrichers.Models;
    using System.Diagnostics;

    public class TraceIdentifierEnricher : IAuditEnricher
    {
        public int Order => 300;

        public void Enrich(AuditLog auditLog, EnrichmentContext context)
        {
            if (Activity.Current?.Id == null) return;

            auditLog.Metadata["TraceIdentifier.TraceId"] = Activity.Current.Id;
            auditLog.Metadata["TraceIdentifier.SpanId"] = Activity.Current.SpanId.ToString();
        }

        public Task EnrichAsync(AuditLog auditLog, EnrichmentContext context)
        {
            if (Activity.Current?.Id == null) return Task.CompletedTask;

            auditLog.Metadata["TraceIdentifier.TraceId"] = Activity.Current.Id;
            auditLog.Metadata["TraceIdentifier.SpanId"] = Activity.Current.SpanId.ToString();

            return Task.CompletedTask;
        }
    }
}