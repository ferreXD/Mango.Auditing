// ReSharper disable once CheckNamespace
namespace Mango.Auditing
{
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using System.Data.Common;

    // TODO: Add feature flag to enable or disable this interceptor.
    public class EfObservabilityInterceptor(IOperationInstrumentator instrumentator) : DbCommandInterceptor
    {
        public override InterceptionResult<DbDataReader> ReaderExecuting(
           DbCommand command,
           CommandEventData eventData,
           InterceptionResult<DbDataReader> result)
           => Execute(command, eventData, "EFCore.Reader", () => base.ReaderExecuting(command, eventData, result));

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
            => await ExecuteAsync(command, eventData, "EFCore.Reader", async () => await base.ReaderExecutingAsync(command, eventData, result, cancellationToken));

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result)
            => Execute(command, eventData, "EFCore.NonQuery", () => base.NonQueryExecuting(command, eventData, result));

        public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
            => await ExecuteAsync(command, eventData, "EFCore.NonQuery", async () => await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken));

        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result)
            => Execute(command, eventData, "EFCore.Scalar", () => base.ScalarExecuting(command, eventData, result));

        public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result,
            CancellationToken cancellationToken = default)
            => await ExecuteAsync(command, eventData, "EFCore.Scalar", async () => await base.ScalarExecutingAsync(command, eventData, result, cancellationToken));

        #region Private helpers

        private InterceptionResult<T> Execute<T>(
            DbCommand command,
            CommandEventData eventData,
            string operation,
            Func<InterceptionResult<T>> func)
            => instrumentator.Instrument(
                operation,
                func,
                GetCommandMetadata(command, eventData));

        private async ValueTask<InterceptionResult<T>> ExecuteAsync<T>(
            DbCommand command,
            CommandEventData eventData,
            string operation,
            Func<Task<InterceptionResult<T>>> func)
            => await instrumentator.InstrumentAsync(
                operation,
                func,
                GetCommandMetadata(command, eventData));

        private static Dictionary<string, object> GetCommandMetadata(DbCommand command, CommandEventData eventData)
        {
            return new()
            {
                { "Sql", command.CommandText },
                { "CommandType", command.CommandType.ToString() },
                { "DbContextType", eventData.Context?.GetType().Name ?? "Unknown" },
                { "DataSource", command.Connection?.DataSource ?? "Unknown" },
                { "Database", command.Connection?.Database ?? "Unknown" }
            };
        }

        #endregion
    }
}
