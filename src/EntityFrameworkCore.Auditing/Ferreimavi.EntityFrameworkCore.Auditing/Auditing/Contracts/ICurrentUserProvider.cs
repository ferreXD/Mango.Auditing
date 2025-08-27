// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    public interface ICurrentUserProvider
    {
        string? GetCurrentUserId();
        string? GetCurrentUserName();
        IDictionary<string, string> GetAdditionalUserInfo();
    }
}