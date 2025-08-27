// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    using Microsoft.Extensions.Logging;
    using Telemetry;

    public class DefaultTelemetryLogger(
        ILoggerFactory loggerFactory,
        ILoggerContextProvider contextProvider) : BaseLogger(loggerFactory, contextProvider), ITelemetryLogger
    {
        public void Log(LogLevel level, string message, params (string Key, object? Value)[]? properties) =>
            base.Write(level, message, properties);

        public void LogError(Exception exception, string message, params (string Key, object? Value)[]? properties) =>
            base.WriteError(exception, message, properties);
    }
}