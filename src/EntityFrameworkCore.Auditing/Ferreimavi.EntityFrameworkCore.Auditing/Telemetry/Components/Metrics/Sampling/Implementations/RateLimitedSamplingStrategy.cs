// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System;
    using System.Collections.Generic;

    public class RateLimitedSamplingStrategy(TelemetryOptions options, int maxPerSecond) : BaseSampleStrategy(options), IMetricSamplingStrategy
    {
        private int _count;
        private DateTime _windowStart = DateTime.UtcNow;

        public override bool ShouldSample(string _, double __, MetricType ___, IDictionary<string, object>? ____)
        {
            var now = DateTime.UtcNow;
            if ((now - _windowStart).TotalSeconds >= 1)
            {
                _windowStart = now;
                _count = 0;
            }

            if (_count >= maxPerSecond) return false;

            _count++;
            return true;
        }
    }
}
