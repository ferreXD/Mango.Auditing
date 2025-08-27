namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup.Factories.Mocks.Telemetry
{
    using Mango.Auditing.Telemetry;
    using Moq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public static class PerformanceContextProviderMockFactory
    {
        /// <summary>
        /// Creates a Mock&lt;IPerformanceContextProvider&gt; that keeps
        /// an in-memory dictionary of tags and correctly responds to GetTags/SetTag/etc.
        /// </summary>
        public static Mock<IPerformanceContextProvider> Create()
        {
            var mock = new Mock<IPerformanceContextProvider>();

            // Backing store for tags
            var backingTags = new Dictionary<string, object>();

            mock.Setup(m => m.GetTags())
                .Returns(() => new ReadOnlyDictionary<string, object>(backingTags));

            mock.Setup(m => m.SetTags(It.IsAny<IDictionary<string, object>>()))
                .Callback<IDictionary<string, object>>(tags =>
                {
                    backingTags.Clear();
                    foreach (var kv in tags)
                        backingTags[kv.Key] = kv.Value;
                });

            mock.Setup(m => m.SetTag(It.IsAny<string>(), It.IsAny<object>()))
                .Callback<string, object>((key, value) =>
                {
                    backingTags[key] = value;
                });

            mock.Setup(m => m.Clear())
                .Callback(() => backingTags.Clear());

            return mock;
        }
    }
}
