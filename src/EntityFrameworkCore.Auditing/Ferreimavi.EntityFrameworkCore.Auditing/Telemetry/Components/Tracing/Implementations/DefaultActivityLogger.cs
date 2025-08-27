// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Telemetry
{
    using System.Diagnostics;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultActivityLogger" /> class.
    /// </summary>
    /// <param name="sourceName">The name of the activity source; if not provided, a default name is used.</param>
    public class DefaultActivityLogger(ActivitySource activitySource) : IActivityLogger
    {
        // TODO: Add enum like TelemetryTags

        /// <inheritdoc />
        public TResult TraceScope<TResult>(string name, Func<TResult> func)
        {
            using var activity = activitySource.StartSafeActivity(name);

            try
            {
                return func();
            }
            catch (Exception ex)
            {
                RecordException(ex);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<TResult> TraceScopeAsync<TResult>(string name, Func<Task<TResult>> func)
        {
            using var activity = activitySource.StartSafeActivity(name);
            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                RecordException(ex);
                throw;
            }
        }

        public void TraceScope(string name, Action func)
        {
            using var activity = activitySource.StartSafeActivity(name);

            try
            {
                func();
            }
            catch (Exception ex)
            {
                RecordException(ex);
                throw;
            }
        }

        public async Task TraceScopeAsync(string name, Func<Task> func)
        {
            using var activity = activitySource.StartSafeActivity(name);

            try
            {
                await func();
            }
            catch (Exception ex)
            {
                RecordException(ex);
                throw;
            }
        }

        /// <inheritdoc />
        public IDisposable StartActivity(string operationName, ActivityKind kind = ActivityKind.Internal, IDictionary<string, object?>? tags = null)
        {
            var activityTags = tags as IEnumerable<KeyValuePair<string, object?>>
                               ?? tags?.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value))
                               ?? Enumerable.Empty<KeyValuePair<string, object?>>();

            return activitySource.StartSafeScope(operationName, kind, activityTags);
        }

        /// <inheritdoc />
        public void SetBaggage(IDictionary<string, string> items) => Activity.Current?.SetBaggage(items);

        /// <inheritdoc />
        public void RecordEvent(string eventName, IDictionary<string, object?>? tags = null)
        {
            // If there's an active activity, record a new event.
            if (Activity.Current == null) return;

            Activity.Current?.AddTags(tags);
            Activity.Current?.AddEvent(new ActivityEvent(eventName));
        }

        /// <inheritdoc />
        public void RecordException(Exception exception, IDictionary<string, object?>? tags = null)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            var allTags = new Dictionary<string, object?>
            {
                ["exception.type"] = exception.GetType().FullName,
                ["exception.message"] = exception.Message,
                ["exception.stacktrace"] = exception.StackTrace
            };

            if (tags != null)
            {
                foreach (var tag in tags)
                    allTags[tag.Key] = tag.Value;
            }

            Activity.Current?.AddTags(allTags);
            Activity.Current?.AddEvent(new ActivityEvent("exception"));
            Activity.Current?.SetStatus(ActivityStatusCode.Error, exception.Message);
        }

        /// <inheritdoc />
        public Activity? GetCurrentActivity() => Activity.Current;

        /// <inheritdoc />
        public string? GetCurrentTraceId() => Activity.Current?.TraceId.ToString();

        /// <inheritdoc />
        public string? GetCurrentSpanId() => Activity.Current?.SpanId.ToString();

        /// <inheritdoc />
        public void LogCurrentContext()
        {
            var activity = Activity.Current;
            if (activity == null) return;

            var tags = string.Join(", ", activity.Tags.Select(kv => $"{kv.Key}={kv.Value}"));
            var baggage = string.Join(", ", activity.Baggage.Select(kv => $"{kv.Key}={kv.Value}"));

            Console.WriteLine($"[Activity: {activity.DisplayName}] TraceId={activity.TraceId}, SpanId={activity.SpanId}");
            Console.WriteLine($"Tags: {tags}");
            Console.WriteLine($"Baggage: {baggage}");
        }
    }
}