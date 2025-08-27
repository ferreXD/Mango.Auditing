// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    public static class TimeSpanExtensions
    {
        public static string ToCronExpression(this TimeSpan interval)
        {
            if (interval.TotalMinutes is >= 1 and < 60) return $"*/{interval.TotalMinutes} * * * *";

            if (interval.TotalHours is >= 1 and < 24) return $"0 */{interval.TotalHours} * * *";

            if (interval.TotalDays >= 1) return $"0 0 */{interval.TotalDays} * *";

            throw new ArgumentException("Interval must be at least 1 minute", nameof(interval));
        }
    }
}