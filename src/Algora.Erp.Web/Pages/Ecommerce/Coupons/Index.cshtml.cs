using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Ecommerce.Coupons;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public IndexModel(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public int TotalCoupons { get; set; }
    public int ActiveCoupons { get; set; }
    public int TotalUsage { get; set; }
    public decimal TotalDiscountGiven { get; set; }

    public async Task OnGetAsync()
    {
        var now = _dateTime.UtcNow;

        TotalCoupons = await _context.Coupons.CountAsync();
        ActiveCoupons = await _context.Coupons.CountAsync(c =>
            c.IsActive &&
            (c.StartsAt == null || c.StartsAt <= now) &&
            (c.ExpiresAt == null || c.ExpiresAt >= now));
        TotalUsage = await _context.Coupons.SumAsync(c => c.TimesUsed);
        TotalDiscountGiven = await _context.Coupons.SumAsync(c => c.TotalDiscountGiven);
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, int pageNumber = 1, int pageSize = 10)
    {
        var now = _dateTime.UtcNow;
        var query = _context.Coupons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c =>
                c.Code.ToLower().Contains(search) ||
                (c.Description != null && c.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            query = statusFilter switch
            {
                "Active" => query.Where(c => c.IsActive &&
                    (c.StartsAt == null || c.StartsAt <= now) &&
                    (c.ExpiresAt == null || c.ExpiresAt >= now)),
                "Expired" => query.Where(c => c.ExpiresAt != null && c.ExpiresAt < now),
                "Disabled" => query.Where(c => !c.IsActive),
                _ => query
            };
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var coupons = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_CouponsTableRows", new CouponsTableViewModel
        {
            Coupons = coupons,
            Page = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            Now = now
        });
    }

    public IActionResult OnGetCreateForm()
    {
        return Partial("_CouponForm", new CouponFormViewModel { IsEdit = false });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon == null)
            return NotFound();

        return Partial("_CouponForm", new CouponFormViewModel
        {
            IsEdit = true,
            Coupon = coupon
        });
    }

    public async Task<IActionResult> OnPostAsync(Guid? id, string code, string? description,
        string discountType, decimal discountValue, decimal? minOrderAmount, decimal? maxDiscountAmount,
        DateTime? startsAt, DateTime? expiresAt, int? usageLimit, int? usageLimitPerCustomer,
        bool freeShipping, bool isActive = true)
    {
        Coupon coupon;

        if (id.HasValue)
        {
            coupon = await _context.Coupons.FindAsync(id.Value);
            if (coupon == null)
                return NotFound();
        }
        else
        {
            coupon = new Coupon { Id = Guid.NewGuid() };
            _context.Coupons.Add(coupon);
        }

        coupon.Code = code.ToUpper().Trim();
        coupon.Description = description;
        coupon.DiscountType = Enum.Parse<DiscountType>(discountType);
        coupon.DiscountValue = discountValue;
        coupon.MinOrderAmount = minOrderAmount;
        coupon.MaxDiscountAmount = maxDiscountAmount;
        coupon.StartsAt = startsAt;
        coupon.ExpiresAt = expiresAt;
        coupon.UsageLimit = usageLimit;
        coupon.UsageLimitPerCustomer = usageLimitPerCustomer;
        coupon.FreeShipping = freeShipping;
        coupon.IsActive = isActive;

        if (!id.HasValue)
        {
            coupon.CreatedAt = _dateTime.UtcNow;
        }
        coupon.ModifiedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon != null)
        {
            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
        }

        return await OnGetTableAsync(null, null);
    }
}

public class CouponsTableViewModel
{
    public List<Coupon> Coupons { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public DateTime Now { get; set; }
}

public class CouponFormViewModel
{
    public bool IsEdit { get; set; }
    public Coupon? Coupon { get; set; }
}
