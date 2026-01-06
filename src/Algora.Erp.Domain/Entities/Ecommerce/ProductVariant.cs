using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Ecommerce;

/// <summary>
/// Product variant (size, color, etc.)
/// </summary>
public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;  // e.g., "Large / Blue"

    // Options
    public string? Option1Name { get; set; }  // e.g., "Size"
    public string? Option1Value { get; set; } // e.g., "Large"
    public string? Option2Name { get; set; }  // e.g., "Color"
    public string? Option2Value { get; set; } // e.g., "Blue"
    public string? Option3Name { get; set; }
    public string? Option3Value { get; set; }

    // Pricing
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }

    // Stock
    public int StockQuantity { get; set; }
    public bool TrackInventory { get; set; } = true;

    // Physical
    public decimal? Weight { get; set; }

    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public EcommerceProduct? Product { get; set; }
}
