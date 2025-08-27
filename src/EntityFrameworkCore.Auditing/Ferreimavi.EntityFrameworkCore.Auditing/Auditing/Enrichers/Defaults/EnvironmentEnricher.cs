// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Enrichers
{
    using Auditing.Enrichers.Models;

    public class EnvironmentEnricher : IAuditEnricher
    {
        public int Order => 100;

        public void Enrich(AuditLog auditLog, EnrichmentContext context)
        {
            auditLog.Metadata["Environment.Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
            auditLog.Metadata["Environment.MachineName"] = Environment.MachineName;
            auditLog.Metadata["Environment.ProcessId"] = Environment.ProcessId.ToString();
            auditLog.Metadata["Environment.ApplicationName"] = AppDomain.CurrentDomain.FriendlyName;
        }

        public Task EnrichAsync(AuditLog auditLog, EnrichmentContext context)
        {
            auditLog.Metadata["Environment.Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
            auditLog.Metadata["Environment.MachineName"] = Environment.MachineName;
            auditLog.Metadata["Environment.ProcessId"] = Environment.ProcessId.ToString();
            auditLog.Metadata["Environment.ApplicationName"] = AppDomain.CurrentDomain.FriendlyName;

            return Task.CompletedTask;
        }
    }
}