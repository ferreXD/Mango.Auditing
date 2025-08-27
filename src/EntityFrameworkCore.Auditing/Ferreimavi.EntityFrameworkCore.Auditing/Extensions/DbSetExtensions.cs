// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    using Microsoft.EntityFrameworkCore;

    public static class DbSetExtensions
    {
        public static IQueryable<T> IncludeSoftDeleted<T>(this DbSet<T> query) where T : class, ISoftDeletableEntity => query.IgnoreQueryFilters();
        public static IQueryable<T> OnlySoftDeleted<T>(this DbSet<T> query) where T : class, ISoftDeletableEntity => query.IgnoreQueryFilters().Where(x => x.IsDeleted);
    }
}