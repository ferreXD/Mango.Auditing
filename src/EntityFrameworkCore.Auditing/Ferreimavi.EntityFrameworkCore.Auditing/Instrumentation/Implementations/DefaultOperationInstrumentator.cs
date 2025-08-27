// ReSharper disable once CheckNamespace
namespace Mango.Auditing
{
    using System.Collections;
    using System.Diagnostics;
    using Telemetry;

    public class DefaultOperationInstrumentator(IPerformanceMonitor monitor) : IOperationInstrumentator
    {
        public virtual void Instrument(
            string name,
            Action action,
            Dictionary<string, object>? metadata = null,
            Action<Exception>? onFailure = null)
            => Instrument(name, () =>
            {
                action();
                return true;
            }, metadata, onFailure);

        public virtual TResult Instrument<TResult>(
            string name,
            Func<TResult> func,
            Dictionary<string, object>? metadata = null,
            Action<Exception>? onFailure = null)
            => InstrumentCore(name, () => Task.FromResult(func()), metadata, onFailure).GetAwaiter().GetResult();

        public virtual async Task<TResult> InstrumentAsync<TResult>(
            string name,
            Func<Task<TResult>> func,
            Dictionary<string, object>? metadata = null,
            Action<Exception>? onFailure = null)
            => await InstrumentCore(name, func, metadata, onFailure);

        private async Task<TResult> InstrumentCore<TResult>(
            string name,
            Func<Task<TResult>> func,
            Dictionary<string, object>? metadata,
            Action<Exception>? onFailure = null)
        {
            using var scope = monitor.BeginOperation(name);
            var stopwatch = Stopwatch.StartNew();
            TResult? result;

            try
            {
                result = await func();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                monitor.Error($"{name} failed after {stopwatch.ElapsedMilliseconds} ms", ex, ("DurationMs", stopwatch.ElapsedMilliseconds));
                monitor.RecordMetric($"{name}.Failure", 1, MetricType.Counter);

                onFailure?.Invoke(ex); // <-- Hook for contextual handling (i.e. alerting, contextual logging, etc.)

                throw;
            }

            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;
            var resultType = typeof(TResult) == typeof(void) ? "void" : result?.GetType().Name ?? "null";

            var tags = new Dictionary<string, object>
            {
                { "DurationMs", duration },
                { "ResultType", resultType }
            };

            if (metadata != null)
            {
                foreach (var kvp in metadata)
                {
                    tags[kvp.Key] = kvp.Value;
                }
            }

            monitor.RecordEvent($"{name} completed", tags);
            monitor.RecordMetric($"{name}.Duration", duration, MetricType.Histogram, tags);
            monitor.Info($"{name} executed in {duration} ms", ("DurationMs", duration), ("ResultType", resultType));

            if (result is IEnumerable enumerable and not string)
            {
                var count = enumerable.Cast<object>().Count();
                var countTags = new Dictionary<string, object> { { "ResultType", resultType } };

                monitor.RecordEvent($"{name} returned {count} items", new Dictionary<string, object> { { "Count", count } });
                monitor.RecordMetric($"{name}.Count", count, MetricType.Counter, countTags);
                monitor.Info($"{name} returned {count} items", ("Count", count), ("ResultType", resultType));
            }

            return result;
        }

    }
}
