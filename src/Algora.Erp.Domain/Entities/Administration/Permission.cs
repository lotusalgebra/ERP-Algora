using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Administration;

/// <summary>
/// Represents a permission that can be assigned to roles
/// </summary>
public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
