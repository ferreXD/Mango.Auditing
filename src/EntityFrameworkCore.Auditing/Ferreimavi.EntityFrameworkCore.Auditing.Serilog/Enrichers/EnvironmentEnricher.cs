// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    using Microsoft.Extensions.Hosting;
    using Serilog.Core;
    using Serilog.Events;

    public class EnvironmentEnricher(IHostEnvironment hostEnvironment) : ILogEventEnricher
    {
        private readonly string _environmentName = hostEnvironment.EnvironmentName;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Environment", _environmentName));
        }
    }
}