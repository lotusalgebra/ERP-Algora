using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.HR;
using Algora.Erp.Domain.Entities.Sales;

namespace Algora.Erp.Domain.Entities.Projects;

public class Project : TenantEntity
{
    public string ProjectCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? ProjectManagerId { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public ProjectPriority Priority { get; set; } = ProjectPriority.Normal;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal ActualCost { get; set; }
    public decimal Progress { get; set; } // 0-100 percentage
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public Customer? Customer { get; set; }
    public Employee? ProjectManager { get; set; }
    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();

    public decimal TotalHours => TimeEntries.Sum(t => t.Hours);
    public int TaskCount => Tasks.Count;
    public int CompletedTaskCount => Tasks.Count(t => t.Status == TaskStatus.Completed);
}

public enum ProjectStatus
{
    Planning,
    Active,
    OnHold,
    Completed,
    Cancelled
}

public enum ProjectPriority
{
    Low,
    Normal,
    High,
    Critical
}
