using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Inventory;

namespace Algora.Erp.Domain.Entities.Manufacturing;

public class WorkOrder : TenantEntity
{
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? BillOfMaterialId { get; set; }
    public Guid ProductId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal CompletedQuantity { get; set; }
    public decimal ScrapQuantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;
    public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Normal;
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public Guid? WarehouseId { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal ActualCost { get; set; }
    public string? Notes { get; set; }

    public BillOfMaterial? BillOfMaterial { get; set; }
    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
    public ICollection<WorkOrderOperation> Operations { get; set; } = new List<WorkOrderOperation>();
    public ICollection<WorkOrderMaterial> Materials { get; set; } = new List<WorkOrderMaterial>();
}

public enum WorkOrderStatus
{
    Draft,
    Released,
    InProgress,
    OnHold,
    Completed,
    Cancelled
}

public enum WorkOrderPriority
{
    Low,
    Normal,
    High,
    Urgent
}
