namespace Algora.Erp.Admin.Entities;

/// <summary>
/// Admin/Host user for managing tenants and billing
/// </summary>
public class AdminUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();

    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }

    // Role
    public Guid RoleId { get; set; }
    public AdminRole? Role { get; set; }

    // Status
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; }
    public DateTime? EmailConfirmedAt { get; set; }

    // Security
    public int AccessFailedCount { get; set; }
    public bool LockoutEnabled { get; set; } = true;
    public DateTime? LockoutEnd { get; set; }
    public string? SecurityStamp { get; set; }
    public string? TwoFactorSecret { get; set; }
    public bool TwoFactorEnabled { get; set; }

    // Password Reset
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    // Email Confirmation
    public string? EmailConfirmationToken { get; set; }
    public DateTime? EmailConfirmationTokenExpiry { get; set; }

    // Login Tracking
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public Guid? ModifiedBy { get; set; }

    // Refresh Tokens
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

/// <summary>
/// Admin roles for access control
/// </summary>
public class AdminRole
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Permissions (JSON array)
    public string Permissions { get; set; } = "[]";

    public bool IsSystemRole { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    public ICollection<AdminUser> Users { get; set; } = new List<AdminUser>();
}

/// <summary>
/// Refresh tokens for JWT authentication
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public AdminUser? User { get; set; }

    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? RevokeReason { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;
}

/// <summary>
/// Admin permissions enum
/// </summary>
public static class AdminPermissions
{
    // Tenant Management
    public const string TenantsView = "tenants.view";
    public const string TenantsCreate = "tenants.create";
    public const string TenantsEdit = "tenants.edit";
    public const string TenantsDelete = "tenants.delete";
    public const string TenantsSuspend = "tenants.suspend";

    // Billing Management
    public const string BillingView = "billing.view";
    public const string BillingManage = "billing.manage";
    public const string PlansManage = "plans.manage";

    // User Management
    public const string UsersView = "users.view";
    public const string UsersManage = "users.manage";

    // Reports
    public const string ReportsView = "reports.view";

    // System
    public const string SystemSettings = "system.settings";

    public static readonly string[] All = new[]
    {
        TenantsView, TenantsCreate, TenantsEdit, TenantsDelete, TenantsSuspend,
        BillingView, BillingManage, PlansManage,
        UsersView, UsersManage,
        ReportsView,
        SystemSettings
    };
}
