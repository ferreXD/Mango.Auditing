// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Interceptors
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Models;
    using SoftDeletion;
    using System.Diagnostics;
    using Telemetry;

    public class SoftDeletionInterceptor(
        AuditingOptions auditOptions,
        IPerformanceMonitor monitor,
        ISoftDeletionMetricRecorder metricRecorder,
        ISoftDeletionStrategyDeterminant strategyDeterminant,
        ISoftDeletionStrategyDispatcher strategyDispatcher,
        ICurrentUserProvider currentUserProvider) : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null || !auditOptions.Features.IsEnabled(AuditFeature.SoftDeletion))
            {
                var message = context == null
                    ? "DbContext is null, skipping soft deletion processing."
                    : "Soft deletion feature is disabled, skipping soft deletion processing.";

                monitor.LogInformation(message);
                return await base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            var operationTags = new Dictionary<string, object>
            {
                { "DbContextType", context.GetType().Name },
                { "DbContextHashCode", context.GetHashCode() },
                { "UserId", currentUserProvider.GetCurrentUserId() ?? string.Empty }
            };

            using var operation = monitor.BeginOperation(
                "SoftDeletionInterceptor.SavingChangesAsync",
                ActivityKind.Internal,
                operationTags);

            var activity = Activity.Current;

            // Streamline the strategy determination and processing
            var strategies = context.ChangeTracker.Entries<ISoftDeletableEntity>()
                .GroupBy(x => new StrategyHashKey<DeletionStrategy>(strategyDeterminant.DetermineStrategy(x), x.Entity.GetType().Name))
                .ToDictionary(x => x.Key);

            var stopwatch = Stopwatch.StartNew();

            EvaluateStrategies(strategies);

            stopwatch.Stop();

            var (softDeletedCount, restoredCount, deletedCount, omittedCount) = EvaluateStrategyCounts(strategies);
            SetActivityTags(activity, stopwatch, softDeletedCount, restoredCount, deletedCount, omittedCount);
            RecordGlobalMetrics(softDeletedCount, restoredCount, deletedCount, omittedCount);

            monitor.LogInformation($"Soft deletion processing completed in {stopwatch.Elapsed.TotalSeconds:F2} seconds." +
                                   ("SoftDeletedCount", softDeletedCount),
                ("RestoredCount", restoredCount),
                ("DeletedCount", deletedCount),
                ("OmittedCount", omittedCount));

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void SetActivityTags(Activity? activity, Stopwatch stopwatch, int softDeletedCount, int restoredCount, int deletedCount, int omittedCount)
        {
            activity?.SetTag("Duration", stopwatch.ElapsedMilliseconds);

            activity?.SetTag("SoftDeletedCount", softDeletedCount);
            activity?.SetTag("RestoredCount", restoredCount);
            activity?.SetTag("DeletedCount", deletedCount);
            activity?.SetTag("OmittedCount", omittedCount);
        }

        private void EvaluateStrategies(Dictionary<StrategyHashKey<DeletionStrategy>, IGrouping<StrategyHashKey<DeletionStrategy>, EntityEntry<ISoftDeletableEntity>>> strategies)
        {
            foreach (var strategy in strategies)
            {
                if (!strategy.Value.Any())
                {
                    monitor.LogInformation($"No entities found for strategy {strategy.Key}.");
                    continue;
                }

                ApplyStrategy(strategy);
            }
        }

        private void ApplyStrategy(KeyValuePair<StrategyHashKey<DeletionStrategy>, IGrouping<StrategyHashKey<DeletionStrategy>, EntityEntry<ISoftDeletableEntity>>> strategy)
        {
            if (strategyDispatcher.TryResolve(strategy.Key.Strategy, out var handler))
            {
                handler!(strategy.Key, strategy.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unknown deletion strategy");
            }
        }

        private void RecordGlobalMetrics(int softDeletedCount, int restoredCount, int deletedCount, int omittedCount)
        {
            var type = nameof(ISoftDeletableEntity);

            metricRecorder.RecordSoftDeleteMetric(type, "SoftDeleted", softDeletedCount);
            metricRecorder.RecordSoftDeleteMetric(type, "Restored", restoredCount);
            metricRecorder.RecordSoftDeleteMetric(type, "Deleted", deletedCount);
            metricRecorder.RecordSoftDeleteMetric(type, "Omitted", omittedCount);
        }

        private static (int softDeletedCount, int restoredCount, int deletedCount, int omittedCount) EvaluateStrategyCounts(Dictionary<StrategyHashKey<DeletionStrategy>, IGrouping<StrategyHashKey<DeletionStrategy>, EntityEntry<ISoftDeletableEntity>>> strategies)
        {
            var softDeletedCount = strategies
                .Where(kvp => kvp.Key.Strategy == DeletionStrategy.SoftDelete)
                .Sum(kvp => kvp.Value.Count());

            var restoredCount = strategies
                .Where(kvp => kvp.Key.Strategy == DeletionStrategy.Restore)
                .Sum(kvp => kvp.Value.Count());

            var deletedCount = strategies
                .Where(kvp => kvp.Key.Strategy == DeletionStrategy.Delete)
                .Sum(kvp => kvp.Value.Count());

            var omittedCount = strategies
                .Where(kvp => kvp.Key.Strategy == DeletionStrategy.None)
                .Sum(kvp => kvp.Value.Count());

            return (softDeletedCount, restoredCount, deletedCount, omittedCount);
        }
    }
}