// ReSharper disable once CheckNamespace
namespace EntityFrameworkCore.Auditing.Tests.UnitTests
{
    using Mango.Auditing.Telemetry;
    using FluentAssertions;
    using Setup.ActivitySource.Listeners;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class ActivityExtensionsTests : IDisposable
    {
        private readonly ActivitySource _source;

        public ActivityExtensionsTests()
        {
            _source = new ActivitySource("TestSource");

            // Listen to our TestSource and sample all data
            TestActivitySourceSetup.RegisterAllSampling("TestSource");
        }

        public void Dispose()
        {
            _source.Dispose();
        }

        [Fact]
        public void StartSafeActivity_Should_Create_Activity_With_Correct_Name_Kind_And_Tags()
        {
            // Arrange
            var tags = new[] { new KeyValuePair<string, object?>("x", 42) };

            // Act
            using var activity = _source.StartSafeActivity("my-op", ActivityKind.Server, tags);

            // Assert
            activity.Should().NotBeNull();
            activity!.DisplayName.Should().Be("my-op");
            activity.Kind.Should().Be(ActivityKind.Server);
            activity.TagObjects.Should().Contain(kv => kv.Key == "x" && (int)kv.Value! == 42);
        }

        [Fact]
        public void StartSafeScope_Should_Start_And_Stop_Activity_And_Set_Duration_Tag_On_Dispose()
        {
            // Act
            Activity? captured = null;
            using (_source.StartSafeScope("scope-op"))
            {
                captured = Activity.Current;
                captured.Should().NotBeNull();
                captured!.DisplayName.Should().Be("scope-op");
                // duration_ms not set until Dispose
                captured.TagObjects.Should().NotContain(kv => kv.Key == "duration_ms");
            }

            // After disposing scope
            captured.Should().NotBeNull();
            captured!.TagObjects.Should().Contain(kv => kv.Key == "duration_ms");
        }

        [Fact]
        public void AppendTag_Should_Set_Single_Tag()
        {
            using var activity = new Activity("t").Start();
            activity.AppendTag("foo", "bar");
            activity.TagObjects.Should().Contain(kv => kv.Key == "foo" && (string)kv.Value! == "bar");
        }

        [Fact]
        public void AddTags_Should_Set_Multiple_Tags()
        {
            using var activity = new Activity("t2").Start();
            var tags = new Dictionary<string, object?>
            {
                ["a"] = 1,
                ["b"] = "xyz"
            };
            activity.AddTags(tags);
            activity.TagObjects.Should().Contain(kv => kv.Key == "a" && (int)kv.Value! == 1);
            activity.TagObjects.Should().Contain(kv => kv.Key == "b" && (string)kv.Value! == "xyz");
        }

        [Fact]
        public void SetBaggage_Should_Add_Baggage_Items()
        {
            using var activity = new Activity("t3").Start();
            var bag = new Dictionary<string, string> { ["u"] = "v" };
            activity.SetBaggage(bag);
            activity.Baggage.Should().Contain(kv => kv.Key == "u" && kv.Value == "v");
        }

        [Fact]
        public void AppendTag_With_Invalid_Key_Should_Do_Nothing()
        {
            using var activity = new Activity("t4").Start();
            activity.AppendTag("", 1);
            activity.AppendTag(null!, 2);
            activity.TagObjects.Should().BeEmpty();
        }

        [Fact]
        public void AddTags_With_Null_Collection_Should_Do_Nothing()
        {
            using var activity = new Activity("t5").Start();
            activity.AddTags(null);
            activity.TagObjects.Should().BeEmpty();
        }
    }
}
