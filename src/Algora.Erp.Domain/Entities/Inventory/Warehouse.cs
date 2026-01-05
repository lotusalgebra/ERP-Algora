using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Inventory;

public class Warehouse : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Location
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    // Contact
    public string? ManagerName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }

    public ICollection<WarehouseLocation> Locations { get; set; } = new List<WarehouseLocation>();
    public ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
}

public class WarehouseLocation : AuditableEntity
{
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    // Location path (e.g., Zone A / Row 1 / Shelf 2)
    public string? Zone { get; set; }
    public string? Aisle { get; set; }
    public string? Rack { get; set; }
    public string? Shelf { get; set; }
    public string? Bin { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
}

public class StockLevel : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public Guid? LocationId { get; set; }
    public WarehouseLocation? Location { get; set; }

    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityOnOrder { get; set; }

    public decimal AvailableQuantity => QuantityOnHand - QuantityReserved;
}

public class StockMovement : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public Guid? FromLocationId { get; set; }
    public WarehouseLocation? FromLocation { get; set; }

    public Guid? ToLocationId { get; set; }
    public WarehouseLocation? ToLocation { get; set; }

    public StockMovementType MovementType { get; set; }
    public decimal Quantity { get; set; }

    public string? Reference { get; set; }
    public string? SourceDocumentType { get; set; }
    public Guid? SourceDocumentId { get; set; }

    public DateTime MovementDate { get; set; }
    public string? Notes { get; set; }
}

public enum StockMovementType
{
    Receipt = 0,
    Issue = 1,
    Transfer = 2,
    Adjustment = 3,
    Return = 4,
    Scrap = 5,
    SalesOrder = 6,
    PurchaseOrder = 7,
    Production = 8
}
