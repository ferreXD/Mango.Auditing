namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup.ActivitySource.Listeners
{
    using System.Diagnostics;

    public static class TestActivitySourceSetup
    {
        public static void RegisterAllSampling(string sourceName)
        {
            var listener = new ActivityListener
            {
                // Listen only to your named source (or return true for all sources)
                ShouldListenTo = src => src.Name == sourceName,
                // We want all data (you could pick SamplingResult.AllData if you care only about tags/events)
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                SampleUsingParentId = (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStopped = activity => { }
            };

            ActivitySource.AddActivityListener(listener);
        }
    }
}
