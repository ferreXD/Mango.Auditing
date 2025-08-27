// ReSharper disable once CheckNamespace
namespace EntityFrameworkCore.Auditing.Tests.UnitTests
{
    using Mango.Auditing.Telemetry;
    using FluentAssertions;
    using Moq;
    using Setup.Factories.Mocks.Telemetry;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class DefaultPerformanceMonitorTests
    {
        private readonly Mock<ITraceMonitor> _traceMock;
        private readonly Mock<IMetricsMonitor> _metricsMock;
        private readonly Mock<ILogMonitor> _logsMock;
        private readonly Mock<IPerformanceContextProvider> _ctxMock;
        private readonly PerformanceMonitor _sut;

        public DefaultPerformanceMonitorTests()
        {
            _traceMock = TraceMonitorMockFactory.Create();
            _metricsMock = MetricsMonitorMockFactory.Create();
            _logsMock = LogMonitorMockFactory.Create();
            _ctxMock = PerformanceContextProviderMockFactory.Create();

            _sut = new PerformanceMonitor(
                _traceMock.Object,
                _metricsMock.Object,
                _logsMock.Object,
                _ctxMock.Object
            );
        }

        #region Tracing

        [Fact]
        public void BeginOperation_MergesTags_And_CallsTrace()
        {
            // Arrange
            var baseTags = new Dictionary<string, object> { ["a"] = 1 };
            _ctxMock.Object.SetTags(baseTags);

            var extra = new Dictionary<string, object> { ["b"] = "two" };
            var kind = ActivityKind.Client;

            var dummyScope = new Mock<IDisposable>().Object;
            _traceMock.Setup(t => t.BeginOperation(
                    "op",
                    kind,
                    It.Is<IDictionary<string, object>>(d =>
                        d.Count == 2 &&
                        (int)d["a"] == 1 &&
                        (string)d["b"] == "two"
                    )
                ))
                .Returns(dummyScope)
                .Verifiable();

            // Act
            var result = _sut.BeginOperation("op", kind, extra);

            // Assert
            result.Should().BeSameAs(dummyScope);
            _traceMock.Verify();
        }

        [Fact]
        public async Task BeginOperationAsync_DelegatesToTraceAsync()
        {
            // Arrange
            var dummyScope = new Mock<IDisposable>().Object;
            _traceMock
                .Setup(t => t.BeginOperationAsync("asyncOp", ActivityKind.Internal, It.IsAny<IDictionary<string, object>>(), CancellationToken.None))
                .ReturnsAsync(dummyScope)
                .Verifiable();

            // Act
            var result = await _sut.BeginOperationAsync("asyncOp");

            // Assert
            result.Should().BeSameAs(dummyScope);
            _traceMock.Verify();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RecordEvent_And_RecordException_MergeTags_And_CallTrace(bool isException)
        {
            // Arrange
            var baseTags = new Dictionary<string, object> { ["u"] = "joe" };
            _ctxMock.Object.SetTags(baseTags);

            var extra = new Dictionary<string, object> { ["z"] = 9 };

            if (!isException)
            {
                _traceMock.Setup(t => t.RecordEvent(
                        "hello",
                        It.Is<IDictionary<string, object>>(d =>
                            d.Count == 2 &&
                            (string)d["u"] == "joe" &&
                            (int)d["z"] == 9
                        )
                    ))
                    .Verifiable();

                // Act
                _sut.RecordEvent("hello", extra);
            }
            else
            {
                var ex = new InvalidOperationException("boom");
                _traceMock.Setup(t => t.RecordException(
                        ex,
                        It.Is<IDictionary<string, object>>(d =>
                            d.Count == 2 &&
                            (string)d["u"] == "joe" &&
                            (int)d["z"] == 9
                        )
                    ))
                    .Verifiable();

                // Act
                _sut.RecordException(ex, extra);
            }

            // Assert
            _traceMock.Verify();
        }

        [Fact]
        public void SetBaggage_Delegates_ToTrace()
        {
            var bag = new Dictionary<string, string> { ["k"] = "v" };
            _traceMock.Setup(t => t.SetBaggage(bag)).Verifiable();

            _sut.SetBaggage(bag);

            _traceMock.Verify();
        }

        [Fact]
        public void GetCurrentTraceAndSpanId_ProxyToTrace()
        {
            _traceMock.Setup(t => t.GetCurrentTraceId()).Returns("TX");
            _traceMock.Setup(t => t.GetCurrentSpanId()).Returns("SX");

            _sut.GetCurrentTraceId().Should().Be("TX");
            _sut.GetCurrentSpanId().Should().Be("SX");
        }

        #endregion

        #region Metrics

        [Fact]
        public void RecordMetric_MergesTags_And_CallsMetrics()
        {
            // Arrange
            var baseTags = new Dictionary<string, object> { ["k1"] = 100 };
            _ctxMock.Object.SetTags(baseTags);

            var extra = new Dictionary<string, object> { ["k2"] = "v2" };

            _metricsMock.Setup(m => m.Record(
                    "m1", 3.14, MetricType.Counter,
                    It.Is<IDictionary<string, object>>(d =>
                        d.Count == 2 &&
                        (int)d["k1"] == 100 &&
                        (string)d["k2"] == "v2"
                    )
                ))
                .Verifiable();

            // Act
            _sut.RecordMetric("m1", 3.14, MetricType.Counter, extra);

            // Assert
            _metricsMock.Verify();
        }

        [Fact]
        public void GetAggregatedMetrics_DelegatesToMetrics()
        {
            var window = TimeSpan.FromMinutes(1);
            var outDict = new Dictionary<MetricKey, AggregatedMetrics>();
            _metricsMock.Setup(m => m.Aggregate(window)).Returns(outDict).Verifiable();

            _sut.GetAggregatedMetrics(window).Should().BeSameAs(outDict);
            _metricsMock.Verify();
        }

        [Fact]
        public void GetMetrics_DelegatesToMetrics()
        {
            var window = TimeSpan.FromMinutes(2);
            var agg = new AggregatedMetrics { Name = "x" };
            _metricsMock.Setup(m => m.Get("x", MetricType.Histogram, window)).Returns(agg).Verifiable();

            _sut.GetMetrics("x", MetricType.Histogram, window).Should().BeSameAs(agg);
            _metricsMock.Verify();
        }

        [Theory]
        [InlineData(null, MetricType.Counter)]
        [InlineData("abc", MetricType.Histogram)]
        public void ResetMetrics_ClearsProperly(string? name, MetricType type)
        {
            if (string.IsNullOrEmpty(name))
            {
                _metricsMock.Setup(m => m.Reset(null, type)).Verifiable();
                _sut.ResetMetrics(null, type);
            }
            else
            {
                _metricsMock.Setup(m => m.Reset(name, type)).Verifiable();
                _sut.ResetMetrics(name, type);
            }

            _metricsMock.Verify();
        }

        #endregion

        #region Logging

        [Theory]
        [InlineData("vmsg")]
        [InlineData("another")]
        public void Verbose_Delegates_ToLogMonitor(string msg)
        {
            _logsMock.Setup(l => l.Verbose(msg, It.IsAny<(string, object?)[]>())).Verifiable();
            _sut.Verbose(msg, ("p", 1));
            _logsMock.Verify();
        }

        [Fact]
        public void Info_Wraps_LogMonitor()
        {
            _logsMock.Setup(l => l.Info("i", It.IsAny<(string, object?)[]>())).Verifiable();
            _sut.Info("i", ("a", 2));
            _logsMock.Verify();
        }

        [Fact]
        public void Warn_Wraps_LogMonitor()
        {
            _logsMock.Setup(l => l.Warn("w", It.IsAny<(string, object?)[]>())).Verifiable();
            _sut.Warn("w", ("a", 3));
            _logsMock.Verify();
        }

        [Fact]
        public void Debug_Wraps_LogMonitor()
        {
            _logsMock.Setup(l => l.Debug("d", It.IsAny<(string, object?)[]>())).Verifiable();
            _sut.Debug("d", ("a", 4));
            _logsMock.Verify();
        }

        [Fact]
        public void Error_Wraps_LogMonitorError()
        {
            var ex = new Exception("err");
            _logsMock.Setup(l => l.Error("bad", ex, It.IsAny<(string, object?)[]>())).Verifiable();
            _sut.Error("bad", ex, ("a", 5));
            _logsMock.Verify();
        }

        #endregion
    }
}