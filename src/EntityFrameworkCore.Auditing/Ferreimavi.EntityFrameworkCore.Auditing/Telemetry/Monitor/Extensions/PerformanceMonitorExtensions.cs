// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Telemetry
{
    public static class PerformanceMonitorExtensions
    {
        public static void LogVerbose(this IPerformanceMonitor monitor, string message, params (string Key, object? Value)[] properties)
            => monitor.Verbose(message, properties);

        public static void LogDebug(this IPerformanceMonitor monitor, string message, params (string Key, object? Value)[] properties)
            => monitor.Debug(message, properties);

        public static void LogInformation(this IPerformanceMonitor monitor, string message, params (string Key, object? Value)[] properties)
            => monitor.Info(message, properties);

        public static void LogWarning(this IPerformanceMonitor monitor, string message, params (string Key, object? Value)[] properties)
            => monitor.Warn(message, properties);

        public static void LogError(this IPerformanceMonitor monitor, Exception ex, string message, params (string Key, object? Value)[] properties)
            => monitor.Error(message, ex, properties);
    }
}