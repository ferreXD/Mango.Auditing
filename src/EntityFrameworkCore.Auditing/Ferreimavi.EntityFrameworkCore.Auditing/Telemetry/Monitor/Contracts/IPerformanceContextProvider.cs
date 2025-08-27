// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    using System.Collections.Generic;

    public interface IPerformanceContextProvider
    {
        /// <summary>
        /// Gets the current set of contextual tags for the current execution scope.
        /// </summary>
        IReadOnlyDictionary<string, object> GetTags();

        /// <summary>
        /// Adds or overrides contextual tags in the current execution scope.
        /// </summary>
        void SetTags(IDictionary<string, object> tags);

        /// <summary>
        /// Appends or replaces a single tag.
        /// </summary>
        void SetTag(string key, object value);

        /// <summary>
        /// Clears all contextual tags.
        /// </summary>
        void Clear();
    }

}
