// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    using Microsoft.AspNetCore.Http;
    using Serilog.Core;
    using Serilog.Events;
    using System.Security.Claims;

    public class SecurityContextEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SecurityContextEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.User?.Identity?.IsAuthenticated != true) return;

            var claims = context.User.Claims.ToDictionary(
                c => c.Type,
                c => c.Value);

            var roles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "UserRoles", roles));

            if (claims.TryGetValue(ClaimTypes.NameIdentifier, out var userId)) logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));

            if (claims.TryGetValue(ClaimTypes.Name, out var userName)) logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserName", userName));

            if (claims.TryGetValue(ClaimTypes.Email, out var email)) logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserEmail", email));

            if (claims.TryGetValue("scope", out var scope)) logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("AuthScope", scope));
        }
    }
}