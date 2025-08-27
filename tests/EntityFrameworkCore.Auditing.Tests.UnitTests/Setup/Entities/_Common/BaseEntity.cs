// ReSharper disable once CheckNamespace

namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup
{
    using Mango.Auditing;
    using System;

    public abstract class BaseEntity<T> : IAuditableEntity, ISoftDeletableEntity
    {
        public T Id { get; set; } = default!;

        // Auditable properties
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        // Soft delete properties
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
