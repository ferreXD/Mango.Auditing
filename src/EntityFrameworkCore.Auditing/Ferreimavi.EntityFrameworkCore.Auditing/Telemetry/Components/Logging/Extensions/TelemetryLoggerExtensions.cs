namespace Mango.Auditing.Telemetry.Components.Logging.Extensions
{
    using Microsoft.Extensions.Logging;

    public static class TelemetryLoggerExtensions
    {
        public static void LogVerbose(this ITelemetryLogger logger, string message, params (string Key, object? Value)[] properties)
            => logger.Log(LogLevel.Trace, message, properties);

        public static void LogDebug(this ITelemetryLogger logger, string message, params (string Key, object? Value)[] properties)
            => logger.Log(LogLevel.Debug, message, properties);

        public static void LogInformation(this ITelemetryLogger logger, string message, params (string Key, object? Value)[] properties)
            => logger.Log(LogLevel.Information, message, properties);

        public static void LogWarning(this ITelemetryLogger logger, string message, params (string Key, object? Value)[] properties)
            => logger.Log(LogLevel.Warning, message, properties);
    }
}