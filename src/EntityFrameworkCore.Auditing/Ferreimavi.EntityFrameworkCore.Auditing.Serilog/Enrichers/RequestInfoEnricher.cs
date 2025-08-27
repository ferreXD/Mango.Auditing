// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    using Microsoft.AspNetCore.Http;
    using Serilog.Core;
    using Serilog.Events;

    public class RequestInfoEnricher(IHttpContextAccessor accessor) : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var context = accessor.HttpContext;
            if (context == null) return;

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "RequestHost", context.Request.Host.Value));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "RequestScheme", context.Request.Scheme));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "RequestMethod", context.Request.Method));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "RequestPath", context.Request.Path));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "RequestQueryString", context.Request.QueryString.Value));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "RequestProtocol", context.Request.Protocol));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "RequestContentType", context.Request.ContentType));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "RequestContentLength", context.Request.ContentLength));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "RequestIp", context.Connection.RemoteIpAddress?.ToString()));
        }
    }
}