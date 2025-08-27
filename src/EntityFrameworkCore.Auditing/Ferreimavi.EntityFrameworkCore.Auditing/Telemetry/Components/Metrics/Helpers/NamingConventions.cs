// ReSharper disable once CheckNamespace
namespace Mango.Auditing.Telemetry
{
    internal static class NamingConventions
    {
        internal static string GetFullName(string prefix, string name)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return name;
            }

            return string.IsNullOrWhiteSpace(name)
                ? prefix
                : $"{prefix}.{name}";
        }
    }
}
