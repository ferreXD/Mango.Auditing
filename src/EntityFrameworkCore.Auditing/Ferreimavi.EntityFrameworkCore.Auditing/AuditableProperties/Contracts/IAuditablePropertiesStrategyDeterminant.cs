// ReSharper disable once CheckNamespace
namespace Mango.Auditing.AuditableProperties
{
    using Base.Contracts;

    public interface IAuditablePropertiesStrategyDeterminant : IStrategyDeterminant<IAuditableEntity, AuditableStrategy>;
}
