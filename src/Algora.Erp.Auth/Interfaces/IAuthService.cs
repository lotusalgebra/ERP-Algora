using Algora.Erp.Auth.Models;

namespace Algora.Erp.Auth.Interfaces;

/// <summary>
/// Authentication service for tenant users
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    Task<AuthResult> LoginAsync(string email, string password, string? ipAddress = null);

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    Task<AuthResult> RefreshTokenAsync(string refreshToken, string? ipAddress = null);

    /// <summary>
    /// Revoke refresh token (logout)
    /// </summary>
    Task<bool> RevokeTokenAsync(Guid userId);

    /// <summary>
    /// Register a new user
    /// </summary>
    Task<AuthResult> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Initiate forgot password flow
    /// </summary>
    Task<bool> ForgotPasswordAsync(string email);

    /// <summary>
    /// Reset password using token
    /// </summary>
    Task<bool> ResetPasswordAsync(string token, string newPassword);

    /// <summary>
    /// Change password for authenticated user
    /// </summary>
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

    /// <summary>
    /// Get user info by ID
    /// </summary>
    Task<AuthUserInfo?> GetUserInfoAsync(Guid userId);
}
