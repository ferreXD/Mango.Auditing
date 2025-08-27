// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    using Microsoft.AspNetCore.Http;
    using System.Security.Claims;

    /// <summary>
    /// Implementation of ICurrentUserProvider that retrieves user information from the HTTP context
    /// </summary>
    public class DefaultHttpContextCurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
    {
        /// <inheritdoc />
        public string? GetCurrentUserId() => httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        /// <inheritdoc />
        public string? GetCurrentUserName() => httpContextAccessor.HttpContext?.User?.Identity?.Name;

        /// <inheritdoc />
        public IDictionary<string, string> GetAdditionalUserInfo()
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null) return new Dictionary<string, string>();

            var additionalInfo = new Dictionary<string, string>();

            // Add common claims
            TryAddClaim(ClaimTypes.Email, "Email");
            TryAddClaim(ClaimTypes.GivenName, "FirstName");
            TryAddClaim(ClaimTypes.Surname, "LastName");
            TryAddClaim(ClaimTypes.Role, "Roles", true);

            // Add custom claims with "custom:" prefix
            var customClaims = user.Claims.Where(c =>
                !c.Type.StartsWith("http://") &&
                !c.Type.StartsWith("https://") &&
                c.Type != ClaimTypes.NameIdentifier &&
                c.Type != ClaimTypes.Name);

            foreach (var claim in customClaims) additionalInfo[$"custom:{claim.Type}"] = claim.Value;

            return additionalInfo;

            void TryAddClaim(string claimType, string key, bool isMultiValue = false)
            {
                if (isMultiValue)
                {
                    var values = user.Claims
                        .Where(c => c.Type == claimType)
                        .Select(c => c.Value)
                        .ToList();

                    if (values.Any()) additionalInfo[key] = string.Join(",", values);
                }
                else
                {
                    var value = user.FindFirst(claimType)?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(value)) additionalInfo[key] = value;
                }
            }
        }
    }
}