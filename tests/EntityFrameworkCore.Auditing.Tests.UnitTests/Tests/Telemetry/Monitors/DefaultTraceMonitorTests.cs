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
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class DefaultTraceMonitorTests
    {
        private readonly Mock<IActivityLogger> _activityLogger;
        private readonly Mock<IPerformanceContextProvider> _contextProvider;
        private readonly DefaultTraceMonitor _sut;

        public DefaultTraceMonitorTests()
        {
            _activityLogger = ActivityLoggerMockFactory.Create();
            _contextProvider = PerformanceContextProviderMockFactory.Create();
            _sut = new DefaultTraceMonitor(_activityLogger.Object, _contextProvider.Object);
        }

        [Fact]
        public void BeginOperation_Should_Merge_Context_And_Invoke_StartActivity()
        {
            // Arrange
            var operationName = "op1";
            var kind = ActivityKind.Server;
            var ctxTags = new Dictionary<string, object> { ["ctx"] = 123 };
            _contextProvider.Object.SetTags(ctxTags);

            var callTags = new Dictionary<string, object> { ["foo"] = "bar" };
            var fakeScope = new Mock<IDisposable>().Object;
            _activityLogger
                .Setup(x => x.StartActivity(
                    operationName,
                    kind,
                    It.Is<IDictionary<string, object?>>(d =>
                        d.Count == 2 &&
                        (int?)d["ctx"] == 123 &&
                        (string?)d["foo"] == "bar")))
                .Returns(fakeScope)
                .Verifiable();

            // Act
            var result = _sut.BeginOperation(operationName, kind, callTags);

            // Assert
            result.Should().BeSameAs(fakeScope);
            _activityLogger.Verify();
        }

        [Fact]
        public async Task BeginOperationAsync_Should_Delegate_To_BeginOperation()
        {
            // Arrange
            var operationName = "asyncOp";
            var fakeScope = new Mock<IDisposable>().Object;
            _activityLogger
                .Setup(x => x.StartActivity(operationName, ActivityKind.Internal, It.IsAny<IDictionary<string, object?>>()))
                .Returns(fakeScope);

            // Act
            var result = await _sut.BeginOperationAsync(operationName);

            // Assert
            result.Should().BeSameAs(fakeScope);
        }


        [Fact]
        public void BeginOperation_WithNullTags_UsesJustContext()
        {
            _contextProvider.Object.SetTags(new Dictionary<string, object> { ["a"] = 1 });
            var scope = _sut.BeginOperation("foo");
            _activityLogger.Verify(x => x.StartActivity("foo", ActivityKind.Internal,
                It.Is<IDictionary<string, object?>>(d => d.Count == 1 && (int?)d["a"] == 1)));
        }

        [Fact]
        public void BeginOperation_WithNullTags_UsesNoTags()
        {
            var scope = _sut.BeginOperation("foo");
            _activityLogger.Verify(x => x.StartActivity("foo", ActivityKind.Internal,
                It.Is<IDictionary<string, object?>>(d => d.Count == 0)));
        }

        [Theory]
        [MemberData(nameof(EventAndExceptionData))]
        public void RecordEvent_And_RecordException_Should_Merge_And_Invoke(
            bool isException,
            string messageOrNull,
            Exception exOrNull,
            IDictionary<string, object>? extraTags)
        {
            var ctxTags = new Dictionary<string, object> { ["user"] = "joe" };
            var tagsCount = ctxTags.Count + (extraTags?.Count ?? 0);

            _contextProvider.Object.SetTags(ctxTags);

            if (!isException)
            {
                // RecordEvent path
                _activityLogger
                    .Setup(x => x.RecordEvent(
                        messageOrNull!,
                        It.Is<IDictionary<string, object?>>(d =>
                            d.Count == tagsCount &&
                            (string?)d["user"] == "joe" &&
                            (extraTags == null || d.ContainsKey(extraTags.Keys.First())))))
                    .Verifiable();

                _sut.RecordEvent(messageOrNull!, extraTags);
            }
            else
            {
                _sut.RecordException(exOrNull!, extraTags);
            }

            _activityLogger.Verify();
        }

        [Fact]
        public void SetBaggage_Should_Delegate_Straight()
        {
            var bag = new Dictionary<string, string> { ["k"] = "v" };
            _activityLogger.Setup(x => x.SetBaggage(bag)).Verifiable();

            _sut.SetBaggage(bag);

            _activityLogger.Verify();
        }

        [Fact]
        public void GetCurrentTraceId_And_SpanId_Should_Proxy()
        {
            _activityLogger.Setup(x => x.GetCurrentTraceId()).Returns("TID");
            _activityLogger.Setup(x => x.GetCurrentSpanId()).Returns("SID");

            _sut.GetCurrentTraceId().Should().Be("TID");
            _sut.GetCurrentSpanId().Should().Be("SID");
        }

        public static IEnumerable<object?[]> EventAndExceptionData()
        {
            yield return new object?[] { false, "evt", null, (IDictionary<string, object>?)null };
            yield return new object?[] { false, "evt2", null, new Dictionary<string, object> { ["x"] = 1 } };
            yield return new object?[] { true, null, new InvalidOperationException("boom"), null };
            yield return new object?[] { true, null, new InvalidOperationException("oops"), new Dictionary<string, object> { ["y"] = 2 } };
        }
    }
}