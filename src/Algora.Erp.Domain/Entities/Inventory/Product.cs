using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Inventory;

public class Product : AuditableEntity
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }

    public Guid? CategoryId { get; set; }
    public ProductCategory? Category { get; set; }

    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }

    public ProductType ProductType { get; set; } = ProductType.Goods;
    public string? UnitOfMeasure { get; set; }

    // Pricing
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public string Currency { get; set; } = "USD";

    // Stock
    public decimal MinimumStock { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal MaximumStock { get; set; }
    public bool TrackInventory { get; set; } = true;

    // Dimensions & Weight
    public decimal? Weight { get; set; }
    public string? WeightUnit { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? DimensionUnit { get; set; }

    // Tax
    public bool IsTaxable { get; set; } = true;
    public decimal? TaxRate { get; set; }

    // Status
    public bool IsActive { get; set; } = true;
    public bool IsSellable { get; set; } = true;
    public bool IsPurchasable { get; set; } = true;

    public string? ImageUrl { get; set; }
    public string? Notes { get; set; }

    public ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}

public class ProductCategory : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }
    public ProductCategory? ParentCategory { get; set; }
    public ICollection<ProductCategory> ChildCategories { get; set; } = new List<ProductCategory>();

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public enum ProductType
{
    Goods = 0,
    Service = 1,
    RawMaterial = 2,
    FinishedGoods = 3,
    Consumable = 4,
    Asset = 5
}
