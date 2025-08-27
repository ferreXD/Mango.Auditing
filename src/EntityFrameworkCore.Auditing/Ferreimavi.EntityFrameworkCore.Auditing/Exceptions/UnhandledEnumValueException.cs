// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    public class UnhandledEnumValueException<T>(T value) : Exception($"Unhandled enum value: {value}") where T : Enum;
}