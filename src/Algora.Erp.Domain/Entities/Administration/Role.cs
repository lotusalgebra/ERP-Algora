using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Administration;

/// <summary>
/// Represents a role within a tenant
/// </summary>
public class Role : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
