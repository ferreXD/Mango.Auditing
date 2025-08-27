namespace Mango.Auditing.AuditableProperties.Contracts
{
    using Mango.Auditing.Base.Contracts;

    public interface IAuditablePropertiesStrategyDispatcher : IStrategyDispatcher<AuditableStrategy, IAuditableEntity>;
}
