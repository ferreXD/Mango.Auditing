// ReSharper disable once CheckNamespace
namespace Mango.Auditing.SoftDeletion
{
    using Base.Contracts;

    public interface ISoftDeletionStrategyDispatcher : IStrategyDispatcher<DeletionStrategy, ISoftDeletableEntity>;
}
