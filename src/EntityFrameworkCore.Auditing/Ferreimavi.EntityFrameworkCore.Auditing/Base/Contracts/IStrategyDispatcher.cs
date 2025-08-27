namespace Mango.Auditing.Base.Contracts
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IStrategyDispatcher<TStrategy, TEntity>
        where TStrategy : Enum
        where TEntity : class
    {
        bool TryResolve(TStrategy strategy, out Func<StrategyHashKey<TStrategy>, IEnumerable<EntityEntry<TEntity>>, ValueTask>? handler);
    }
}
