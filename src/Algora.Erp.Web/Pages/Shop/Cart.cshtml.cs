using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Shop;

public class CartModel : PageModel
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public CartModel(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetValidateCouponAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new JsonResult(new { valid = false, message = "Please enter a coupon code" });
        }

        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper() && c.IsActive);

        if (coupon == null)
        {
            return new JsonResult(new { valid = false, message = "Invalid coupon code" });
        }

        var now = _dateTime.UtcNow;

        // Check if coupon has started
        if (coupon.StartsAt.HasValue && coupon.StartsAt > now)
        {
            return new JsonResult(new { valid = false, message = "This coupon is not yet active" });
        }

        // Check if coupon has expired
        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < now)
        {
            return new JsonResult(new { valid = false, message = "This coupon has expired" });
        }

        // Check usage limit
        if (coupon.UsageLimit.HasValue && coupon.TimesUsed >= coupon.UsageLimit)
        {
            return new JsonResult(new { valid = false, message = "This coupon has reached its usage limit" });
        }

        string message;
        decimal discount;

        switch (coupon.DiscountType)
        {
            case DiscountType.Percentage:
                discount = coupon.DiscountValue;
                message = $"{coupon.DiscountValue}% off your order";
                break;
            case DiscountType.FixedAmount:
                discount = coupon.DiscountValue;
                message = $"${coupon.DiscountValue:F2} off your order";
                break;
            default:
                discount = coupon.DiscountValue;
                message = "Discount applied";
                break;
        }

        if (coupon.FreeShipping)
        {
            message += " + Free Shipping";
        }

        return new JsonResult(new
        {
            valid = true,
            discount,
            discountType = coupon.DiscountType.ToString(),
            freeShipping = coupon.FreeShipping,
            minOrderAmount = coupon.MinOrderAmount,
            maxDiscountAmount = coupon.MaxDiscountAmount,
            message
        });
    }
}
