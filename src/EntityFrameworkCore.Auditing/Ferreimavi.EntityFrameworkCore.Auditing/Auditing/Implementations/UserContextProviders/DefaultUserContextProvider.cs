// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    using System.Collections.Generic;

    /// <summary>
    /// Default implementation of ICurrentUserProvider that can be used in non-HTTP scenarios
    /// </summary>
    public class DefaultCurrentUserProvider : ICurrentUserProvider
    {
        private static readonly AsyncLocal<UserContext> _currentUser = new();

        /// <inheritdoc />
        public string? GetCurrentUserId() => _currentUser.Value?.UserId;

        /// <inheritdoc />
        public string? GetCurrentUserName() => _currentUser.Value?.UserName;

        /// <inheritdoc />
        public IDictionary<string, string> GetAdditionalUserInfo() => _currentUser.Value?.AdditionalInfo ?? new Dictionary<string, string>();

        /// <summary>
        /// Sets the current user context
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="userName">The user name</param>
        /// <param name="additionalInfo">Additional user information</param>
        public static void SetCurrentUser(
            string userId,
            string userName,
            IDictionary<string, string>? additionalInfo = null)
        {
            _currentUser.Value = new UserContext
            {
                UserId = userId,
                UserName = userName,
                AdditionalInfo = additionalInfo ?? new Dictionary<string, string>()
            };
        }

        /// <summary>
        /// Clears the current user context
        /// </summary>
        public static void ClearCurrentUser() => _currentUser.Value = new UserContext();

        private class UserContext
        {
            public string? UserId { get; set; }
            public string? UserName { get; set; }
            public IDictionary<string, string> AdditionalInfo { get; set; } = new Dictionary<string, string>();
        }
    }
}
