// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System.Collections.Generic;
    using System.Linq;

    public static class TagMergeExtensions
    {
        public static IDictionary<string, object> Merge(this IReadOnlyDictionary<string, object> source, IDictionary<string, object>? extra)
        {
            var merged = new Dictionary<string, object>(source);
            if (extra == null) return merged;

            foreach (var kv in extra) merged[kv.Key] = kv.Value;
            return merged;
        }

        public static IEnumerable<(string Key, object? Value)> Merge(this IReadOnlyDictionary<string, object> source, params (string Key, object? Value)[] extra)
        {
            var dict = new Dictionary<string, object?>(source.ToDictionary(x => x.Key, x => x.Value)!);
            foreach (var kv in extra) dict[kv.Key] = kv.Value;
            return dict.Select(kv => (kv.Key, kv.Value));
        }
    }

}
