// ReSharper disable once CheckNamespace

namespace EntityFrameworkCore.Auditing.Tests.UnitTests
{
    using Mango.Auditing;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Setup.Factories.Scenarios;
    using System.Security.Claims;

    public class DefaultHttpContextCurrentUserProviderTests
    {
        [Fact]
        public void GetCurrentUserId_Should_ReturnEmpty_When_HttpContextIsNull()
        {
            // Arrange
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            var provider = new DefaultHttpContextCurrentUserProvider(accessorMock.Object);

            // Act
            var userId = provider.GetCurrentUserId();

            // Assert
            userId
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GetCurrentUserName_Should_ReturnNull_When_HttpContextIsNull()
        {
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            var provider = new DefaultHttpContextCurrentUserProvider(accessorMock.Object);

            // Act
            var username = provider.GetCurrentUserName();

            // Assert
            username
                .Should()
                .BeNull();
        }

        [Fact]
        public void GetAdditionalUserInfo_Should_ReturnEmpty_When_HttpContextIsNull()
        {
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            var provider = new DefaultHttpContextCurrentUserProvider(accessorMock.Object);

            // Act
            var info = provider.GetAdditionalUserInfo();

            // Assert
            info
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GetCurrentUserId_Should_PickUp_NameIdentifierClaim()
        {
            // Arrange
            var scenario = HttpContextAccessorMockScenarioFactory.Create(ctx =>
            {
                var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
                ctx.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
            });

            var provider = new DefaultHttpContextCurrentUserProvider(scenario.Accessor);

            // Act
            var userId = provider.GetCurrentUserId();

            // Assert
            userId
                .Should()
                .Be("user-123");
        }

        [Fact]
        public void GetCurrentUserName_Should_PickUp_IdentityName()
        {
            var scenario = HttpContextAccessorMockScenarioFactory.Create(ctx =>
            {
                var identity = new ClaimsIdentity("Test");
                identity.AddClaim(new Claim(ClaimTypes.Name, "alice"));
                ctx.User = new ClaimsPrincipal(identity);
            });

            var provider = new DefaultHttpContextCurrentUserProvider(scenario.Accessor);

            // Act
            var name = provider.GetCurrentUserName();

            // Assert
            name
                .Should()
                .Be("alice");
        }

        [Fact]
        public void GetAdditionalUserInfo_Should_Include_CommonClaims()
        {
            var scenario = HttpContextAccessorMockScenarioFactory.Create(ctx =>
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Email, "alice@example.com"),
                    new Claim(ClaimTypes.GivenName, "Alice"),
                    new Claim(ClaimTypes.Surname, "Smith"),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim(ClaimTypes.Role, "User")
                };

                ctx.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
            });

            var provider = new DefaultHttpContextCurrentUserProvider(scenario.Accessor);

            // Act
            var info = provider.GetAdditionalUserInfo();

            // Assert
            info
                .Should()
                .ContainKey("Email")
                .WhoseValue
                .Should()
                .Be("alice@example.com");

            info
                .Should()
                .ContainKey("FirstName")
                .WhoseValue
                .Should()
                .Be("Alice");

            info
                .Should()
                .ContainKey("LastName")
                .WhoseValue
                .Should()
                .Be("Smith");

            info
                .Should()
                .ContainKey("Roles")
                .WhoseValue
                .Should()
                .Be("Admin,User");
        }

        [Fact]
        public void GetAdditionalUserInfo_Should_Include_CustomClaimsWithPrefix()
        {
            var scenario = HttpContextAccessorMockScenarioFactory.Create(ctx =>
            {
                var claims = new[]
                {
                    new Claim("X-Dept", "Engineering"),
                    new Claim("X-Region", "EMEA")
                };

                ctx.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
            });

            var provider = new DefaultHttpContextCurrentUserProvider(scenario.Accessor);

            // Act
            var info = provider.GetAdditionalUserInfo();

            // Assert
            info
                .Should()
                .ContainKey("custom:X-Dept").WhoseValue
                .Should()
                .Be("Engineering");

            info
                .Should()
                .ContainKey("custom:X-Region").WhoseValue
                .Should()
                .Be("EMEA");
        }

        [Fact]
        public void GetAdditionalUserInfo_Should_Mix_CommonAndCustomClaims()
        {
            var scenario = HttpContextAccessorMockScenarioFactory.Create(ctx =>
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Email, "bob@example.com"),
                    new Claim("Dept", "HR")
                };
                ctx.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
            });
            var provider = new DefaultHttpContextCurrentUserProvider(scenario.Accessor);

            // Act
            var info = provider.GetAdditionalUserInfo();

            // Assert
            info
                .Should()
                .Contain(new KeyValuePair<string, string>("Email", "bob@example.com"))
                .And
                .Contain(new KeyValuePair<string, string>("custom:Dept", "HR"));
        }

        [Fact]
        public void GetAdditionalUserInfo_Should_Omit_EmptyCommonClaims()
        {
            var scenario = HttpContextAccessorMockScenarioFactory.Create(ctx =>
            {
                // no email, givenName, surname at all
                ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim("Custom1", "Val1")
                }, "Test"));
            });

            var provider = new DefaultHttpContextCurrentUserProvider(scenario.Accessor);

            // Act
            var info = provider.GetAdditionalUserInfo();

            // Assert
            info
                .Should()
                .NotContainKey("Email");

            info
                .Should()
                .NotContainKey("FirstName");

            info
                .Should()
                .NotContainKey("LastName");

            info
                .Should()
                .ContainKey("custom:Custom1");
        }
    }
}