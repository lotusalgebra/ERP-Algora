using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Ecommerce;

/// <summary>
/// Store configuration for eCommerce
/// </summary>
public class Store : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Tagline { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }

    // Contact
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }

    // Social
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? InstagramUrl { get; set; }

    // Settings
    public string Currency { get; set; } = "USD";
    public string CurrencySymbol { get; set; } = "$";
    public bool EnableTax { get; set; }
    public decimal TaxRate { get; set; }
    public bool EnableReviews { get; set; } = true;
    public bool EnableWishlist { get; set; } = true;
    public bool EnableGuestCheckout { get; set; } = true;
    public int MinOrderAmount { get; set; }
    public int FreeShippingThreshold { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }

    public bool IsActive { get; set; } = true;
}
