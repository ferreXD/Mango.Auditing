// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    public interface ILogMonitor
    {
        void Verbose(string message, params (string Key, object? Value)[] properties);
        void Info(string message, params (string Key, object? Value)[] properties);
        void Warn(string message, params (string Key, object? Value)[] properties);
        void Error(string message, Exception ex, params (string Key, object? Value)[] properties);
        void Debug(string message, params (string Key, object? Value)[] properties);
    }
}
