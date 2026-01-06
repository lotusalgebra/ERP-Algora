using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Ecommerce;

/// <summary>
/// Discount coupon
/// </summary>
public class Coupon : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }  // Cap for percentage discounts

    public decimal? MinOrderAmount { get; set; }
    public int? MinQuantity { get; set; }

    // Limits
    public int? UsageLimit { get; set; }
    public int? UsageLimitPerCustomer { get; set; }
    public int TimesUsed { get; set; }
    public decimal TotalDiscountGiven { get; set; }

    // Validity
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Restrictions
    public bool FirstOrderOnly { get; set; }
    public string? ApplicableProductIds { get; set; }  // Comma-separated
    public string? ApplicableCategoryIds { get; set; }
    public string? ExcludedProductIds { get; set; }
    public string? CustomerIds { get; set; }  // Specific customers only

    public bool FreeShipping { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum DiscountType
{
    Percentage,
    FixedAmount,
    BuyXGetY
}
