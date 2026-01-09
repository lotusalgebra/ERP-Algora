using Algora.Erp.Domain.Entities.Administration;
using Algora.Erp.Domain.Enums;

namespace Algora.Erp.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void User_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().BeEmpty();
        user.FirstName.Should().BeEmpty();
        user.LastName.Should().BeEmpty();
        user.Status.Should().Be(UserStatus.Active);
        user.EmailConfirmed.Should().BeFalse();
        user.TwoFactorEnabled.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(0);
        user.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void User_FullName_ShouldCombineFirstAndLastName()
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void User_FullName_ShouldHandleEmptyNames()
    {
        // Arrange
        var user = new User
        {
            FirstName = "",
            LastName = ""
        };

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be(" ");
    }

    [Fact]
    public void User_ShouldStorePasswordHash()
    {
        // Arrange
        var user = new User();
        var passwordHash = "hashed_password_value";

        // Act
        user.PasswordHash = passwordHash;

        // Assert
        user.PasswordHash.Should().Be(passwordHash);
    }

    [Fact]
    public void User_ShouldTrackFailedLoginAttempts()
    {
        // Arrange
        var user = new User();

        // Act
        user.FailedLoginAttempts = 3;

        // Assert
        user.FailedLoginAttempts.Should().Be(3);
    }

    [Fact]
    public void User_LockoutEndAt_ShouldBeSettable()
    {
        // Arrange
        var user = new User();
        var lockoutEnd = DateTime.UtcNow.AddMinutes(15);

        // Act
        user.LockoutEndAt = lockoutEnd;

        // Assert
        user.LockoutEndAt.Should().Be(lockoutEnd);
    }

    [Fact]
    public void User_RefreshToken_ShouldBeSettable()
    {
        // Arrange
        var user = new User();
        var refreshToken = "test_refresh_token";
        var expiryDate = DateTime.UtcNow.AddDays(7);

        // Act
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryAt = expiryDate;

        // Assert
        user.RefreshToken.Should().Be(refreshToken);
        user.RefreshTokenExpiryAt.Should().Be(expiryDate);
    }
}
