// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    using System.Linq.Expressions;

    /// <summary>
    /// Provides functionality for saving and retrieving audit logs
    /// </summary>
    public interface IAuditProvider
    {
        /// <summary>
        /// Retrieves all audit logs
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns>A list with all audit logs</returns>
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an audit log by its ID
        /// </summary>
        /// <param name="id">The ID of the audit log to retrieve</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The audit log if found, null otherwise</returns>
        Task<AuditLog?> GetAuditLogAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves audit logs for a specific entity
        /// </summary>
        /// <param name="entityType">The type of the entity</param>
        /// <param name="entityId">The ID of the entity</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of audit logs for the entity</returns>
        Task<IEnumerable<AuditLog>> GetAuditLogsForEntityAsync(
            string entityType,
            string entityId,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves audit logs for a specific user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of audit logs for the user</returns>
        Task<IEnumerable<AuditLog>> GetAuditLogsForUserAsync(
            string userId,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves audit logs within a specific date range
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of audit logs within the date range</returns>
        Task<IEnumerable<AuditLog>> GetAuditLogsInDateRangeAsync(
            DateTime? startDate,
            DateTime? endDate,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves audit logs with a specific operation type
        /// </summary>
        /// <param name="operation">The operation type</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of audit logs with the specified operation type</returns>
        Task<IEnumerable<AuditLog>> GetAuditLogsByActionAsync(
            string operation,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves audit logs with a specific entity type
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A collection of audit logs with the specified entity type</returns>
        Task<IEnumerable<AuditLog>> GetAuditLogsByFilterExpression(
            Expression<Func<AuditLog, bool>> predicate,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the most recent audit logs
        /// </summary>
        /// <param name="count">The number of logs to retrieve</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of the most recent audit logs</returns>
        Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(
            int count = 10,
            CancellationToken cancellationToken = default);
    }
}