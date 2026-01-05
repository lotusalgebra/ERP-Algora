namespace Algora.Erp.Application.Common.Interfaces;

/// <summary>
/// Service for identity operations (authentication, password hashing)
/// </summary>
public interface IIdentityService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    string GenerateRefreshToken();
    Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(Guid userId, string email, IEnumerable<string> roles);
    Task<bool> ValidateTokenAsync(string token);
}
