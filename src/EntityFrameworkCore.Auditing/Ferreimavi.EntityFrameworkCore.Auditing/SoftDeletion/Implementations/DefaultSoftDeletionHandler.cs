// ReSharper disable once CheckNamespace
namespace Mango.Auditing.SoftDeletion
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public class DefaultSoftDeletionHandler : ISoftDeletionHandler
    {
        public void Restore<T>(IEnumerable<EntityEntry<T>> entries) where T : class, ISoftDeletableEntity
            => entries.ToList().ForEach(Restore);

        public void Restore<T>(EntityEntry<T> entry) where T : class, ISoftDeletableEntity
        {
            entry.State = EntityState.Modified;

            entry.Entity.IsDeleted = false;
            entry.Entity.DeletedAt = null;
            entry.Entity.DeletedBy = null;
        }

        public void MarkAsDeleted<T>(IEnumerable<EntityEntry<T>> entries, string? deletedBy) where T : class, ISoftDeletableEntity
            => entries.ToList().ForEach(entry => MarkAsDeleted(entry, deletedBy));

        public void MarkAsDeleted<T>(EntityEntry<T> entry, string? deletedBy) where T : class, ISoftDeletableEntity
        {
            // Switch to Modified to allow setting soft deletion markers
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = DateTime.UtcNow;
            entry.Entity.DeletedBy = deletedBy;
        }
    }
}
