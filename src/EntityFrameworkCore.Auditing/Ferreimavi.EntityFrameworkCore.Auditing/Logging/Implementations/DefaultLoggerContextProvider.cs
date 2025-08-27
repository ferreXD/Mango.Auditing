// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    public class DefaultLoggerContextProvider(
        ICurrentUserProvider currentUserProvider)
        : ILoggerContextProvider
    {
        private readonly AsyncLocal<LoggerContext> _currentContext = new();

        public LoggerContext GetCurrentContext()
        {
            var context = _currentContext.Value ?? new LoggerContext();

            return context;
        }

        public void SetContext(bool overrideWithDefaults = false) => SetContext(new LoggerContext(), overrideWithDefaults);

        public void SetContext(LoggerContext context, bool overrideWithDefaults = false)
        {
            _currentContext.Value = context ?? throw new ArgumentNullException(nameof(context));

            if (overrideWithDefaults)
            {
                _currentContext.Value.UserId = currentUserProvider.GetCurrentUserId();
                _currentContext.Value.UserName = currentUserProvider.GetCurrentUserName();
            }
        }

        public void ResetContext() => _currentContext.Value = new LoggerContext();
    }
}