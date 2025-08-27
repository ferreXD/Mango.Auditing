// ReSharper disable once CheckNamespace
namespace Mango.Auditing.SoftDeletion
{
    public interface ISoftDeletionMetricRecorder
    {
        void RecordSoftDeleteMetric(
            string type,
            string metric,
            long count,
            params (string Key, object? Value)[] additionalTags);
    }
}
