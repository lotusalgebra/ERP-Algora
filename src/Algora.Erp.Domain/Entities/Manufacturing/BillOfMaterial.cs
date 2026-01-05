using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Inventory;

namespace Algora.Erp.Domain.Entities.Manufacturing;

public class BillOfMaterial : TenantEntity
{
    public string BomNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public string? UnitOfMeasure { get; set; }
    public BomStatus Status { get; set; } = BomStatus.Draft;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public Product? Product { get; set; }
    public ICollection<BomLine> Lines { get; set; } = new List<BomLine>();

    public decimal TotalMaterialCost => Lines.Sum(l => l.TotalCost);
}

public enum BomStatus
{
    Draft,
    Active,
    Obsolete
}
