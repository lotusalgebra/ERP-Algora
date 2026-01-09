using System.ComponentModel.DataAnnotations;
using Algora.Erp.Auth.Models;

namespace Algora.Erp.Auth.Tests.Models;

public class LoginRequestTests
{
    [Fact]
    public void LoginRequest_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var request = new LoginRequest();

        // Assert
        request.Email.Should().BeEmpty();
        request.Password.Should().BeEmpty();
        request.RememberMe.Should().BeFalse();
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    public void LoginRequest_ShouldValidateEmail(string email, bool isValid)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = email,
            Password = "ValidPassword123"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        var hasEmailError = validationResults.Any(v => v.MemberNames.Contains("Email"));
        hasEmailError.Should().Be(!isValid);
    }

    [Theory]
    [InlineData("password123", true)]
    [InlineData("", false)]
    public void LoginRequest_ShouldValidatePassword(string password, bool isValid)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = password
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        var hasPasswordError = validationResults.Any(v => v.MemberNames.Contains("Password"));
        hasPasswordError.Should().Be(!isValid);
    }

    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults;
    }
}

public class RegisterRequestTests
{
    [Fact]
    public void RegisterRequest_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var request = new RegisterRequest();

        // Assert
        request.Email.Should().BeEmpty();
        request.Password.Should().BeEmpty();
        request.FirstName.Should().BeEmpty();
        request.LastName.Should().BeEmpty();
        request.PhoneNumber.Should().BeNull();
    }

    [Theory]
    [InlineData("ValidPassword123", true)]
    [InlineData("12345", false)] // Too short
    [InlineData("", false)]
    public void RegisterRequest_ShouldValidatePassword(string password, bool isValid)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        var hasPasswordError = validationResults.Any(v => v.MemberNames.Contains("Password"));
        hasPasswordError.Should().Be(!isValid);
    }

    [Theory]
    [InlineData("John", "Doe", true)]
    [InlineData("", "Doe", false)]
    [InlineData("John", "", false)]
    public void RegisterRequest_ShouldValidateNames(string firstName, string lastName, bool isValid)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "ValidPassword123",
            FirstName = firstName,
            LastName = lastName
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        var hasNameError = validationResults.Any(v =>
            v.MemberNames.Contains("FirstName") || v.MemberNames.Contains("LastName"));
        hasNameError.Should().Be(!isValid);
    }

    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults;
    }
}

public class ChangePasswordRequestTests
{
    [Fact]
    public void ChangePasswordRequest_ShouldValidateMatchingPasswords()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "NewPassword123",
            ConfirmPassword = "NewPassword123"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void ChangePasswordRequest_ShouldFailForMismatchedPasswords()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "NewPassword123",
            ConfirmPassword = "DifferentPassword123"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().Contain(v => v.MemberNames.Contains("ConfirmPassword"));
    }

    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults;
    }
}
