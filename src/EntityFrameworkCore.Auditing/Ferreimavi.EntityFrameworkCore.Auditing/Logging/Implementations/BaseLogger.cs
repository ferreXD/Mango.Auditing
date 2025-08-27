// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    using Microsoft.Extensions.Logging;
    using System.Diagnostics;
    using Telemetry;

    public abstract class BaseLogger(
        ILoggerFactory loggerFactory,
        ILoggerContextProvider contextProvider,
        LoggerOptions? options = null) : IBaseLogger
    {
        private readonly ILoggerFactory _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        private readonly ILoggerContextProvider _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
        private readonly LoggerOptions _options = options ?? new LoggerOptions();
        private readonly AsyncLocal<LoggerContext> _currentContext = new();

        protected LoggerContext GetOrCreateContext()
        {
            if (_currentContext.Value != null) return _currentContext.Value;

            if (_options.EnableActivityFallback && Activity.Current is { } activity)
            {
                var fromActivity = new LoggerContext
                {
                    CorrelationId = activity.TraceId.ToString(),
                    OperationName = activity.DisplayName,
                    Timestamp = DateTime.UtcNow,
                };

                _currentContext.Value = fromActivity;
                return fromActivity;
            }

            return new LoggerContext();
        }

        public virtual void Write(LogLevel level, string message, params (string Key, object? Value)[]? properties)
        {
            var context = GetOrCreateContext();
            var category = context.OperationName ?? _options.DefaultCategory;
            var logger = _loggerFactory.CreateLogger(category);

            if (!logger.IsEnabled(level))
            {
                Debug.WriteLine($"[Logger] Log level {level} is not enabled for category '{category}'. Message: {message}");
                return;
            }

            try
            {
                if (properties == null || properties.Length == 0)
                {
                    logger.Log(level, message);
                    return;
                }

                using var scope = logger.BeginScope(properties.ToDictionary(p => p.Key, p => p.Value));
                logger.Log(level, message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Logger] Logging failed: {ex.Message}");
            }
        }

        public virtual void WriteError(Exception exception, string message, params (string Key, object? Value)[]? properties)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            var context = GetOrCreateContext();
            var category = context.OperationName ?? _options.DefaultCategory;
            var logger = _loggerFactory.CreateLogger(category);

            try
            {
                if (properties == null || properties.Length == 0)
                {
                    logger.LogError(exception, message);
                    return;
                }

                using var scope = logger.BeginScope(properties.ToDictionary(p => p.Key, p => p.Value));
                logger.LogError(exception, message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Logger] Error logging exception: {ex.Message}");
            }
        }

        public virtual IDisposable BeginScope(string operationName, string? correlationId = null,
            params (string Key, object? Value)[] properties)
        {
            var context = new LoggerContext
            {
                OperationName = operationName,
                CorrelationId = correlationId ?? Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N"),
                Timestamp = DateTime.UtcNow
            };

            foreach (var (key, value) in properties)
                context.AdditionalData[key] = value;

            _currentContext.Value = context;
            _contextProvider.SetContext(context);

            var category = context.OperationName ?? _options.DefaultCategory;
            var logger = _loggerFactory.CreateLogger(category);
            var fullScope = LogEventHelper.CreateLogProperties(context, []).ToDictionary(p => p.Key, p => p.Value);

            return logger.BeginScope(fullScope)!;
        }

        public IDisposable BeginNestedScope(params (string Key, object? Value)[] extraProps)
        {
            var context = GetOrCreateContext();
            if (_currentContext.Value == null)
            {
                _currentContext.Value = context;
                _contextProvider.SetContext(context);
            }

            var category = context.OperationName ?? _options.DefaultCategory;
            var logger = _loggerFactory.CreateLogger(category);

            return logger.BeginScope(extraProps.ToDictionary(p => p.Key, p => p.Value))!;
        }
    }
}