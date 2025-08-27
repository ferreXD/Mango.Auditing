// ReSharper disable once CheckNamespace
namespace EntityFrameworkCore.Auditing.Tests.UnitTests
{
    using Mango.Auditing.Telemetry;
    using FluentAssertions;
    using Moq;
    using Setup;
    using Setup.Factories.Mocks.Telemetry;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DefaultMetricsMonitorTests
    {
        private readonly MetricProviderMockFactory.MetricProviderMock _metricProviderMock;
        private readonly Mock<IRootMetricProvider> _rootProviderMock;
        private readonly Mock<IPerformanceContextProvider> _contextProviderMock;
        private readonly DefaultMetricsMonitor _sut;

        public DefaultMetricsMonitorTests()
        {
            // 1) Create the generic IMetricProvider mock + capture list
            _metricProviderMock = MetricProviderMockFactory.Create();
            // 2) Treat it as IRootMetricProvider
            _rootProviderMock = _metricProviderMock.Mock.As<IRootMetricProvider>();
            // 3) Create a real context‐provider backing store
            _contextProviderMock = PerformanceContextProviderMockFactory.Create();

            _sut = new DefaultMetricsMonitor(
                _rootProviderMock.Object,
                _contextProviderMock.Object
            );
        }

        [Fact]
        public void Record_WithOnlyContextTags_Calls_RecordMeasurement_WithMergedTags()
        {
            // arrange
            var ctx = new Dictionary<string, object> { ["user"] = "alice" };
            _contextProviderMock.Object.SetTags(ctx);

            // act
            _sut.Record("mymetric", 42);

            // assert
            // the factory captured the actual calls in RecordedMeasurements
            var rec = _metricProviderMock.RecordedMeasurements.Single();
            rec.MetricName.Should().Be("mymetric");
            rec.Value.Should().Be(42);
            rec.MetricType.Should().Be(MetricType.Histogram);
            rec.Tags.Should().ContainKey("user").WhoseValue.Should().Be("alice");
        }

        [Fact]
        public void Record_WithExtraTags_Calls_RecordMeasurement_WithMergedTags()
        {
            // arrange
            var ctx = new Dictionary<string, object> { ["env"] = "test" };
            var extra = new Dictionary<string, object> { ["feature"] = "flag" };
            _contextProviderMock.Object.SetTags(ctx);

            // act
            _sut.Record("x", 1.23, MetricType.Counter, extra);

            // assert
            var rec = _metricProviderMock.RecordedMeasurements.Single();
            rec.MetricName.Should().Be("x");
            rec.Value.Should().Be(1.23);
            rec.MetricType.Should().Be(MetricType.Counter);
            rec.Tags.Should().HaveCount(2)
                       .And.Contain(new KeyValuePair<string, object>("env", "test"))
                       .And.Contain(new KeyValuePair<string, object>("feature", "flag"));
        }

        [Fact]
        public void Get_Delegates_To_GetMetric_And_ReturnsResult()
        {
            // arrange
            var expected = new AggregatedMetrics { Name = "m", Count = 5 };
            _rootProviderMock
                .Setup(x => x.GetMetric("m", MetricType.Histogram, TimeSpan.FromSeconds(1)))
                .Returns(expected);

            // act
            var actual = _sut.Get("m", MetricType.Histogram, TimeSpan.FromSeconds(1));

            // assert
            actual.Should().BeSameAs(expected);
        }

        [Fact]
        public void Aggregate_Delegates_To_GetAggregatedMetrics()
        {
            // arrange
            var dummy = new Dictionary<MetricKey, AggregatedMetrics>
            {
                [new MetricKey("a", MetricType.Counter)] = new AggregatedMetrics { Name = "a", Count = 1 }
            };
            _rootProviderMock
                .Setup(x => x.GetAggregatedMetrics(TimeSpan.FromMinutes(5)))
                .Returns(dummy);

            // act
            var actual = _sut.Aggregate(TimeSpan.FromMinutes(5));

            // assert
            actual.Should().BeSameAs(dummy);
        }

        [Fact]
        public void Reset_WithNullName_Calls_Clear_All()
        {
            // act
            _sut.Reset(null);

            // assert
            _rootProviderMock.Verify(x => x.Clear(), Times.Once);
            _rootProviderMock.Verify(x => x.Clear(It.IsAny<string>(), It.IsAny<MetricType>()), Times.Never);
        }

        [Fact]
        public void Reset_WithName_Calls_Clear_WithNameAndDefaultType()
        {
            // act
            _sut.Reset("abc", MetricType.UpDownCounter);

            // assert
            _rootProviderMock.Verify(x => x.Clear("abc", MetricType.UpDownCounter), Times.Once);
            _rootProviderMock.Verify(x => x.Clear(), Times.Never);
        }
    }
}
