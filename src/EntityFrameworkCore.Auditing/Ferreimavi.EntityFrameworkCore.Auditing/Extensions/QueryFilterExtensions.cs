// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    using System.Linq.Expressions;

    public static class QueryFilterExtensions
    {
        public static IQueryable<T> WithoutSoftDeleteFilter<T>(this IQueryable<T> query)
            where T : class, ISoftDeletableEntity
        {
            if (query is null) throw new ArgumentNullException(nameof(query));

            var parameter = Expression.Parameter(typeof(T), "e");
            var propertyAccess = Expression.Property(parameter, nameof(ISoftDeletableEntity.IsDeleted));
            var condition = Expression.Equal(propertyAccess, Expression.Constant(false));
            var lambda = Expression.Lambda<Func<T, bool>>(condition, parameter);

            return query.Where(lambda);
        }
    }
}