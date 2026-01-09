using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Auth.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Algora.Erp.Auth.Services;

/// <summary>
/// Implementation of IIdentityService for password hashing and JWT token management
/// </summary>
public class IdentityService : IIdentityService
{
    private readonly AuthSettings _settings;

    public IdentityService(IOptions<AuthSettings> settings)
    {
        _settings = settings.Value;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(
        Guid userId, string email, IEnumerable<string> roles)
    {
        var accessToken = GenerateAccessToken(userId, email, roles);
        var refreshToken = GenerateRefreshToken();
        return Task.FromResult((accessToken, refreshToken));
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Jwt.Key));
            var tokenHandler = new JwtSecurityTokenHandler();

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _settings.Jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Jwt.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1) // Allow 1 minute clock skew for minor timing differences
            }, out _);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles)
    {
        var jwtSettings = _settings.Jwt ?? throw new InvalidOperationException("JWT settings not configured");
        if (string.IsNullOrEmpty(jwtSettings.Key))
            throw new InvalidOperationException("JWT Key not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(jwtSettings.AccessTokenExpiryMinutes);

        // Build claims for the token
        var claims = new List<Claim>
        {
            new("sub", userId.ToString()),
            new("email", email),
            new("jti", Guid.NewGuid().ToString()),
            new("role", string.Join(",", roles))
        };

        // Create token with all necessary parameters
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}
