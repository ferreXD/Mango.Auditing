// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;

    public class LoggerContext
    {
        public string? CorrelationId { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? OperationName { get; set; }
        public DateTime Timestamp { get; set; }
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public IDictionary<string, object?> AdditionalData { get; set; } = new Dictionary<string, object?>();
        public PathString RequestPath { get; set; }
        public StringValues UserAgent { get; set; }
        public string? RequestMethod { get; set; }
    }
}