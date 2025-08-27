namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup.Factories.Mocks.Http
{
    using Mango.Auditing;
    using Moq;

    public static class CurrentUserProviderMockFactory
    {
        /// <summary>
        /// Creates a mock ICurrentUserProvider.
        /// </summary>
        /// <param name="userId">The value GetCurrentUserId() should return.</param>
        /// <param name="userName">The value GetCurrentUserName() should return.</param>
        /// <param name="additionalInfo">
        /// The dictionary GetAdditionalUserInfo() should return.
        /// If null, an empty dictionary is returned.
        /// </param>
        public static Mock<ICurrentUserProvider> Create(
            string? userId = "test-user-id",
            string? userName = "test-user",
            IDictionary<string, string>? additionalInfo = null)
        {
            var mock = new Mock<ICurrentUserProvider>();

            mock
                .Setup(m => m.GetCurrentUserId())
                .Returns(userId);

            mock
                .Setup(m => m.GetCurrentUserName())
                .Returns(userName);

            mock
                .Setup(m => m.GetAdditionalUserInfo())
                .Returns(additionalInfo ?? new Dictionary<string, string>());

            return mock;
        }
    }
}