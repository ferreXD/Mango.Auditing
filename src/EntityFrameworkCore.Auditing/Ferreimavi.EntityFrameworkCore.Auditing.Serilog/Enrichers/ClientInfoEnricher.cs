namespace Mango.Auditing.Logging.Enrichers
{
    using Microsoft.AspNetCore.Http;
    using Serilog.Core;
    using Serilog.Events;

    public class ClientInfoEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var context = httpContextAccessor.HttpContext;
            if (context == null) return;

            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var clientInfo = ParseUserAgent(userAgent);

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ClientBrowser", clientInfo.Browser));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ClientOS", clientInfo.OS));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ClientDevice", clientInfo.Device));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ClientIsMobile", clientInfo.IsMobile));
        }

        private static (string Browser, string OS, string Device, bool IsMobile) ParseUserAgent(string userAgent)
        {
            // Simple parsing logic - in production you might want to use a library like UAParser
            var isMobile = userAgent.Contains("Mobile") || userAgent.Contains("Android");
            var browser = DetermineBrowser(userAgent);
            var os = DetermineOS(userAgent);
            var device = DetermineDevice(userAgent);

            return (browser, os, device, isMobile);
        }

        private static string DetermineBrowser(string userAgent) =>
            userAgent.Contains("Chrome") ? "Chrome" :
            userAgent.Contains("Firefox") ? "Firefox" :
            userAgent.Contains("Safari") ? "Safari" :
            userAgent.Contains("Edge") ? "Edge" :
            userAgent.Contains("MSIE") || userAgent.Contains("Trident") ? "Internet Explorer" :
            "Unknown";

        private static string DetermineOS(string userAgent) =>
            userAgent.Contains("Windows") ? "Windows" :
            userAgent.Contains("Mac OS") ? "macOS" :
            userAgent.Contains("Linux") ? "Linux" :
            userAgent.Contains("Android") ? "Android" :
            userAgent.Contains("iOS") || userAgent.Contains("iPhone") || userAgent.Contains("iPad") ? "iOS" :
            "Unknown";

        private static string DetermineDevice(string userAgent) =>
            userAgent.Contains("iPhone") ? "iPhone" :
            userAgent.Contains("iPad") ? "iPad" :
            userAgent.Contains("Android") && userAgent.Contains("Mobile") ? "Android Phone" :
            userAgent.Contains("Android") ? "Android Tablet" :
            "Desktop";
    }
}