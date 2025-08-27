namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup.Factories.Mocks.Telemetry
{
    using Mango.Auditing.Telemetry;
    using Moq;
    using System;

    public static class LogMonitorMockFactory
    {
        /// <summary>
        /// Creates a Mock&lt;ILogMonitor&gt; with all methods no-op by default.
        /// </summary>
        public static Mock<ILogMonitor> Create()
        {
            var mock = new Mock<ILogMonitor>();

            // Verbose / Debug / Info / Warn — no-op
            mock.Setup(m => m.Verbose(It.IsAny<string>(), It.IsAny<(string Key, object? Value)[]>()));
            mock.Setup(m => m.Debug(It.IsAny<string>(), It.IsAny<(string Key, object? Value)[]>()));
            mock.Setup(m => m.Info(It.IsAny<string>(), It.IsAny<(string Key, object? Value)[]>()));
            mock.Setup(m => m.Warn(It.IsAny<string>(), It.IsAny<(string Key, object? Value)[]>()));

            // Error — no-op
            mock.Setup(m => m.Error(
                It.IsAny<string>(),
                It.IsAny<Exception>(),
                It.IsAny<(string Key, object? Value)[]>()));

            return mock;
        }
    }
}
