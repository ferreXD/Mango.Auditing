// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    using Microsoft.AspNetCore.Http;
    using Telemetry;

    public class DefaultHttpLoggerContextProvider(
        IHttpContextAccessor httpContextAccessor,
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

            // Enrich with HTTP context if available
            if (httpContextAccessor.HttpContext != null && overrideWithDefaults)
            {
                var httpContext = httpContextAccessor.HttpContext;
                _currentContext.Value.RequestPath = httpContext.Request.Path;
                _currentContext.Value.RequestMethod = httpContext.Request.Method;
                _currentContext.Value.UserAgent = httpContext.Request.Headers["User-Agent"];

                // Add user information if authenticated
                if (httpContext.User?.Identity?.IsAuthenticated == true)
                {
                    _currentContext.Value.UserId = currentUserProvider.GetCurrentUserId();
                    _currentContext.Value.UserName = currentUserProvider.GetCurrentUserName();
                }
            }
        }

        public void ResetContext() => _currentContext.Value = new LoggerContext();
    }
}