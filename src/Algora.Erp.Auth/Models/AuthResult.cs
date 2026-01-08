namespace Algora.Erp.Auth.Models;

/// <summary>
/// Result of an authentication operation
/// </summary>
public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public AuthUserInfo? User { get; set; }

    public static AuthResult Success(string accessToken, string refreshToken, AuthUserInfo user)
    {
        return new AuthResult
        {
            IsSuccess = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = user
        };
    }

    public static AuthResult Failed(string errorMessage)
    {
        return new AuthResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// User information returned after successful authentication
/// </summary>
public class AuthUserInfo
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
    public IList<string> Permissions { get; set; } = new List<string>();
}
