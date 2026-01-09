using Algora.Erp.Auth.Models;

namespace Algora.Erp.Auth.Tests.Models;

public class AuthResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var accessToken = "test_access_token";
        var refreshToken = "test_refresh_token";
        var user = new AuthUserInfo
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User"
        };

        // Act
        var result = AuthResult.Success(accessToken, refreshToken, user);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
        result.User.Should().Be(user);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Failed_ShouldCreateFailedResult()
    {
        // Arrange
        var errorMessage = "Invalid credentials";

        // Act
        var result = AuthResult.Failed(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        result.AccessToken.Should().BeNull();
        result.RefreshToken.Should().BeNull();
        result.User.Should().BeNull();
    }
}

public class AuthUserInfoTests
{
    [Fact]
    public void AuthUserInfo_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var userInfo = new AuthUserInfo();

        // Assert
        userInfo.Id.Should().Be(Guid.Empty);
        userInfo.TenantId.Should().Be(Guid.Empty);
        userInfo.Email.Should().BeEmpty();
        userInfo.FullName.Should().BeEmpty();
        userInfo.FirstName.Should().BeEmpty();
        userInfo.LastName.Should().BeEmpty();
        userInfo.Roles.Should().BeEmpty();
        userInfo.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void AuthUserInfo_ShouldStoreUserData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        var userInfo = new AuthUserInfo
        {
            Id = userId,
            TenantId = tenantId,
            Email = "user@example.com",
            FullName = "John Doe",
            FirstName = "John",
            LastName = "Doe",
            Roles = new List<string> { "Admin", "User" },
            Permissions = new List<string> { "read", "write" }
        };

        // Assert
        userInfo.Id.Should().Be(userId);
        userInfo.TenantId.Should().Be(tenantId);
        userInfo.Email.Should().Be("user@example.com");
        userInfo.FullName.Should().Be("John Doe");
        userInfo.FirstName.Should().Be("John");
        userInfo.LastName.Should().Be("Doe");
        userInfo.Roles.Should().HaveCount(2);
        userInfo.Permissions.Should().HaveCount(2);
    }
}
