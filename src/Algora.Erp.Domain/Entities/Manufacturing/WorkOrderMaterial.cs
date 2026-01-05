using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Inventory;

namespace Algora.Erp.Domain.Entities.Manufacturing;

public class WorkOrderMaterial : TenantEntity
{
    public Guid WorkOrderId { get; set; }
    public Guid ProductId { get; set; }
    public int LineNumber { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal IssuedQuantity { get; set; }
    public decimal ReturnedQuantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal UnitCost { get; set; }
    public MaterialStatus Status { get; set; } = MaterialStatus.Pending;
    public string? Notes { get; set; }

    public WorkOrder? WorkOrder { get; set; }
    public Product? Product { get; set; }

    public decimal ConsumedQuantity => IssuedQuantity - ReturnedQuantity;
    public decimal TotalCost => ConsumedQuantity * UnitCost;
}

public enum MaterialStatus
{
    Pending,
    PartiallyIssued,
    Issued,
    Returned
}
