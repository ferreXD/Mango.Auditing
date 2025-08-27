// ReSharper disable once CheckNamespace
namespace Mango.Auditing
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Telemetry;

    public class DefaultAuditLogReader<T>(T dbContext, IPerformanceMonitor monitor) : IAuditReader
        where T : DbContext
    {
        public async Task<AuditLog?> GetAuditLogAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var response = await dbContext
                .Set<AuditLog>()
                .FindAsync(new object[] { id }, cancellationToken);

            monitor.LogDebug("Audit log retrieved by id", ("AuditLogId", id));

            return response;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLog>> GetAuditLogsForEntityAsync(
            string entityType,
            string entityId,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
            => await GetAuditLogsByFilterExpressionAsync(a => a.EntityType == entityType && a.EntityId == entityId, skip, take, cancellationToken);

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLog>> GetAuditLogsForUserAsync(
            string userId,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
            => await GetAuditLogsByFilterExpressionAsync(a => a.UserId == userId, skip, take, cancellationToken);

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLog>> GetAuditLogsByActionAsync(
            string operation,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
            => await GetAuditLogsByFilterExpressionAsync(a => a.Operation == operation, skip, take, cancellationToken);

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(
            int count = 10,
            CancellationToken cancellationToken = default)
            => await GetAuditLogsByFilterExpressionAsync(null, 0, count, cancellationToken);

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLog>> GetAuditLogsInDateRangeAsync(
            DateTime? startDate,
            DateTime? endDate,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
            => await GetAuditLogsByFilterExpressionAsync(a => a.Timestamp >= startDate && a.Timestamp <= endDate, skip, take, cancellationToken);

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
            => await GetAuditLogsByFilterExpressionAsync(null, skip, take, cancellationToken);

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByFilterExpression(
            Expression<Func<AuditLog, bool>> predicate,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
            => await GetAuditLogsByFilterExpressionAsync(predicate, skip, take, cancellationToken);

        private async Task<List<AuditLog>> GetAuditLogsByFilterExpressionAsync(
            Expression<Func<AuditLog, bool>>? predicate,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
        {
            predicate ??= _ => true;

            var response = await dbContext.Set<AuditLog>()
                .Where(predicate)
                .Skip(skip)
                .Take(take)
                .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);


            monitor.LogDebug("Audit logs query executed",
                ("ResultCount", response.Count),
                ("Predicate", predicate?.ToString() ?? "null")
            );

            return response;
        }
    }
}
