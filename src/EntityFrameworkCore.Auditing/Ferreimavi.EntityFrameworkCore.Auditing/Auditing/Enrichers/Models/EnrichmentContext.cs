namespace Mango.Auditing.Auditing.Enrichers.Models
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public class EnrichmentContext(
        EntityEntry entityEntry,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        public EntityEntry EntityEntry { get; } = entityEntry;
        public IServiceProvider ServiceProvider { get; } = serviceProvider;
        public CancellationToken CancellationToken { get; } = cancellationToken;
    }
}