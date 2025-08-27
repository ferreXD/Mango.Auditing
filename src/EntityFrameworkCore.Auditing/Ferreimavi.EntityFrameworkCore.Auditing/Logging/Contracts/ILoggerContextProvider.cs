// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    public interface ILoggerContextProvider
    {
        LoggerContext GetCurrentContext();
        void SetContext(bool overrideWithDefaults = false);
        void SetContext(LoggerContext context, bool overrideWithDefaults = false);
        void ResetContext();
    }
}