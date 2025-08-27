// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    using Microsoft.AspNetCore.Http;
    using Serilog.Core;
    using Serilog.Events;

    public class TraceIdentifierEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TraceIdentifierEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var traceId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            if (string.IsNullOrEmpty(traceId)) return;

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "TraceId", traceId));
        }
    }
}