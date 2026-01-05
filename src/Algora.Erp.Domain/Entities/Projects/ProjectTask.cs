using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.HR;

namespace Algora.Erp.Domain.Entities.Projects;

public class ProjectTask : TenantEntity
{
    public Guid ProjectId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public Guid? AssigneeId { get; set; }
    public string TaskNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Normal;
    public DateTime? DueDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }

    public Project? Project { get; set; }
    public ProjectTask? ParentTask { get; set; }
    public Employee? Assignee { get; set; }
    public ICollection<ProjectTask> SubTasks { get; set; } = new List<ProjectTask>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}

public enum TaskStatus
{
    Todo,
    InProgress,
    InReview,
    Completed,
    Cancelled
}

public enum TaskPriority
{
    Low,
    Normal,
    High,
    Urgent
}
