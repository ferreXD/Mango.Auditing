// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Security
{
    public class ConfigurableSensitiveDataFilter : ISensitiveDataFilter
    {
        private readonly Dictionary<string, HashSet<string>> _entitySpecificRules;
        private readonly HashSet<string> _globalPatterns;
        private readonly string _maskValue;

        public ConfigurableSensitiveDataFilter(
            IEnumerable<(string EntityType, string PropertyName)> sensitiveProperties,
            IEnumerable<string>? globalPatterns = null,
            string maskValue = "***MASKED***")
        {
            _entitySpecificRules = sensitiveProperties
                .GroupBy(x => x.EntityType)
                .ToDictionary(
                    g => g.Key,
                    g => new HashSet<string>(g.Select(x => x.PropertyName), StringComparer.OrdinalIgnoreCase)
                );

            _globalPatterns = new HashSet<string>(
                globalPatterns ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase
            );

            _maskValue = maskValue;
        }

        public bool IsSensitive(string entityType, string propertyName)
        {
            if (!_entitySpecificRules.TryGetValue(entityType, out var properties))
                return _globalPatterns.Any(pattern => propertyName.Contains(pattern, StringComparison.OrdinalIgnoreCase));

            return properties.Contains(propertyName) || _globalPatterns.Any(pattern => propertyName.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        public object? MaskValue(object? value) => _maskValue;
    }
}