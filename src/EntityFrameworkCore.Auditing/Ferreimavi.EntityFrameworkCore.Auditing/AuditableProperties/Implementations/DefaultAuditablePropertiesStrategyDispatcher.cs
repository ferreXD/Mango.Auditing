namespace Mango.Auditing.AuditableProperties.Implementations
{
    using Contracts;
    using Mango.Auditing.SoftDeletion;
    using Mango.Auditing.Telemetry;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class DefaultAuditablePropertiesStrategyDispatcher : IAuditablePropertiesStrategyDispatcher
    {
        private readonly Dictionary<AuditableStrategy, Func<StrategyHashKey<AuditableStrategy>, IEnumerable<EntityEntry<IAuditableEntity>>, ValueTask>> _strategyHandlers;

        private readonly IPerformanceMonitor _monitor;
        private readonly ICurrentUserProvider _currentUserProvider;

        public DefaultAuditablePropertiesStrategyDispatcher(
            IPerformanceMonitor monitor,
            ISoftDeletionHandler handler,
            ISoftDeletionMetricRecorder metricRecorder,
            ICurrentUserProvider currentUserProvider)
        {
            // Ensure the dictionary is initialized with all handlers
            _strategyHandlers = new()
            {
                [AuditableStrategy.Added] = HandleAdded,
                [AuditableStrategy.Modified] = HandleModified,
                [AuditableStrategy.None] = (_, _) => ValueTask.CompletedTask
            };

            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _currentUserProvider = currentUserProvider ?? throw new ArgumentNullException(nameof(currentUserProvider));
        }

        public bool TryResolve(AuditableStrategy strategy, out Func<StrategyHashKey<AuditableStrategy>, IEnumerable<EntityEntry<IAuditableEntity>>, ValueTask>? handler)
            => _strategyHandlers.TryGetValue(strategy, out handler);

        private ValueTask HandleAdded(StrategyHashKey<AuditableStrategy> key, IEnumerable<EntityEntry<IAuditableEntity>> entries)
        {
            var count = entries.Count();
            var stopwatch = Stopwatch.StartNew();

            entries.ToList().ForEach(entry =>
            {
                var entity = entry.Entity;
                entity.CreatedAt = DateTime.UtcNow;
                entity.CreatedBy = _currentUserProvider.GetCurrentUserId();
            });

            stopwatch.Stop();

            _monitor.LogInformation($"[AuditableProperties::Added] Added {count} entities of type {key.Type} in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");

            return ValueTask.CompletedTask;
        }

        private ValueTask HandleModified(StrategyHashKey<AuditableStrategy> key, IEnumerable<EntityEntry<IAuditableEntity>> entries)
        {
            var count = entries.Count();
            var stopwatch = Stopwatch.StartNew();

            entries.ToList().ForEach(entry =>
            {
                var entity = entry.Entity;
                entity.LastModifiedAt = DateTime.UtcNow;
                entity.LastModifiedBy = _currentUserProvider.GetCurrentUserId();
            });

            stopwatch.Stop();

            _monitor.LogInformation($"[AuditableProperties::Modified] Modified {count} entities of type {key.Type} in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");

            return ValueTask.CompletedTask;
        }
    }
}
