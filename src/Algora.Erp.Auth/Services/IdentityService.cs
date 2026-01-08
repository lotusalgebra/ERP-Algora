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
                ClockSkew = TimeSpan.Zero
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
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Jwt.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _settings.Jwt.Issuer,
            audience: _settings.Jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.Jwt.AccessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
