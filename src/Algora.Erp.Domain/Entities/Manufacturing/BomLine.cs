using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Inventory;

namespace Algora.Erp.Domain.Entities.Manufacturing;

public class BomLine : TenantEntity
{
    public Guid BillOfMaterialId { get; set; }
    public Guid ProductId { get; set; }
    public int LineNumber { get; set; }
    public decimal Quantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal WastagePercent { get; set; }
    public bool IsOptional { get; set; }
    public string? Notes { get; set; }

    public BillOfMaterial? BillOfMaterial { get; set; }
    public Product? Product { get; set; }
}
