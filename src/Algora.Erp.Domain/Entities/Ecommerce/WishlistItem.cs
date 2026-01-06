using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Ecommerce;

/// <summary>
/// Customer wishlist item
/// </summary>
public class WishlistItem : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public WebCustomer? Customer { get; set; }
    public EcommerceProduct? Product { get; set; }
    public ProductVariant? Variant { get; set; }
}
