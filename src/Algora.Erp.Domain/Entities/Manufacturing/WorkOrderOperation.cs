using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Manufacturing;

public class WorkOrderOperation : TenantEntity
{
    public Guid WorkOrderId { get; set; }
    public int OperationNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Workstation { get; set; }
    public decimal PlannedHours { get; set; }
    public decimal ActualHours { get; set; }
    public OperationStatus Status { get; set; } = OperationStatus.Pending;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }

    public WorkOrder? WorkOrder { get; set; }
}

public enum OperationStatus
{
    Pending,
    InProgress,
    Completed,
    Skipped
}
