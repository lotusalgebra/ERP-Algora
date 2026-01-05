using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Administration;

/// <summary>
/// Many-to-many relationship between Role and Permission
/// </summary>
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    // Navigation properties
    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}
