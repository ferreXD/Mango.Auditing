// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Telemetry
{
    using Logging;
    using Microsoft.Extensions.Logging;

    public interface ITelemetryLogger : IBaseLogger
    {
        /// <summary>
        /// Logs an audit event with the specified level and enriched context.
        /// </summary>
        void Log(LogLevel logLevel, string message, params (string Key, object? Value)[]? properties);

        /// <summary>
        /// Logs an audit event with exception details.
        /// </summary>
        void LogError(Exception exception, string message, params (string Key, object? Value)[]? properties);
    }
}