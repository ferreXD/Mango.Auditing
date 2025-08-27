// ReSharper disable once CheckNamespace
namespace Mango.Auditing.SoftDeletion
{
    using Base.Contracts;

    public interface ISoftDeletionStrategyDeterminant : IStrategyDeterminant<ISoftDeletableEntity, DeletionStrategy>;
}
