// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Security
{
    public class DefaultSensitiveDataFilter(string maskValue = "***MASKED***") : ISensitiveDataFilter
    {
        private readonly HashSet<string> _sensitivePatterns = new(StringComparer.OrdinalIgnoreCase)
        {
            "password",
            "secret",
            "creditcard",
            "ssn",
            "securitycode",
            "pin",
            "privatekey",
            "token",
            "apikey",
            "accesskey",
            "pwd",
            "pass",
            "cvv",
            "social"
        };

        public bool IsSensitive(string entityType, string propertyName)
            => _sensitivePatterns.Any(pattern => propertyName.Contains(pattern, StringComparison.OrdinalIgnoreCase));

        public object? MaskValue(object? value) => maskValue;
    }
}