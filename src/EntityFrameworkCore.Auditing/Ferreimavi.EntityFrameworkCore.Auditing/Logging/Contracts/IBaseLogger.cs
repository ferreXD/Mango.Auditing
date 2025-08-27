// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    public interface IBaseLogger
    {
        /// <summary>
        /// Begins a new operation scope with correlation tracking.
        /// </summary>
        IDisposable BeginScope(string operationName, string? correlationId = null,
            params (string Key, object? Value)[] properties);

        /// <summary>
        /// Begins a new nested scope with additional properties.
        /// </summary>
        IDisposable BeginNestedScope(params (string Key, object? Value)[] extraProps);
    }
}