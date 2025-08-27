// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using Microsoft.Extensions.Logging;
    using System;

    public class DefaultLogMonitor(ITelemetryLogger logger, IPerformanceContextProvider context) : ILogMonitor
    {
        private readonly ITelemetryLogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public void Verbose(string message, params (string Key, object? Value)[] properties)
            => _logger.Log(LogLevel.Trace, message, context.GetTags().Merge(properties).ToArray());

        public void Debug(string message, params (string Key, object? Value)[] properties)
            => _logger.Log(LogLevel.Debug, message, context.GetTags().Merge(properties).ToArray());

        public void Info(string message, params (string Key, object? Value)[] properties)
            => _logger.Log(LogLevel.Information, message, context.GetTags().Merge(properties).ToArray());

        public void Warn(string message, params (string Key, object? Value)[] properties)
            => _logger.Log(LogLevel.Warning, message, context.GetTags().Merge(properties).ToArray());

        public void Error(string message, Exception ex, params (string Key, object? Value)[] properties)
            => _logger.LogError(ex, message, context.GetTags().Merge(properties).ToArray());
    }
}
