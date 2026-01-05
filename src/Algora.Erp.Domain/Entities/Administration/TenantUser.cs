using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Administration;

/// <summary>
/// Represents a user in the master database who can access tenants
/// Used for super admins and multi-tenant users
/// </summary>
public class TenantUser : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public Tenant? Tenant { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
