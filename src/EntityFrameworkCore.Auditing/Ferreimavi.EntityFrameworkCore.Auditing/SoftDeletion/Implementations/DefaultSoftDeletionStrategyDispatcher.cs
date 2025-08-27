// ReSharper disable once CheckNamespace
namespace Mango.Auditing.SoftDeletion
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Telemetry;

    public class DefaultSoftDeletionStrategyDispatcher : ISoftDeletionStrategyDispatcher
    {
        private readonly Dictionary<DeletionStrategy, Func<StrategyHashKey<DeletionStrategy>, IEnumerable<EntityEntry<ISoftDeletableEntity>>, ValueTask>> _strategyHandlers;

        private readonly IPerformanceMonitor _monitor;
        private readonly ISoftDeletionHandler _handler;
        private readonly ISoftDeletionMetricRecorder _metricRecorder;
        private readonly ICurrentUserProvider _currentUserProvider;

        public DefaultSoftDeletionStrategyDispatcher(
            IPerformanceMonitor monitor,
            ISoftDeletionHandler handler,
            ISoftDeletionMetricRecorder metricRecorder,
            ICurrentUserProvider currentUserProvider)
        {
            // Ensure the dictionary is initialized with all handlers
            _strategyHandlers = new()
            {
                [DeletionStrategy.SoftDelete] = HandleSoftDelete,
                [DeletionStrategy.Restore] = HandleRestore,
                [DeletionStrategy.Delete] = HandleDelete,
                [DeletionStrategy.None] = HandleNone
            };

            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _metricRecorder = metricRecorder ?? throw new ArgumentNullException(nameof(metricRecorder));
            _currentUserProvider = currentUserProvider ?? throw new ArgumentNullException(nameof(currentUserProvider));
        }

        public bool TryResolve(DeletionStrategy strategy, out Func<StrategyHashKey<DeletionStrategy>, IEnumerable<EntityEntry<ISoftDeletableEntity>>, ValueTask>? handler)
            => _strategyHandlers.TryGetValue(strategy, out handler);

        private ValueTask HandleSoftDelete(StrategyHashKey<DeletionStrategy> key, IEnumerable<EntityEntry<ISoftDeletableEntity>> entries)
        {
            var count = entries.Count();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _handler.MarkAsDeleted(entries, _currentUserProvider.GetCurrentUserId());

                _metricRecorder.RecordSoftDeleteMetric(key.Type, "TotalSoftDeleted", count);

                _monitor.LogInformation($"[SoftDeletion::SoftDelete] Soft deleted {count} entities of type {key.Type}.");
            }
            catch (Exception ex)
            {
                _monitor.RecordException(ex, new Dictionary<string, object>()
                {
                    {$"SoftDeletion.Errors.SoftDelete.{key.Type}", true}
                });

                _monitor.LogError(ex, $"[SoftDeletion::SoftDelete] Error soft deleting entities of type {key.Type}.");
            }
            finally
            {
                stopwatch.Stop();
                _metricRecorder.RecordSoftDeleteMetric(key.Type, "SoftDeleteDuration", stopwatch.ElapsedMilliseconds);
            }

            return ValueTask.CompletedTask;
        }

        private ValueTask HandleRestore(StrategyHashKey<DeletionStrategy> key, IEnumerable<EntityEntry<ISoftDeletableEntity>> entries)
        {
            var count = entries.Count();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _handler.Restore(entries);

                _metricRecorder.RecordSoftDeleteMetric(key.Type, "TotalRestored", count);

                _monitor.LogInformation($"[SoftDeletion::Restore] Restored {count} entities of type {key.Type}.");
            }
            catch (Exception ex)
            {
                _monitor.RecordException(ex, new Dictionary<string, object>()
                {
                    {$"SoftDeletion.Errors.Restore.{key.Type}", true}
                });

                _monitor.LogError(ex, $"[SoftDeletion::Restore] Error restoring entities of type {key.Type}.");
            }
            finally
            {
                stopwatch.Stop();
                _metricRecorder.RecordSoftDeleteMetric(key.Type, "RestoreDuration", stopwatch.ElapsedMilliseconds);
            }

            return ValueTask.CompletedTask;
        }

        private ValueTask HandleDelete(StrategyHashKey<DeletionStrategy> key, IEnumerable<EntityEntry<ISoftDeletableEntity>> entries)
        {
            var count = entries.Count();

            _metricRecorder.RecordSoftDeleteMetric(key.Type, "TotalDeleted", count);
            _monitor.LogInformation($"[SoftDeletion::Delete] Deleting {count} entities of type {key.Type}.");

            return ValueTask.CompletedTask;
        }

        private ValueTask HandleNone(StrategyHashKey<DeletionStrategy> key, IEnumerable<EntityEntry<ISoftDeletableEntity>> entries)
        {
            var count = entries.Count();

            _metricRecorder.RecordSoftDeleteMetric(key.Type, "TotalOmitted", count);
            _monitor.LogInformation($"[SoftDeletion::Omit] Omitting {count} entities of type {key.Type} from soft deletion processing.");

            return ValueTask.CompletedTask;
        }
    }
}
