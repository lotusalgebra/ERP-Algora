using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Administration;

/// <summary>
/// Many-to-many relationship between User and Role
/// </summary>
public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid? AssignedBy { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Role? Role { get; set; }
}
