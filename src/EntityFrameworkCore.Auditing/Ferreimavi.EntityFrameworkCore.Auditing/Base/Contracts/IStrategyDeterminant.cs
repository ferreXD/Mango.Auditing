namespace Mango.Auditing.Base.Contracts
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public interface IStrategyDeterminant<TEntity, out TStrategy>
        where TEntity : class
        where TStrategy : struct
    {
        TStrategy DetermineStrategy(EntityEntry<TEntity> entry);
    }
}
