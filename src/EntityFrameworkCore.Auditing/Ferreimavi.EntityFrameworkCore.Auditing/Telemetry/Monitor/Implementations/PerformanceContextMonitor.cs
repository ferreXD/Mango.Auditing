// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class PerformanceContextProvider : IPerformanceContextProvider
    {
        private static readonly AsyncLocal<IDictionary<string, object>> _asyncTags = new();

        public IReadOnlyDictionary<string, object> GetTags()
            => new ReadOnlyDictionary<string, object>(_asyncTags.Value ?? new Dictionary<string, object>());

        public void SetTags(IDictionary<string, object> tags)
            => _asyncTags.Value = new Dictionary<string, object>(tags);

        public void SetTag(string key, object value)
        {
            _asyncTags.Value ??= new Dictionary<string, object>();
            _asyncTags.Value[key] = value;
        }

        public void Clear() => _asyncTags.Value?.Clear();
    }
}
