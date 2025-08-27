// ReSharper disable once CheckNamespace
namespace Mango.Auditing.SoftDeletion
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public interface ISoftDeletionHandler
    {
        void Restore<T>(IEnumerable<EntityEntry<T>> entries) where T : class, ISoftDeletableEntity;
        void Restore<T>(EntityEntry<T> entry) where T : class, ISoftDeletableEntity;
        void MarkAsDeleted<T>(IEnumerable<EntityEntry<T>> entries, string? deletedBy) where T : class, ISoftDeletableEntity;
        void MarkAsDeleted<T>(EntityEntry<T> entry, string? deletedBy) where T : class, ISoftDeletableEntity;
    }
}
