// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    /// <summary>
    /// Marker interface for the final, decorated metric provider injected into system-wide consumers.
    /// </summary>
    public interface IRootMetricProvider : IMetricProvider
    {
    }
}
