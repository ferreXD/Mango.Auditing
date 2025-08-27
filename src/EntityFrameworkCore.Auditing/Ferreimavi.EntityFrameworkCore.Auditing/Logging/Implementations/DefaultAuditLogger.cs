// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    using Microsoft.Extensions.Logging;

    public class DefaultAuditLogger(
        ILoggerFactory loggerFactory,
        ILoggerContextProvider contextProvider)
        : BaseLogger(loggerFactory, contextProvider), IAuditLogger
    {
        public void Log(AuditLevel level, string message, params (string Key, object? Value)[]? properties) =>
            base.Write(level.ToLogLevel(), message, properties);

        public void LogError(Exception exception, string message, params (string Key, object? Value)[]? properties) =>
            base.WriteError(exception, message, properties);
    }
}