// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Telemetry
{
    using System.Diagnostics;

    /// <summary>
    /// Provides functionality for tracing and distributed logging.
    /// </summary>
    public interface IActivityLogger
    {
        /// <summary>
        /// Executes a function within a traced scope, automatically managing the activity lifecycle.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="name"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        TResult TraceScope<TResult>(string name, Func<TResult> func);

        /// <summary>
        /// Executes an asynchronous function within a traced scope, automatically managing the activity lifecycle.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="name"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        Task<TResult> TraceScopeAsync<TResult>(string name, Func<Task<TResult>> func);


        /// <summary>
        /// Executes an action within a traced scope, automatically managing the activity lifecycle.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="name"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        void TraceScope(string name, Action func);

        /// <summary>
        /// Executes an asynchronous action within a traced scope, automatically managing the activity lifecycle.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="name"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        Task TraceScopeAsync(string name, Func<Task> func);

        /// <summary>
        /// Starts an activity for the specified operation.
        /// </summary>
        /// <param name="operationName">The name of the operation.</param>
        /// <param name="kind">The kind of activity (defaults to Internal).</param>
        /// <param name="tags">Optional tags to enrich the activity.</param>
        /// <returns>An IDisposable that stops the activity when disposed.</returns>
        IDisposable StartActivity(string operationName, ActivityKind kind = ActivityKind.Internal, IDictionary<string, object?>? tags = null);

        /// <summary>
        /// Adds a baggage item to the current activity.
        /// </summary>
        /// <param name="key">The baggage key.</param>
        /// <param name="value">The baggage value.</param>
        void SetBaggage(IDictionary<string, string> items);

        /// <summary>
        /// Records a custom event to the current activity.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="tagserties">Optional properties associated with the event.</param>
        void RecordEvent(string eventName, IDictionary<string, object?>? tags = null);

        /// <summary>
        /// Records an exception as a special event on the current activity.
        /// </summary>
        /// <param name="exception">The exception to record.</param>
        /// <param name="tags">Optional properties to enrich the event.</param>
        void RecordException(Exception exception, IDictionary<string, object?>? tags = null);

        /// <summary>
        /// Retrieves the current active activity.
        /// </summary>
        /// <returns>The current <see cref="Activity" /> or null if none is active.</returns>
        Activity? GetCurrentActivity();

        /// <summary>
        /// Retrieves the current trace ID from the active activity, if available.
        /// </summary>
        /// <returns>The trace ID or null if no activity is active.</returns>
        string? GetCurrentTraceId();

        /// <summary>
        /// Retrieves the current span ID from the active activity, if available.
        /// </summary>
        /// <returns>The span ID or null if no activity is active.</returns>
        string? GetCurrentSpanId();

        /// <summary>
        /// Logs the current activity context, including its name, ID, and any associated tags.
        /// </summary>
        void LogCurrentContext();
    }
}