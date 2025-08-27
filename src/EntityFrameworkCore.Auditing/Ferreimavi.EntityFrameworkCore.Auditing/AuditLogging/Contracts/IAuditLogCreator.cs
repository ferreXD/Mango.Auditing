// ReSharper disable once CheckNamespace
namespace Mango.Auditing.AuditLogging
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using System.Threading.Tasks;

    public interface IAuditLogCreator
    {
        Task<AuditLog> CreateAsync(EntityEntry entry, AuditLevel level, CancellationToken cancellationToken);
    }
}
