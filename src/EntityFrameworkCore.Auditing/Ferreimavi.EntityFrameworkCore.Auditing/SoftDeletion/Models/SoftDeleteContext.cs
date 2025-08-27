// ReSharper disable once CheckNamespace
namespace Mango.Auditing.SoftDeletion
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Context for managing soft deletion operations, allowing entities to be excluded from soft deletion logic.
    /// Usage:
    /// using (SoftDeleteContext.BeginScope())
    /// {
    ///     SoftDeleteContext.IgnoreInSoftDeletion(entity);
    ///     await TestingHelpers.DeleteSampleEntityOnDb(entity, fixture); // normal usage
    /// }
    /// </summary>

    public static class SoftDeleteContext
    {
        public static IDisposable CreateSoftDeleteExclusionScope(this ISoftDeletableEntity entity)
        {
            entity.ExcludeFromSoftDelete();
            return BeginScope();
        }

        public static void ExcludeFromSoftDelete(this ISoftDeletableEntity entity)
        {
            IgnoreInSoftDeletion(entity);
        }

        public static void IncludeInSoftDelete(this ISoftDeletableEntity entity)
        {
            _excluded.Value?.Remove(entity);
        }

        public static bool IsExcludedFromSoftDelete(this ISoftDeletableEntity entity)
        {
            return ShouldSkip(entity);
        }

        public static void ClearSoftDeleteExclusions()
        {
            _excluded.Value?.Clear();
        }

        private static readonly AsyncLocal<ConditionalWeakTable<ISoftDeletableEntity, object?>> _excluded = new();

        public static void IgnoreInSoftDeletion(ISoftDeletableEntity entity)
        {
            _excluded.Value ??= new ConditionalWeakTable<ISoftDeletableEntity, object?>();
            _excluded.Value.GetValue(entity, _ => null);
        }

        public static bool ShouldSkip(ISoftDeletableEntity entity) => _excluded.Value?.TryGetValue(entity, out _) ?? false;

        public static IDisposable BeginScope() => new SoftDeleteExclusionScope();

        private class SoftDeleteExclusionScope : IDisposable
        {
            private readonly ConditionalWeakTable<ISoftDeletableEntity, object?>? _prev = _excluded.Value;

            public void Dispose() => _excluded.Value = _prev!;

#if DEBUG

            ~SoftDeleteExclusionScope()
            {
                Debug.Fail("SoftDeleteContext scope was not disposed.");
            }

#endif
        }
    }
}
