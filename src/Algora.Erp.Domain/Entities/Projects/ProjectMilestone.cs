using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Projects;

public class ProjectMilestone : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;
    public int SortOrder { get; set; }
    public string? Notes { get; set; }

    public Project? Project { get; set; }

    public bool IsOverdue => Status != MilestoneStatus.Completed && DueDate < DateTime.UtcNow;
}

public enum MilestoneStatus
{
    Pending,
    InProgress,
    Completed,
    Missed
}
