// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    using Serilog.Core;
    using Serilog.Events;
    using System.Diagnostics;

    public class PerformanceMetricsEnricher : ILogEventEnricher
    {
        private readonly Process _process = Process.GetCurrentProcess();
        private readonly Stopwatch _uptime = Stopwatch.StartNew();

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            _process.Refresh();

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ProcessUptime", _uptime.Elapsed.ToString()));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ProcessCpuTime", _process.TotalProcessorTime.TotalMilliseconds));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ProcessMemoryMB", _process.WorkingSet64 / (1024 * 1024)));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ProcessThreads", _process.Threads.Count));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "GcTotalMemoryMB", GC.GetTotalMemory(false) / (1024 * 1024)));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "GcGen0Collections", GC.CollectionCount(0)));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "GcGen1Collections", GC.CollectionCount(1)));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "GcGen2Collections", GC.CollectionCount(2)));
        }
    }
}