// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    /// <summary>
    ///     Represents an exception that is thrown when there is a configuration error in the audit logging.
    /// </summary>
    internal class AuditConfigurationException(string message) : Exception(message);
}