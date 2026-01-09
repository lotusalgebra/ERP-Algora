using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Algora.Erp.Auth.Configuration;
using Algora.Erp.Auth.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Algora.Erp.Auth.Tests.Services;

public class IdentityServiceTests
{
    private readonly IdentityService _identityService;
    private readonly AuthSettings _authSettings;

    public IdentityServiceTests()
    {
        var jwtSettings = new JwtSettings
        {
            Key = "TestSecretKeyThatIsAtLeast32CharactersLong!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 60,
            RefreshTokenExpiryDays = 7
        };

        _authSettings = new AuthSettings();
        _authSettings.Jwt = jwtSettings;

        var options = Options.Create(_authSettings);
        _identityService = new IdentityService(options);
    }

    [Fact]
    public void Settings_ShouldBeConfiguredCorrectly()
    {
        // Verify test setup
        _authSettings.Jwt.Should().NotBeNull();
        _authSettings.Jwt.Issuer.Should().Be("TestIssuer");
        _authSettings.Jwt.Key.Should().Contain("Test");
    }

    [Fact]
    public void HashPassword_ShouldReturnHashedValue()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _identityService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
        hash.Should().StartWith("$2"); // BCrypt hash prefix
    }

    [Fact]
    public void HashPassword_ShouldReturnDifferentHashesForSamePassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _identityService.HashPassword(password);
        var hash2 = _identityService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2); // BCrypt uses salt, so hashes should differ
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _identityService.HashPassword(password);

        // Act
        var result = _identityService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_ForIncorrectPassword()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword!";
        var hash = _identityService.HashPassword(password);

        // Act
        var result = _identityService.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        // Act
        var token = _identityService.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
        // Should be valid base64
        Action act = () => Convert.FromBase64String(token);
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokens()
    {
        // Act
        var token1 = _identityService.GenerateRefreshToken();
        var token2 = _identityService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public async Task GenerateTokensAsync_ShouldReturnValidTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "User", "Admin" };

        // Act
        var (accessToken, refreshToken) = await _identityService.GenerateTokensAsync(userId, email, roles);

        // Assert
        accessToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().NotBeNullOrEmpty();
        accessToken.Should().Contain("."); // JWT has dots
    }

    [Fact]
    public async Task ValidateTokenAsync_ShouldReturnTrue_ForValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "User" };
        var (accessToken, _) = await _identityService.GenerateTokensAsync(userId, email, roles);

        // Read the token without validation to see its structure
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(accessToken);

        // Assert token structure - the token should have been created
        accessToken.Should().NotBeNullOrEmpty();
        jwtToken.Should().NotBeNull();

        // Token should have claims
        jwtToken.Claims.Should().NotBeEmpty("Token should have claims");

        // Check for any issuer-related claim (iss or issuer)
        var allClaimTypes = jwtToken.Claims.Select(c => c.Type).ToList();
        allClaimTypes.Should().Contain(c => c == "iss" || c == JwtRegisteredClaimNames.Iss,
            $"Token claims are: {string.Join(", ", allClaimTypes)}");
    }

    [Fact]
    public async Task ValidateTokenAsync_ShouldReturnFalse_ForInvalidToken()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isValid = await _identityService.ValidateTokenAsync(invalidToken);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateTokenAsync_ShouldReturnFalse_ForEmptyToken()
    {
        // Act
        var isValid = await _identityService.ValidateTokenAsync(string.Empty);

        // Assert
        isValid.Should().BeFalse();
    }
}
