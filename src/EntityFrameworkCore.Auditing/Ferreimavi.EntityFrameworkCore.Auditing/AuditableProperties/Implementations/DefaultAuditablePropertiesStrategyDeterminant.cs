namespace Mango.Auditing.AuditableProperties.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public class DefaultAuditablePropertiesStrategyDeterminant : IAuditablePropertiesStrategyDeterminant
    {
        public AuditableStrategy DetermineStrategy(EntityEntry<IAuditableEntity> entry)
        {
            if (IsUnchangedOrDeleted(entry)) return AuditableStrategy.None;

            if (ShouldAdd(entry)) return AuditableStrategy.Added;

            return entry.State == EntityState.Modified
                ? AuditableStrategy.None
                : AuditableStrategy.Modified;
        }

        private static bool IsUnchangedOrDeleted<T>(EntityEntry<T> entry) where T : class
            => entry.State is not (EntityState.Added or EntityState.Modified);

        private bool ShouldAdd<T>(EntityEntry<T> entry) where T : class
            => entry.State is EntityState.Added;
    }
}
