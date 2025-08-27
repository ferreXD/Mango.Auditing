// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq.Expressions;
    using System.Reflection;
    using DbContext = Microsoft.EntityFrameworkCore.DbContext;

    public class AuditableDbContext(
        DbContextOptions options,
        AuditingOptions auditingOptions) : DbContext(options)
    {
        public AuditLevel CurrentAuditLevel { get; set; } = AuditLevel.Information;

        internal DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<AuditLog>().ToTable(
                auditingOptions.TableConfiguration.TableName,
                auditingOptions.TableConfiguration.Schema
            );

            // Add global query filter for soft-deleted entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                if (typeof(ISoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(ISoftDeletableEntity.IsDeleted));
                    var falseConstant = Expression.Constant(false);
                    var lambdaExpression = Expression.Lambda(Expression.Equal(property, falseConstant), parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambdaExpression);
                }
        }

        public virtual Task<int> SaveChangesAsync(
            AuditLevel auditLevel = AuditLevel.Information,
            CancellationToken cancellationToken = default) =>
            SaveChangesAsync(true, auditLevel, cancellationToken);

        public virtual async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            AuditLevel auditLevel = AuditLevel.Information,
            CancellationToken cancellationToken = default)
        {
            CurrentAuditLevel = auditLevel;
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}