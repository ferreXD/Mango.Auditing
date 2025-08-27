// ReSharper disable once CheckNamespace
namespace EntityFrameworkCore.Auditing.Tests.UnitTests
{
    using EntityFrameworkCore.Auditing.Tests.UnitTests.Setup.ActivitySource.Listeners;
    using Mango.Auditing.Telemetry;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class DefaultActivityLoggerTests
    {
        private readonly ActivitySource _source;
        private readonly DefaultActivityLogger _logger;

        public DefaultActivityLoggerTests()
        {
            // give the source a unique name so it doesn't collide with other tests
            _source = new ActivitySource("TestActivityLogger");
            _logger = new DefaultActivityLogger(_source);

            TestActivitySourceSetup.RegisterAllSampling("TestActivityLogger");
        }

        internal void Dispose()
        {
            _source.Dispose();
        }

        [Fact]
        public void TraceScope_executes_and_returns_value()
        {
            var result = _logger.TraceScope("test-op", () => 123);
            result.Should().Be(123);
            Activity.Current.Should().BeNull();
        }

        [Fact]
        public async Task TraceScopeAsync_executes_and_returns_value()
        {
            var result = await _logger.TraceScopeAsync("test-async-op", () => Task.FromResult("hello"));
            result.Should().Be("hello");
            Activity.Current.Should().BeNull();
        }

        [Fact]
        public void TraceScope_rethrows_and_records_exception()
        {
            Action act = () => _logger.TraceScope("boom-op", () => throw new InvalidOperationException("oops"));
            act.Should().Throw<InvalidOperationException>().WithMessage("oops");
            // after disposal, no current activity
            Activity.Current.Should().BeNull();
        }

        [Fact]
        public void StartActivity_creates_and_disposes_scope()
        {
            Activity? inside = null;
            IDisposable scope;
            scope = _logger.StartActivity("my-op", ActivityKind.Server);
            inside = Activity.Current;
            inside.Should().NotBeNull();
            inside.DisplayName.Should().Be("my-op");
            inside.Kind.Should().Be(ActivityKind.Server);

            scope.Dispose();
            Activity.Current.Should().BeNull();
        }

        [Fact]
        public void SetBaggage_attaches_baggage_to_current_activity()
        {
            using var scope = _logger.StartActivity("bag-op");
            _logger.SetBaggage(new Dictionary<string, string> { { "k", "v" } });
            var baggage = Activity.Current!.Baggage.ToList();
            baggage.Should().ContainSingle()
                .Which.Should().Be(new KeyValuePair<string, string>("k", "v"));
        }

        [Fact]
        public void RecordEvent_adds_event_and_tags()
        {
            using var scope = _logger.StartActivity("evt-op");
            _logger.RecordEvent("myevent", new Dictionary<string, object?> { { "x", 42 } });
            var act = Activity.Current!;
            act.Events.Should().ContainSingle(e => e.Name == "myevent");
            act.TagObjects
                .Should()
                .Contain(kv => kv.Key == "x" && (int)kv.Value! == 42);
        }

        [Fact]
        public void RecordEvent_noop_if_no_current_activity()
        {
            // no exception
            _logger.RecordEvent("no-act");
            Activity.Current.Should().BeNull();
        }

        [Fact]
        public void RecordException_throws_on_null()
        {
            Assert.Throws<ArgumentNullException>(() => _logger.RecordException(null!));
        }

        [Fact]
        public void RecordException_adds_exception_event_tags_and_status()
        {
            using var scope = _logger.StartActivity("ex-op");
            var ex = new InvalidOperationException("bad");
            _logger.RecordException(ex, new Dictionary<string, object?> { { "foo", "bar" } });
            var act = Activity.Current!;

            // it should have an "exception" event
            act.Events.Should().Contain(e => e.Name == "exception");

            // tags must include exception.type/message/stacktrace and foo
            act.Tags.Should().Contain(kv => kv.Key == "exception.type" && kv.Value!.ToString().EndsWith("InvalidOperationException"));
            act.Tags.Should().Contain(kv => kv.Key == "exception.message" && kv.Value as string == "bad");
            act.Tags.Should().Contain(kv => kv.Key == "foo" && kv.Value as string == "bar");

            // status must be error
            act.Status.Should().Be(ActivityStatusCode.Error);
        }

        [Fact]
        public void GetCurrentActivity_trace_and_span_id_are_available()
        {
            using var scope = _logger.StartActivity("ids-op");
            _logger.GetCurrentActivity().Should().BeSameAs(Activity.Current);
            _logger.GetCurrentTraceId().Should().Be(Activity.Current?.TraceId.ToString());
            _logger.GetCurrentSpanId().Should().Be(Activity.Current?.SpanId.ToString());
        }

        [Fact]
        public void LogCurrentContext_writes_expected_console_output()
        {
            using var scope = _logger.StartActivity("ctx-op");
            // add a tag and baggage so they appear in the output
            _logger.RecordEvent("e1", new Dictionary<string, object?> { { "t", "v" } });
            _logger.SetBaggage(new Dictionary<string, string> { { "b", "w" } });

            var sw = new StringWriter();
            var orig = Console.Out;
            Console.SetOut(sw);

            _logger.LogCurrentContext();

            Console.SetOut(orig);

            var outStr = sw.ToString();
            outStr.Should().Contain("[Activity: ctx-op]");
            outStr.Should().Contain("TraceId=");
            outStr.Should().Contain("SpanId=");
            outStr.Should().Contain("Tags: t=v");
            outStr.Should().Contain("Baggage: b=w");
        }
    }
}
