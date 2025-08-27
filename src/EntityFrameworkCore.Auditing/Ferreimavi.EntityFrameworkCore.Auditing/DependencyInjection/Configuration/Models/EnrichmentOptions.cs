// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    using Enrichers;
    using Security;

    public class EnrichmentOptions
    {
        public bool IncludeMetadata { get; set; } = false;
        public bool IncludeUserInfo { get; set; } = false;
        // TODO: Give another thought to this
        public IEnumerable<IAuditEnricher> Enrichers { get; set; } = Enumerable.Empty<IAuditEnricher>();
        public ISensitiveDataFilter? CustomSensitiveDataFilter { get; set; }
    }
}