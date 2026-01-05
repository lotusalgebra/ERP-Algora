using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.HR;

namespace Algora.Erp.Domain.Entities.Projects;

public class ProjectMember : TenantEntity
{
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public ProjectRole Role { get; set; } = ProjectRole.Member;
    public decimal HourlyRate { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public bool IsActive { get; set; } = true;

    public Project? Project { get; set; }
    public Employee? Employee { get; set; }
}

public enum ProjectRole
{
    Member,
    Lead,
    Manager,
    Stakeholder
}
