namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup.Factories.Mocks.Telemetry
{
    using Mango.Auditing.Telemetry;
    using Moq;
    using System;
    using System.Collections.Generic;

    public static class MetricsMonitorMockFactory
    {
        public static Mock<IMetricsMonitor> Create()
        {
            var mock = new Mock<IMetricsMonitor>();

            // Record — just log internally or no-op
            mock.Setup(m => m.Record(
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<MetricType>(),
                It.IsAny<IDictionary<string, object>>()));

            // Get — return null by default (safe for optional chaining)
            mock.Setup(m => m.Get(
                    It.IsAny<string>(),
                    It.IsAny<MetricType>(),
                    It.IsAny<TimeSpan>()))
                .Returns(() => null);

            // Aggregate — return empty dictionary by default
            mock.Setup(m => m.Aggregate(It.IsAny<TimeSpan>()))
                .Returns(() => new Dictionary<MetricKey, AggregatedMetrics>());

            // Reset — no-op
            mock.Setup(m => m.Reset(It.IsAny<string>(), It.IsAny<MetricType>()));

            return mock;
        }
    }
}
