// ReSharper disable once CheckNamespace
namespace Mango.Auditing
{
    using System.Linq.Expressions;

    public interface IAuditReader
    {
        Task<AuditLog?> GetAuditLogAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<AuditLog>> GetAuditLogsForEntityAsync(string entityType, string entityId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<AuditLog>> GetAuditLogsForUserAsync(string userId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<AuditLog>> GetAuditLogsByActionAsync(string operation, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count = 10, CancellationToken cancellationToken = default);
        Task<IEnumerable<AuditLog>> GetAuditLogsInDateRangeAsync(DateTime? startDate, DateTime? endDate, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<AuditLog>> GetAuditLogsByFilterExpression(Expression<Func<AuditLog, bool>> predicate, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    }
}
