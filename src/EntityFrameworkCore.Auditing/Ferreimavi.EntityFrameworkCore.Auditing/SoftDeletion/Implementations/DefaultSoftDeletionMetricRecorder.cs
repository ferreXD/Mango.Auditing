// ReSharper disable once CheckNamespace
namespace Mango.Auditing.SoftDeletion
{
    using Telemetry;

    public class DefaultSoftDeletionMetricRecorder(
        IPerformanceMonitor monitor,
        ICurrentUserProvider currentUserProvider) : ISoftDeletionMetricRecorder
    {
        public void RecordSoftDeleteMetric(string type, string metric, long count, params (string Key, object? Value)[] additionalTags)
        {
            var tags = new Dictionary<string, object>
            {
                { "EntityType", type },
                { "UserId", currentUserProvider.GetCurrentUserId() ?? string.Empty }
            };

            if (additionalTags.Length > 0)
            {
                foreach (var tag in additionalTags)
                {
                    if (!tags.ContainsKey(tag.Key)) tags[tag.Key] = tag.Value!;
                }
            }

            monitor.RecordMetric(
                $"SoftDeletion.{metric}",
                count,
                MetricType.Histogram,
                tags
            );
        }
    }
}
