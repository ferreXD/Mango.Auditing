// ReSharper disable once CheckNamespace
namespace Mango.Auditing.SoftDeletion
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public class DefaultSoftDeletionStrategyDeterminant : ISoftDeletionStrategyDeterminant
    {
        public DeletionStrategy DetermineStrategy(EntityEntry<ISoftDeletableEntity> entry)
        {
            if (IsUnchangedOrAdded(entry)) return DeletionStrategy.None;

            if (ShouldRestore(entry)) return DeletionStrategy.Restore;

            return entry.State == EntityState.Modified
                ? DeletionStrategy.None
                : DetermineDeletionStrategy(entry);
        }

        private static bool IsUnchangedOrAdded<T>(EntityEntry<T> entry) where T : class
            => entry.State is not (EntityState.Deleted or EntityState.Modified);

        private bool ShouldRestore<T>(EntityEntry<T> entry) where T : class, ISoftDeletableEntity
        {
            var entity = entry.Entity;
            var wasDeleted = entry.OriginalValues.GetValue<bool>(nameof(ISoftDeletableEntity.IsDeleted));

            return entry.Property(x => x.IsDeleted).IsModified && wasDeleted && !entity.IsDeleted;
        }

        private DeletionStrategy DetermineDeletionStrategy<T>(EntityEntry<T> entry) where T : class, ISoftDeletableEntity
            => ShouldSkipSoftDelete(entry)
                ? DeletionStrategy.Delete
                : DeletionStrategy.SoftDelete;

        private bool ShouldSkipSoftDelete<T>(EntityEntry<T> entry) where T : class, ISoftDeletableEntity
        {
            return SoftDeleteContext.ShouldSkip(entry.Entity);
        }
    }
}
