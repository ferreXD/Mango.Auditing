// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    public class AuditingOptions
    {
        public AuditTableConfiguration TableConfiguration { get; set; } = new();
        public AuditingFeaturesConfiguration Features { get; } = new();
        public EnrichmentOptions Enrichment { get; set; } = new();
    }
}