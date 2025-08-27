// ReSharper disable once CheckNamespace
namespace Mango.Auditing
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAuditLogDetacher
    {
        /// <summary>
        /// Deletes a batch of <see cref="AuditLog"/> entries by their IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DetachAsync(
            DbContext dbContext,
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default);
    }
}
