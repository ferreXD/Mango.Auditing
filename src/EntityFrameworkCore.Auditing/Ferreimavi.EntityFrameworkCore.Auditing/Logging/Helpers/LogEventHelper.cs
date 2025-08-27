// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Telemetry
{
    using Logging;

    public static class LogEventHelper
    {
        public static IEnumerable<(string Key, object? Value)> CreateLogProperties(LoggerContext context, (string Key, object? Value)[]? properties)
        {
            var baseProperties = CreateBaseLogProperties(context);
            var customProperties = properties?.Select(p => (p.Key, p.Value)) ?? [];
            return baseProperties.Concat(customProperties).ToArray();
        }

        private static IEnumerable<(string Key, object? Value)> CreateBaseLogProperties(LoggerContext context)
        {
            var baseProperties = new List<(string Key, object? Value)>
            {
                new("CorrelationId", context.CorrelationId),
                new("UserId", context.UserId),
                new("UserName", context.UserName),
                new("OperationName", context.OperationName),
                new("Timestamp", context.Timestamp),
                new("RequestPath", context.RequestPath),
                new("RequestMethod", context.RequestMethod),
                new("UserAgent", context.UserAgent)
            };

            context.AdditionalData?.ToList().ForEach(kvp =>
            {
                baseProperties.Add((kvp.Key, kvp.Value));
            });

            return baseProperties.ToArray();
        }
    }
}