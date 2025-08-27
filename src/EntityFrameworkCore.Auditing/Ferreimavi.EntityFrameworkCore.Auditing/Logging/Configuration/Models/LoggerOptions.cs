// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Logging
{
    public record LoggerOptions
    {
        public bool EnableTraceLinking { get; init; } = false;
        public bool EnableActivityFallback { get; init; } = true;
        public string DefaultCategory { get; init; } = "Telemetry";
    }
}
