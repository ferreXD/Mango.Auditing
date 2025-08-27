// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Models
{
    public record StrategyHashKey<T>(T Strategy, string Type)
    {
        public override string ToString() => $"{Type}::{Strategy}";
    }
}
