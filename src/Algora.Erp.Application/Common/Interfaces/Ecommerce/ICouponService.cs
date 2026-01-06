using Algora.Erp.Domain.Entities.Ecommerce;

namespace Algora.Erp.Application.Common.Interfaces.Ecommerce;

/// <summary>
/// Service for managing coupons and discounts
/// </summary>
public interface ICouponService
{
    /// <summary>
    /// Creates a new coupon
    /// </summary>
    Task<Coupon> CreateCouponAsync(CouponDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a coupon by ID
    /// </summary>
    Task<Coupon?> GetCouponByIdAsync(Guid couponId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a coupon by code
    /// </summary>
    Task<Coupon?> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists coupons with filtering and pagination
    /// </summary>
    Task<CouponListResult> GetCouponsAsync(CouponListRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a coupon
    /// </summary>
    Task<Coupon> UpdateCouponAsync(Guid couponId, CouponDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a coupon
    /// </summary>
    Task DeleteCouponAsync(Guid couponId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a coupon can be applied
    /// </summary>
    Task<CouponValidationResult> ValidateCouponAsync(string code, Guid? customerId, decimal cartTotal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates discount for a coupon
    /// </summary>
    Task<decimal> CalculateDiscountAsync(string code, decimal cartTotal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records coupon usage
    /// </summary>
    Task RecordUsageAsync(Guid couponId, Guid orderId, Guid? customerId, decimal discountAmount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets coupon usage statistics
    /// </summary>
    Task<CouponStatistics> GetCouponStatisticsAsync(Guid couponId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a unique coupon code
    /// </summary>
    Task<string> GenerateCouponCodeAsync(int length = 8, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk generates coupon codes
    /// </summary>
    Task<List<Coupon>> BulkGenerateCouponsAsync(BulkCouponRequest request, CancellationToken cancellationToken = default);
}

public class CouponDto
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? UsageLimit { get; set; }
    public int? UsageLimitPerCustomer { get; set; }
    public bool IsActive { get; set; } = true;
    public bool FirstOrderOnly { get; set; }
    public bool FreeShipping { get; set; }
    public List<Guid>? ApplicableProductIds { get; set; }
    public List<Guid>? ApplicableCategoryIds { get; set; }
    public List<Guid>? ExcludedProductIds { get; set; }
}

public class CouponListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsExpired { get; set; }
    public DiscountType? DiscountType { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class CouponListResult
{
    public List<CouponListItem> Coupons { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class CouponListItem
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? UsageLimit { get; set; }
    public int TimesUsed { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public decimal TotalDiscountGiven { get; set; }
}

public class CouponValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? DiscountDescription { get; set; }
    public bool FreeShipping { get; set; }
}

public class CouponStatistics
{
    public Guid CouponId { get; set; }
    public string Code { get; set; } = string.Empty;
    public int TotalUsage { get; set; }
    public int UniqueCustomers { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public decimal TotalOrderValue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<DailyCouponUsage> DailyUsage { get; set; } = new();
}

public class DailyCouponUsage
{
    public DateTime Date { get; set; }
    public int TimesUsed { get; set; }
    public decimal DiscountAmount { get; set; }
}

public class BulkCouponRequest
{
    public int Count { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public int CodeLength { get; set; } = 8;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? UsageLimitPerCoupon { get; set; } = 1;
    public bool FirstOrderOnly { get; set; }
}
