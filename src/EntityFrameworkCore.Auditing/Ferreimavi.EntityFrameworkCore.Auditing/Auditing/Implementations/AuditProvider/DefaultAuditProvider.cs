// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using Telemetry;

    /// <summary>
    /// Default implementation of the IAuditProvider interface
    /// </summary>
    public class DefaultAuditProvider<T>(IAuditReader auditReader, IAuditLogger logger, IPerformanceMonitor monitor) : IAuditProvider where T : DbContext
    {
        private readonly IAuditLogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IPerformanceMonitor _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));

        /// <inheritdoc />
        public async Task<AuditLog?> GetAuditLogAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using var op = await _monitor.BeginOperationAsync("audit.provider.get_by_id", ActivityKind.Internal, new Dictionary<string, object> { { "id", id } }, cancellationToken);

            try
            {
                return await auditReader.GetAuditLogAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                _monitor.RecordException(ex, new Dictionary<string, object>
                {
                    { "AuditLogId", id },
                    { "Error", ex.Message }
                });

                _monitor.LogError(ex, "Failed to retrieve audit log by id", ("AuditLogId", id));
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLog>> GetAuditLogsForEntityAsync(
            string entityType,
            string entityId,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
        {
            using var op = await _monitor.BeginOperationAsync("audit.provider.get_by_entity", ActivityKind.Internal, new Dictionary<string, object> { { "entityType", entityType }, { "entityId", entityId } }, cancellationToken);

            try
            {
                return await auditReader.GetAuditLogsForEntityAsync(entityType, entityId, skip, take, cancellationToken);
            }
            catch (Exception ex)
            {
                _monitor.RecordException(ex, new Dictionary<string, object>
                {
                    { "EntityType", entityType },
                    { "EntityId", entityId },
                    { "Error", ex.Message }
                });

                _monitor.LogError(ex, "Failed to retrieve audit logs for entity", ("EntityType", entityType), ("EntityId", entityId));
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLog>> GetAuditLogsForUserAsync(
            string userId,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
        {
            using var op = await _monitor.BeginOperationAsync("audit.provider.get_by_user", ActivityKind.Internal, new Dictionary<string, object> { { "userId", userId } }, cancellationToken);

            try
            {
                return await auditReader.GetAuditLogsForUserAsync(userId, skip, take, cancellationToken);
            }
            catch (Exception ex)
            {
                _monitor.RecordException(ex, new Dictionary<string, object>
                {
                    { "UserId", userId },
                    { "Error", ex.Message }
                });

                _monitor.LogError(ex, "Failed to retrieve audit logs for user", ("UserId", userId));
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLog>> GetAuditLogsByActionAsync(
            string operation,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
        {
            using var op = await _monitor.BeginOperationAsync("audit.provider.get_by_action", ActivityKind.Internal, new Dictionary<string, object> { { "operation", operation } }, cancellationToken);

            try
            {
                return await auditReader.GetAuditLogsByActionAsync(operation, skip, take, cancellationToken);
            }
            catch (Exception ex)
            {
                _monitor.RecordException(ex, new Dictionary<string, object>
                {
                    { "Operation", operation },
                    { "Error", ex.Message }
                });

                _monitor.LogError(ex, "Failed to retrieve audit logs for operation", ("Operation", operation));
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            using var op = await _monitor.BeginOperationAsync("audit.provider.get_recent", ActivityKind.Internal, new Dictionary<string, object> { { "count", count } }, cancellationToken);

            try
            {
                return await auditReader.GetRecentAuditLogsAsync(count, cancellationToken);
            }
            catch (Exception ex)
            {
                _monitor.RecordException(ex, new Dictionary<string, object>
                {
                    { "Count", count },
                    { "Error", ex.Message }
                });

                _monitor.LogError(ex, "Failed to retrieve recent audit logs");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLog>> GetAuditLogsInDateRangeAsync(
            DateTime? startDate,
            DateTime? endDate,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
        {
            startDate ??= DateTime.MinValue;
            endDate ??= DateTime.MaxValue;

            using var op = await _monitor.BeginOperationAsync("audit.provider.get_by_date_range", ActivityKind.Internal, new Dictionary<string, object>
            {
                { "start", startDate },
                { "end", endDate }
            }, cancellationToken);

            try
            {
                return await auditReader.GetAuditLogsInDateRangeAsync(startDate, endDate, skip, take, cancellationToken);
            }
            catch (Exception ex)
            {
                _monitor.RecordException(ex, new Dictionary<string, object>
                {
                    { "StartDate", startDate },
                    { "EndDate", endDate },
                    { "Error", ex.Message }
                });

                _monitor.LogError(ex, "Failed to retrieve audit logs between date range", ("StartDate", startDate), ("EndDate", endDate));
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
        {
            return await auditReader.GetAuditLogsAsync(skip, take, cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByFilterExpression(
            Expression<Func<AuditLog, bool>> predicate,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
        {
            using var op = await _monitor.BeginOperationAsync("audit.provider.get_by_expression", ActivityKind.Internal, new Dictionary<string, object>
            {
                { "expression", predicate.ToString() }
            }, cancellationToken);

            try
            {

                return await auditReader.GetAuditLogsByFilterExpression(predicate, skip, take, cancellationToken);
            }
            catch (Exception ex)
            {
                _monitor.RecordException(ex, new Dictionary<string, object>
                {
                    { "Expression", predicate.ToString() },
                    { "Error", ex.Message }
                });

                _monitor.LogError(ex, "Failed to retrieve audit logs for expression", ("Expression", predicate.ToString()));
                throw;
            }
        }
    }
}