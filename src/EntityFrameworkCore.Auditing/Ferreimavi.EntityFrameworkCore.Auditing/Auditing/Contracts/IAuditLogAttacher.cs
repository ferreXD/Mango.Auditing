// ReSharper disable once CheckNamespace
namespace Mango.Auditing
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAuditLogAttacher
    {
        /// <summary>
        /// Saves a batch of <see cref="AuditLog"/> entries into the <see cref="DbContext"/> for later persistence.
        /// This method **does not** call <c>SaveChangesAsync</c>; commit responsibility lies with the caller (e.g., EF SaveChanges interceptor).
        /// </summary>
        Task AttachAsync(DbContext dbContext, IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken = default);
    }
}
