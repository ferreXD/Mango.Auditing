namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup.Factories.Scenarios
{
    using Mango.Auditing;
    using Mango.Auditing.Telemetry;
    using Microsoft.AspNetCore.Http;
    using Mocks.Http;
    using Mocks.Telemetry;
    using Moq;
    using System.Security.Claims;

    /// <summary>
    /// Bundles all relevant mocks so tests can easily verify behaviors.
    /// </summary>
    public class TelemetryMockScenario(
        Mock<IActivityLogger> activityLoggerMock,
        MetricProviderMockFactory.MetricProviderMock metricProviderMock,
        TelemetryLoggerMockFactory.TelemetryLoggerMock telemetryLoggerMock,
        Mock<IPerformanceContextProvider> contextProviderMock)
    {
        public Mock<IActivityLogger> ActivityLoggerMock { get; set; } = activityLoggerMock;
        public MetricProviderMockFactory.MetricProviderMock MetricProviderMock { get; set; } = metricProviderMock;
        public TelemetryLoggerMockFactory.TelemetryLoggerMock TelemetryLoggerMock { get; set; } = telemetryLoggerMock;
        public Mock<IPerformanceContextProvider> PerformanceContextProviderMock { get; set; } = contextProviderMock;
    }

    public class TelemetryMonitorsMockScenario(

        Mock<ITraceMonitor> traceMonitorMock,
        Mock<IMetricsMonitor> metricsMonitorMock,
        Mock<ILogMonitor> logMonitorMock,
        Mock<IPerformanceContextProvider> contextProviderMock)
    {
        public Mock<ITraceMonitor> TraceMonitorMock { get; set; } = traceMonitorMock;
        public Mock<IMetricsMonitor> MetricsMonitorMock { get; set; } = metricsMonitorMock;
        public Mock<ILogMonitor> LogMonitorMock { get; set; } = logMonitorMock;
        public Mock<IPerformanceContextProvider> PerformanceContextProviderMock { get; set; } = contextProviderMock;
    }

    /// <summary>
    /// Factory for creating mocks scenarios
    /// </summary>
    internal static class MockScenarioFactory
    {
        internal static TelemetryMockScenario CreateTelemetryScenario()
        {
            var activityLoggerMock = ActivityLoggerMockFactory.Create();
            var telemetryLoggerMock = TelemetryLoggerMockFactory.Create();
            var metricProviderMock = MetricProviderMockFactory.Create();
            var contextProviderMock = PerformanceContextProviderMockFactory.Create();

            return new TelemetryMockScenario(activityLoggerMock, metricProviderMock, telemetryLoggerMock, contextProviderMock);
        }

        internal static TelemetryMonitorsMockScenario CreateTelemetryMonitorsScenario()
        {
            var traceMonitorMock = TraceMonitorMockFactory.Create();
            var metricsMonitorMock = MetricsMonitorMockFactory.Create();
            var logMonitorMock = LogMonitorMockFactory.Create();
            var contextProviderMock = PerformanceContextProviderMockFactory.Create();

            return new TelemetryMonitorsMockScenario(traceMonitorMock, metricsMonitorMock, logMonitorMock, contextProviderMock);
        }
    }

    /// <summary>
    /// Bundles a mock IHttpContextAccessor with its underlying DefaultHttpContext for easy test setup.
    /// </summary>
    public class HttpContextMockScenario
    {
        internal HttpContextMockScenario(
            DefaultHttpContext httpContext,
            HttpContextAccessor accessor)
        {
            HttpContext = httpContext;
            Accessor = accessor;
        }

        public DefaultHttpContext HttpContext { get; }
        public HttpContextAccessor Accessor { get; }
    }

    /// <summary>
    /// Factory for creating pre‐configured HttpContextMockScenarios.
    /// </summary>
    public static class HttpContextAccessorMockScenarioFactory
    {
        /// <summary>
        /// Create a scenario with an empty HttpContext you can further customize.
        /// </summary>
        public static HttpContextMockScenario Create() => Create(ctx =>
        {
            // seed your context once, here:
            ctx.Request.Method = "POST";
            ctx.Request.Path = "/api/things";
            ctx.Request.Headers["X-Correlation-ID"] = "abc-123";

            // simulate an authenticated user:
            var idClaim = new Claim(ClaimTypes.NameIdentifier, "user-xyz");
            ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { idClaim }, "TestAuth"));
        });

        /// <summary>
        /// Create a scenario and immediately configure the DefaultHttpContext before wiring up the mock.
        /// </summary>
        /// <param name="configure">Action that receives the DefaultHttpContext for headers, user, items, etc.</param>
        public static HttpContextMockScenario Create(Action<DefaultHttpContext> configure)
        {
            // 1) Create the “real” HttpContext you want to manipulate in your test:
            var httpContext = new DefaultHttpContext();
            configure(httpContext);

            var accessor = new HttpContextAccessor
            {
                HttpContext = httpContext
            };

            return new HttpContextMockScenario(httpContext, accessor);
        }
    }

    /// <summary>
    /// Bundles a mock IHttpContextAccessor with its underlying DefaultHttpContext for easy test setup.
    /// </summary>
    public class DefaultHttpUserProviderMockScenario : HttpContextMockScenario
    {
        internal DefaultHttpUserProviderMockScenario(
            DefaultHttpContext httpContext,
            HttpContextAccessor accessor,
            Mock<ICurrentUserProvider> currentUserProviderMock) : base(httpContext, accessor)
        {
            CurrentUserProviderMock = currentUserProviderMock;
        }

        public Mock<ICurrentUserProvider> CurrentUserProviderMock { get; set; }
    }

    /// <summary>
    /// Factory for creating pre‐configured HttpContextMockScenarios.
    /// </summary>
    public static class DefaultHttpUserProviderMockScenarioFactory
    {
        /// <summary>
        /// Create a scenario with an empty HttpContext you can further customize.
        /// </summary>
        public static DefaultHttpUserProviderMockScenario Create() => Create(ctx =>
        {
            // seed your context once, here:
            ctx.Request.Method = "POST";
            ctx.Request.Path = "/api/things";
            ctx.Request.Headers["X-Correlation-ID"] = "abc-123";

            // simulate an authenticated user:
            var idClaim = new Claim(ClaimTypes.NameIdentifier, "user-xyz");
            ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { idClaim }, "TestAuth"));
        });

        /// <summary>
        /// Create a scenario and immediately configure the DefaultHttpContext before wiring up the mock.
        /// </summary>
        /// <param name="configure">Action that receives the DefaultHttpContext for headers, user, items, etc.</param>
        public static DefaultHttpUserProviderMockScenario Create(Action<DefaultHttpContext> configure)
        {
            // 1) Create the “real” HttpContext you want to manipulate in your test:
            var httpContext = new DefaultHttpContext();
            configure(httpContext);

            var accessor = new HttpContextAccessor
            {
                HttpContext = httpContext
            };

            // 2) Create the mock for ICurrentUserProvider:
            var currentUserProviderMock = CurrentUserProviderMockFactory.Create("user-xyz", "user-xyz");

            return new DefaultHttpUserProviderMockScenario(httpContext, accessor, currentUserProviderMock);
        }
    }
}