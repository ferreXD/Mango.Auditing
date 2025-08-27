// ReSharper disable once CheckNamespace
namespace Mango.Auditing
{
    using Microsoft.EntityFrameworkCore;

    public interface IAuditTracker
    {
        /// <summary>
        /// Tracks a batch of <see cref="AuditLog"/> entries into the <see cref="DbContext"/> for later persistence.
        /// This method **does not** call <c>SaveChangesAsync</c>; commit responsibility lies with the caller (e.g., EF SaveChanges interceptor).
        /// </summary>
        Task TrackAsync(
            DbContext dbContext,
            IEnumerable<AuditLog> auditLogs,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tracks an audit log entry
        /// </summary>
        /// <param name="auditLog">The audit log to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task TrackAsync(
            DbContext dbContext,
            AuditLog auditLog,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Untracks a batch of <see cref="AuditLog"/> entries by their IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task UntrackAsync(
            DbContext dbContext,
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default);
    }
}
