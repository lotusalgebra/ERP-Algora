using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Enums;

namespace Algora.Erp.Domain.Entities.Administration;

/// <summary>
/// Represents a user within a tenant's database
/// </summary>
public class User : TenantEntity
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Avatar { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryAt { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public string FullName => $"{FirstName} {LastName}";
}
