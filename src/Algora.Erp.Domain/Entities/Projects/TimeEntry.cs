using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.HR;

namespace Algora.Erp.Domain.Entities.Projects;

public class TimeEntry : TenantEntity
{
    public Guid ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public bool IsBillable { get; set; } = true;
    public TimeEntryStatus Status { get; set; } = TimeEntryStatus.Draft;
    public string? Notes { get; set; }

    public Project? Project { get; set; }
    public ProjectTask? Task { get; set; }
    public Employee? Employee { get; set; }

    public decimal Cost => Hours * (Employee?.BaseSalary / 160 ?? 0); // Rough hourly rate
}

public enum TimeEntryStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected
}
