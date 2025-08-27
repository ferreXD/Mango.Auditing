// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    public interface ISoftDeletableEntity
    {
        string? DeletedBy { get; set; }
        DateTime? DeletedAt { get; set; }
        bool IsDeleted { get; set; }
    }
}