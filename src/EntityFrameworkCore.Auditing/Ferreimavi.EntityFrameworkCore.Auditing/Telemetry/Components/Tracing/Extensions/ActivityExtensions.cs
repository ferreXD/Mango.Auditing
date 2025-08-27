// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static class ActivityExtensions
    {
        public static Activity? StartSafeActivity(this ActivitySource source, string name, ActivityKind kind = ActivityKind.Internal, IEnumerable<KeyValuePair<string, object?>>? tags = null)
        {
            return source.StartActivity(
                name,
                kind,
                (ActivityContext)default, // links to Activity.Current
                tags,
                null
            );
        }

        public static IDisposable StartSafeScope(this ActivitySource source, string name, ActivityKind kind = ActivityKind.Internal, IEnumerable<KeyValuePair<string, object?>>? tags = null)
        {
            var activity = source.StartSafeActivity(name, kind, tags);
            return new ActivityScopeWrapper(activity);
        }

        public static void SetBaggage(this Activity activity, IDictionary<string, string> baggage)
        {
            if (activity == null || baggage == null) return;

            foreach (var kvp in baggage) activity.AddBaggage(kvp.Key, kvp.Value);
        }

        public static void AppendTag(this Activity? activity, string key, object? value)
        {
            if (activity == null || string.IsNullOrWhiteSpace(key)) return;
            activity.SetTag(key, value);
        }

        public static void AddTags(this Activity? activity, IDictionary<string, object?>? tags)
        {
            if (activity == null || tags == null) return;

            foreach (var kvp in tags)
            {
                activity.AppendTag(kvp.Key, kvp.Value);
            }
        }

        private sealed class ActivityScopeWrapper(Activity? activity) : IDisposable
        {
            private readonly DateTime _start = DateTime.UtcNow;

            public void Dispose()
            {
                if (activity == null) return;

                activity.SetTag("duration_ms", (DateTime.UtcNow - _start).TotalMilliseconds);
                activity.Dispose();
            }

            public void RecordError(IActivityLogger logger, Exception ex)
            {
                logger.RecordException(ex);
            }
        }
    }
}
