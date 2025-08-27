// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Logging
{
    using Serilog.Core;
    using Serilog.Events;
    using System.Collections;
    using System.Data.Common;

    public class ExceptionContextEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Exception == null) return;

            var exception = logEvent.Exception;
            var exceptionProperties = new Dictionary<string, object>
            {
                ["ExceptionType"] = exception.GetType().FullName!,
                ["ExceptionMessage"] = exception.Message,
                ["ExceptionStackTrace"] = exception.StackTrace!,
                ["ExceptionSource"] = exception.Source!,
                ["ExceptionHResult"] = exception.HResult
            };

            // Add data dictionary items
            if (exception.Data.Count > 0)
            {
                var dataDict = new Dictionary<string, object>();
                foreach (DictionaryEntry entry in exception.Data)
                    if (entry.Key is string key)
                        dataDict[key] = entry.Value?.ToString() ?? "null";
                exceptionProperties["ExceptionData"] = dataDict;
            }

            // Add inner exception details
            if (exception.InnerException != null)
            {
                exceptionProperties["InnerExceptionType"] = exception.InnerException.GetType().FullName!;
                exceptionProperties["InnerExceptionMessage"] = exception.InnerException.Message;
            }

            // Handle specific exception types
            if (exception is DbException dbException)
                exceptionProperties["DbErrorCode"] = dbException.ErrorCode;
            else if (exception is HttpRequestException httpException) exceptionProperties["HttpStatusCode"] = httpException.StatusCode!;

            var exceptionDetail = new StructureValue(exceptionProperties.Select(kvp =>
                new LogEventProperty(kvp.Key, new ScalarValue(kvp.Value))));

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ExceptionDetail", exceptionDetail));
        }
    }
}