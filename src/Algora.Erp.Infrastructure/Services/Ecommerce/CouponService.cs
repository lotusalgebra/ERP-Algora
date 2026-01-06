using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Application.Common.Interfaces.Ecommerce;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Infrastructure.Services.Ecommerce;

/// <summary>
/// Service for managing coupons and discounts
/// </summary>
public class CouponService : ICouponService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private static readonly Random _random = new();

    public CouponService(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<Coupon> CreateCouponAsync(CouponDto dto, CancellationToken cancellationToken = default)
    {
        // Check if code already exists
        var exists = await _context.Coupons.AnyAsync(c => c.Code == dto.Code, cancellationToken);
        if (exists)
            throw new InvalidOperationException($"Coupon code '{dto.Code}' already exists.");

        var coupon = new Coupon
        {
            Code = dto.Code.ToUpper(),
            Description = dto.Description,
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            MinOrderAmount = dto.MinOrderAmount,
            StartsAt = dto.StartsAt,
            ExpiresAt = dto.ExpiresAt,
            UsageLimit = dto.UsageLimit,
            UsageLimitPerCustomer = dto.UsageLimitPerCustomer,
            IsActive = dto.IsActive,
            FirstOrderOnly = dto.FirstOrderOnly,
            FreeShipping = dto.FreeShipping,
            ApplicableProductIds = dto.ApplicableProductIds != null ? string.Join(",", dto.ApplicableProductIds) : null,
            ApplicableCategoryIds = dto.ApplicableCategoryIds != null ? string.Join(",", dto.ApplicableCategoryIds) : null,
            ExcludedProductIds = dto.ExcludedProductIds != null ? string.Join(",", dto.ExcludedProductIds) : null
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);

        return coupon;
    }

    public async Task<Coupon?> GetCouponByIdAsync(Guid couponId, CancellationToken cancellationToken = default)
    {
        return await _context.Coupons.FindAsync(new object[] { couponId }, cancellationToken);
    }

    public async Task<Coupon?> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code.ToUpper(), cancellationToken);
    }

    public async Task<CouponListResult> GetCouponsAsync(CouponListRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Coupons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.Code.ToLower().Contains(term) ||
                (c.Description != null && c.Description.ToLower().Contains(term)));
        }

        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        if (request.IsExpired.HasValue)
        {
            var now = _dateTime.UtcNow;
            if (request.IsExpired.Value)
                query = query.Where(c => c.ExpiresAt.HasValue && c.ExpiresAt < now);
            else
                query = query.Where(c => !c.ExpiresAt.HasValue || c.ExpiresAt >= now);
        }

        if (request.DiscountType.HasValue)
            query = query.Where(c => c.DiscountType == request.DiscountType.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy.ToLower() switch
        {
            "code" => request.SortDescending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "discountvalue" => request.SortDescending ? query.OrderByDescending(c => c.DiscountValue) : query.OrderBy(c => c.DiscountValue),
            "usagecount" => request.SortDescending ? query.OrderByDescending(c => c.TimesUsed) : query.OrderBy(c => c.TimesUsed),
            "enddate" => request.SortDescending ? query.OrderByDescending(c => c.ExpiresAt) : query.OrderBy(c => c.ExpiresAt),
            _ => request.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
        };

        var now2 = _dateTime.UtcNow;
        var coupons = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CouponListItem
            {
                Id = c.Id,
                Code = c.Code,
                Description = c.Description,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                MinOrderAmount = c.MinOrderAmount,
                StartsAt = c.StartsAt,
                ExpiresAt = c.ExpiresAt,
                UsageLimit = c.UsageLimit,
                TimesUsed = c.TimesUsed,
                IsActive = c.IsActive,
                IsExpired = c.ExpiresAt.HasValue && c.ExpiresAt < now2,
                TotalDiscountGiven = c.TotalDiscountGiven
            })
            .ToListAsync(cancellationToken);

        return new CouponListResult
        {
            Coupons = coupons,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<Coupon> UpdateCouponAsync(Guid couponId, CouponDto dto, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FindAsync(new object[] { couponId }, cancellationToken)
            ?? throw new InvalidOperationException($"Coupon with ID {couponId} not found.");

        // Check if code is being changed to an existing one
        if (coupon.Code != dto.Code.ToUpper())
        {
            var exists = await _context.Coupons.AnyAsync(c => c.Code == dto.Code.ToUpper() && c.Id != couponId, cancellationToken);
            if (exists)
                throw new InvalidOperationException($"Coupon code '{dto.Code}' already exists.");
        }

        coupon.Code = dto.Code.ToUpper();
        coupon.Description = dto.Description;
        coupon.DiscountType = dto.DiscountType;
        coupon.DiscountValue = dto.DiscountValue;
        coupon.MaxDiscountAmount = dto.MaxDiscountAmount;
        coupon.MinOrderAmount = dto.MinOrderAmount;
        coupon.StartsAt = dto.StartsAt;
        coupon.ExpiresAt = dto.ExpiresAt;
        coupon.UsageLimit = dto.UsageLimit;
        coupon.UsageLimitPerCustomer = dto.UsageLimitPerCustomer;
        coupon.IsActive = dto.IsActive;
        coupon.FirstOrderOnly = dto.FirstOrderOnly;
        coupon.FreeShipping = dto.FreeShipping;
        coupon.ApplicableProductIds = dto.ApplicableProductIds != null ? string.Join(",", dto.ApplicableProductIds) : null;
        coupon.ApplicableCategoryIds = dto.ApplicableCategoryIds != null ? string.Join(",", dto.ApplicableCategoryIds) : null;
        coupon.ExcludedProductIds = dto.ExcludedProductIds != null ? string.Join(",", dto.ExcludedProductIds) : null;

        await _context.SaveChangesAsync(cancellationToken);
        return coupon;
    }

    public async Task DeleteCouponAsync(Guid couponId, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FindAsync(new object[] { couponId }, cancellationToken)
            ?? throw new InvalidOperationException($"Coupon with ID {couponId} not found.");

        coupon.IsDeleted = true;
        coupon.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<CouponValidationResult> ValidateCouponAsync(string code, Guid? customerId, decimal cartTotal, CancellationToken cancellationToken = default)
    {
        var coupon = await GetCouponByCodeAsync(code, cancellationToken);

        if (coupon == null)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid coupon code."
            };
        }

        if (!coupon.IsActive)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                ErrorMessage = "This coupon is no longer active."
            };
        }

        var now = _dateTime.UtcNow;

        if (coupon.StartsAt.HasValue && coupon.StartsAt > now)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                ErrorMessage = "This coupon is not yet active."
            };
        }

        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < now)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                ErrorMessage = "This coupon has expired."
            };
        }

        if (coupon.UsageLimit.HasValue && coupon.TimesUsed >= coupon.UsageLimit)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                ErrorMessage = "This coupon has reached its usage limit."
            };
        }

        if (coupon.MinOrderAmount.HasValue && cartTotal < coupon.MinOrderAmount)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Minimum order amount of {coupon.MinOrderAmount:C2} required."
            };
        }

        // Check customer-specific limits
        if (customerId.HasValue && coupon.UsageLimitPerCustomer.HasValue)
        {
            var customerUsage = await _context.WebOrders
                .CountAsync(o => o.CustomerId == customerId && o.CouponCode == code, cancellationToken);

            if (customerUsage >= coupon.UsageLimitPerCustomer)
            {
                return new CouponValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "You have already used this coupon the maximum number of times."
                };
            }
        }

        // Check first order only
        if (coupon.FirstOrderOnly && customerId.HasValue)
        {
            var hasOrders = await _context.WebOrders.AnyAsync(o => o.CustomerId == customerId, cancellationToken);
            if (hasOrders)
            {
                return new CouponValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "This coupon is only valid for first-time orders."
                };
            }
        }

        var discountAmount = await CalculateDiscountAsync(code, cartTotal, cancellationToken);

        var discountDescription = coupon.DiscountType == DiscountType.Percentage
            ? $"{coupon.DiscountValue}% off"
            : $"{coupon.DiscountValue:C2} off";

        return new CouponValidationResult
        {
            IsValid = true,
            DiscountAmount = discountAmount,
            DiscountDescription = discountDescription,
            FreeShipping = coupon.FreeShipping
        };
    }

    public async Task<decimal> CalculateDiscountAsync(string code, decimal cartTotal, CancellationToken cancellationToken = default)
    {
        var coupon = await GetCouponByCodeAsync(code, cancellationToken);
        if (coupon == null)
            return 0;

        decimal discount;

        if (coupon.DiscountType == DiscountType.Percentage)
        {
            discount = Math.Round(cartTotal * (coupon.DiscountValue / 100), 2);
        }
        else
        {
            discount = coupon.DiscountValue;
        }

        // Apply max discount cap
        if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
            discount = coupon.MaxDiscountAmount.Value;

        // Don't exceed cart total
        if (discount > cartTotal)
            discount = cartTotal;

        return discount;
    }

    public async Task RecordUsageAsync(Guid couponId, Guid orderId, Guid? customerId, decimal discountAmount, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FindAsync(new object[] { couponId }, cancellationToken)
            ?? throw new InvalidOperationException($"Coupon with ID {couponId} not found.");

        coupon.TimesUsed++;
        coupon.TotalDiscountGiven += discountAmount;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<CouponStatistics> GetCouponStatisticsAsync(Guid couponId, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FindAsync(new object[] { couponId }, cancellationToken)
            ?? throw new InvalidOperationException($"Coupon with ID {couponId} not found.");

        var orders = await _context.WebOrders
            .Where(o => o.CouponCode == coupon.Code)
            .ToListAsync(cancellationToken);

        var uniqueCustomers = orders.Where(o => o.CustomerId.HasValue).Select(o => o.CustomerId).Distinct().Count();
        var totalOrderValue = orders.Sum(o => o.Total);

        var dailyUsage = orders
            .GroupBy(o => o.OrderDate.Date)
            .OrderBy(g => g.Key)
            .TakeLast(30)
            .Select(g => new DailyCouponUsage
            {
                Date = g.Key,
                TimesUsed = g.Count(),
                DiscountAmount = g.Sum(o => o.DiscountAmount)
            })
            .ToList();

        return new CouponStatistics
        {
            CouponId = couponId,
            Code = coupon.Code,
            TotalUsage = coupon.TimesUsed,
            UniqueCustomers = uniqueCustomers,
            TotalDiscountGiven = coupon.TotalDiscountGiven,
            TotalOrderValue = totalOrderValue,
            AverageOrderValue = orders.Count > 0 ? totalOrderValue / orders.Count : 0,
            DailyUsage = dailyUsage
        };
    }

    public async Task<string> GenerateCouponCodeAsync(int length = 8, CancellationToken cancellationToken = default)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string code;

        do
        {
            code = new string(Enumerable.Range(0, length).Select(_ => chars[_random.Next(chars.Length)]).ToArray());
        }
        while (await _context.Coupons.AnyAsync(c => c.Code == code, cancellationToken));

        return code;
    }

    public async Task<List<Coupon>> BulkGenerateCouponsAsync(BulkCouponRequest request, CancellationToken cancellationToken = default)
    {
        var coupons = new List<Coupon>();

        for (int i = 0; i < request.Count; i++)
        {
            var code = await GenerateCouponCodeAsync(request.CodeLength, cancellationToken);
            if (!string.IsNullOrEmpty(request.Prefix))
                code = $"{request.Prefix}{code}";

            var coupon = new Coupon
            {
                Code = code,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MaxDiscountAmount = request.MaxDiscountAmount,
                MinOrderAmount = request.MinOrderAmount,
                StartsAt = request.StartsAt,
                ExpiresAt = request.ExpiresAt,
                UsageLimit = request.UsageLimitPerCoupon,
                UsageLimitPerCustomer = 1,
                FirstOrderOnly = request.FirstOrderOnly,
                IsActive = true
            };

            _context.Coupons.Add(coupon);
            coupons.Add(coupon);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return coupons;
    }
}
