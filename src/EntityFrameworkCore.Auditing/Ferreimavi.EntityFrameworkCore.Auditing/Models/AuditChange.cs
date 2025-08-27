// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    public class AuditChange
    {
        public string PropertyName { get; set; } = string.Empty;
        public object? OriginalValue { get; set; }
        public object? NewValue { get; set; }
        public bool IsSensitive { get; set; }
    }
}