// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    /// <summary>
    /// Defines the contract for audit logging operations.
    /// </summary>
    public interface IAuditLogger : IBaseLogger
    {
        /// <summary>
        /// Logs an audit event with the specified level and enriched context.
        /// </summary>
        void Log(AuditLevel level, string message, params (string Key, object? Value)[]? properties);

        /// <summary>
        /// Logs an audit event with exception details.
        /// </summary>
        void LogError(Exception exception, string message, params (string Key, object? Value)[]? properties);
    }
}