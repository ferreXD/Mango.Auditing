// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Security
{
    public interface ISensitiveDataFilter
    {
        bool IsSensitive(string entityType, string propertyName);
        object? MaskValue(object? value);
    }
}