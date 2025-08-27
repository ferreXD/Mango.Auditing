// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? AffectedColumns { get; set; }
        public string? PrimaryKey { get; set; }
        public string? TableName { get; set; }
        public AuditLevel Level { get; set; } = AuditLevel.Information;
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}