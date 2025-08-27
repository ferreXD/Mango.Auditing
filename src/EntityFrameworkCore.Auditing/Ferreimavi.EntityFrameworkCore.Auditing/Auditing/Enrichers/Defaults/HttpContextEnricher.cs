// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Enrichers
{
    using Auditing.Enrichers.Models;
    using Microsoft.AspNetCore.Http;

    public class HttpContextEnricher(IHttpContextAccessor httpContextAccessor) : IAuditEnricher
    {
        public int Order => 200;

        public void Enrich(AuditLog auditLog, EnrichmentContext context)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            auditLog.Metadata["HttpContext.RequestPath"] = httpContext.Request.Path;
            auditLog.Metadata["HttpContext.RequestMethod"] = httpContext.Request.Method;
            auditLog.Metadata["HttpContext.CorrelationId"] = httpContext.TraceIdentifier;
            auditLog.Metadata["HttpContext.IpAddress"] = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            auditLog.Metadata["HttpContext.Host"] = httpContext.Request.Host.ToString();
            auditLog.Metadata["HttpContext.Protocol"] = httpContext.Request.Protocol;
            auditLog.Metadata["HttpContext.Scheme"] = httpContext.Request.Scheme;

            httpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent);
            if (userAgent.Any()) auditLog.Metadata["HttpContext.UserAgent"] = userAgent.ToString();

            // Si hay claims de identidad, los incluimos también
            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                auditLog.Metadata["HttpContext.AuthenticationType"] = httpContext.User.Identity.AuthenticationType ?? "Unknown";

                var claims = httpContext
                    .User
                    .Claims
                    .ToDictionary(
                        c => $"Claim_{c.Type}",
                        c => c.Value
                    );

                foreach (var claim in claims)
                {
                    auditLog.Metadata[$"HttpContext.{claim.Key}"] = claim.Value;
                }
            }
        }

        public Task EnrichAsync(AuditLog auditLog, EnrichmentContext context)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null) return Task.CompletedTask;

            auditLog.Metadata["HttpContext.RequestPath"] = httpContext.Request.Path;
            auditLog.Metadata["HttpContext.RequestMethod"] = httpContext.Request.Method;
            auditLog.Metadata["HttpContext.CorrelationId"] = httpContext.TraceIdentifier;
            auditLog.Metadata["HttpContext.IpAddress"] = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            auditLog.Metadata["HttpContext.Host"] = httpContext.Request.Host.ToString();
            auditLog.Metadata["HttpContext.Protocol"] = httpContext.Request.Protocol;
            auditLog.Metadata["HttpContext.Scheme"] = httpContext.Request.Scheme;

            httpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent);
            if (userAgent.Any()) auditLog.Metadata["HttpContext.UserAgent"] = userAgent.ToString();

            // Si hay claims de identidad, los incluimos también
            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                auditLog.Metadata["HttpContext.AuthenticationType"] = httpContext.User.Identity.AuthenticationType ?? "Unknown";

                var claims = httpContext
                    .User
                    .Claims
                    .ToDictionary(
                        c => $"Claim_{c.Type}",
                        c => c.Value
                    );

                foreach (var claim in claims)
                {
                    auditLog.Metadata[$"HttpContext.{claim.Key}"] = claim.Value;
                }
            }

            return Task.CompletedTask;
        }
    }
}