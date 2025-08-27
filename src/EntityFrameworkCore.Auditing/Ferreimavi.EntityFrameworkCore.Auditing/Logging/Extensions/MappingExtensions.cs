// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    using Microsoft.Extensions.Logging;

    public static class MappingExtensions
    {
        public static LogLevel ToLogLevel(this AuditLevel auditLevel)
        {
            return auditLevel switch
            {
                AuditLevel.Information => LogLevel.Information,
                AuditLevel.Debug => LogLevel.Debug,
                AuditLevel.Warning => LogLevel.Warning,
                AuditLevel.Error => LogLevel.Error,
                AuditLevel.Critical => LogLevel.Critical,
                _ => throw new UnhandledEnumValueException<AuditLevel>(auditLevel)
            };
        }

        public static AuditLevel ToAuditLevel(this LogLevel LogLevel)
        {
            return LogLevel switch
            {
                LogLevel.Information => AuditLevel.Information,
                LogLevel.Debug => AuditLevel.Debug,
                LogLevel.Warning => AuditLevel.Warning,
                LogLevel.Error => AuditLevel.Error,
                LogLevel.Critical => AuditLevel.Critical,
                _ => throw new UnhandledEnumValueException<LogLevel>(LogLevel)
            };
        }
    }
}