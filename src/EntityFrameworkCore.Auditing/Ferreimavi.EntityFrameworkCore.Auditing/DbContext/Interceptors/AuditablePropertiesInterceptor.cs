// ReSharper disable once CheckNamespace

namespace Mango.Auditing.Interceptors
{
    using AuditableProperties;
    using AuditableProperties.Contracts;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Models;
    using Telemetry;

    public class AuditablePropertiesInterceptor(
        AuditingOptions auditOptions,
        IAuditablePropertiesStrategyDeterminant strategyDeterminant,
        IAuditablePropertiesStrategyDispatcher strategyDispatcher,
        ICurrentUserProvider currentUserProvider,
        IPerformanceMonitor monitor) : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null || !auditOptions.Features.IsEnabled(AuditFeature.TrackUnmodified))
            {
                var message = context == null
                    ? "DbContext is null, skipping auditable properties processing."
                    : "TrackUnmodified feature is disabled, skipping auditable properties processing.";

                monitor.LogInformation(message);

                return await base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            var strategies = context.ChangeTracker.Entries<IAuditableEntity>()
                .GroupBy(x => new StrategyHashKey<AuditableStrategy>(strategyDeterminant.DetermineStrategy(x), x.Entity.GetType().Name))
                .ToDictionary(x => x.Key);

            EvaluateStrategies(strategies);

            foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
            {
                var entity = entry.Entity;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = DateTime.UtcNow;
                        entity.CreatedBy = currentUserProvider.GetCurrentUserId();
                        break;
                    case EntityState.Modified:
                        entity.LastModifiedAt = DateTime.UtcNow;
                        entity.LastModifiedBy = currentUserProvider.GetCurrentUserId();
                        break;
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }



        private void EvaluateStrategies(Dictionary<StrategyHashKey<AuditableStrategy>, IGrouping<StrategyHashKey<AuditableStrategy>, EntityEntry<IAuditableEntity>>> strategies)
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

        private void ApplyStrategy(KeyValuePair<StrategyHashKey<AuditableStrategy>, IGrouping<StrategyHashKey<AuditableStrategy>, EntityEntry<IAuditableEntity>>> strategy)
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
    }
}