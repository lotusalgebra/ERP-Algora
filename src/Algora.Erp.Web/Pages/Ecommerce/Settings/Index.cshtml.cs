using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Ecommerce.Settings;

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

    public Store? Settings { get; set; }

    public async Task OnGetAsync()
    {
        Settings = await _context.Stores.FirstOrDefaultAsync();
    }

    public async Task<IActionResult> OnPostGeneralAsync(string name, string? email, string? phone,
        string? address, string currency, bool enableTax, decimal taxRate)
    {
        var settings = await GetOrCreateSettings();

        settings.Name = name;
        settings.Email = email;
        settings.Phone = phone;
        settings.Address = address;
        settings.Currency = currency;
        settings.EnableTax = enableTax;
        settings.TaxRate = taxRate;
        settings.ModifiedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync();
        return new OkResult();
    }

    public async Task<IActionResult> OnPostCheckoutAsync(bool guestCheckout, int minOrderAmount, int freeShippingThreshold)
    {
        var settings = await GetOrCreateSettings();

        settings.EnableGuestCheckout = guestCheckout;
        settings.MinOrderAmount = minOrderAmount;
        settings.FreeShippingThreshold = freeShippingThreshold;
        settings.ModifiedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync();
        return new OkResult();
    }

    public async Task<IActionResult> OnPostFeaturesAsync(bool enableReviews, bool enableWishlist)
    {
        var settings = await GetOrCreateSettings();

        settings.EnableReviews = enableReviews;
        settings.EnableWishlist = enableWishlist;
        settings.ModifiedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync();
        return new OkResult();
    }

    public async Task<IActionResult> OnPostStatusAsync(bool storeOpen)
    {
        var settings = await GetOrCreateSettings();

        settings.IsActive = storeOpen;
        settings.ModifiedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync();
        return new OkResult();
    }

    public async Task<IActionResult> OnGetShippingListAsync()
    {
        var methods = await _context.ShippingMethods
            .OrderBy(m => m.SortOrder)
            .ToListAsync();

        return Partial("_ShippingMethodsList", methods);
    }

    public IActionResult OnGetShippingForm(Guid? id = null)
    {
        return Partial("_ShippingMethodForm", new ShippingMethod { IsActive = true });
    }

    public async Task<IActionResult> OnPostShippingAsync(Guid? id, string name, string? description,
        string carrier, decimal rate, decimal? freeShippingThreshold, int? minDeliveryDays, int? maxDeliveryDays,
        bool isActive, int sortOrder)
    {
        ShippingMethod method;

        if (id.HasValue && id != Guid.Empty)
        {
            method = await _context.ShippingMethods.FindAsync(id.Value);
            if (method == null) return NotFound();
        }
        else
        {
            method = new ShippingMethod { Id = Guid.NewGuid() };
            _context.ShippingMethods.Add(method);
        }

        method.Name = name;
        method.Description = description;
        method.Carrier = carrier;
        method.Rate = rate;
        method.FreeShippingThreshold = freeShippingThreshold;
        method.MinDeliveryDays = minDeliveryDays;
        method.MaxDeliveryDays = maxDeliveryDays;
        method.IsActive = isActive;
        method.SortOrder = sortOrder;

        await _context.SaveChangesAsync();
        return new OkResult();
    }

    public async Task<IActionResult> OnDeleteShippingAsync(Guid id)
    {
        var method = await _context.ShippingMethods.FindAsync(id);
        if (method != null)
        {
            _context.ShippingMethods.Remove(method);
            await _context.SaveChangesAsync();
        }
        return new OkResult();
    }

    public async Task<IActionResult> OnGetPaymentListAsync()
    {
        var methods = await _context.WebPaymentMethods
            .OrderBy(m => m.SortOrder)
            .ToListAsync();

        return Partial("_PaymentMethodsList", methods);
    }

    public IActionResult OnGetPaymentForm(Guid? id = null)
    {
        return Partial("_PaymentMethodForm", new WebPaymentMethod { IsActive = true });
    }

    public async Task<IActionResult> OnPostPaymentAsync(Guid? id, string name, string code, string? description,
        string gateway, decimal? transactionFeePercent, decimal? transactionFeeFixed,
        bool isActive, int sortOrder)
    {
        WebPaymentMethod method;

        if (id.HasValue && id != Guid.Empty)
        {
            method = await _context.WebPaymentMethods.FindAsync(id.Value);
            if (method == null) return NotFound();
        }
        else
        {
            method = new WebPaymentMethod { Id = Guid.NewGuid() };
            _context.WebPaymentMethods.Add(method);
        }

        method.Name = name;
        method.Code = code;
        method.Description = description;
        method.Gateway = Enum.Parse<PaymentGateway>(gateway);
        method.TransactionFeePercent = transactionFeePercent;
        method.TransactionFeeFixed = transactionFeeFixed;
        method.IsActive = isActive;
        method.SortOrder = sortOrder;

        await _context.SaveChangesAsync();
        return new OkResult();
    }

    public async Task<IActionResult> OnDeletePaymentAsync(Guid id)
    {
        var method = await _context.WebPaymentMethods.FindAsync(id);
        if (method != null)
        {
            _context.WebPaymentMethods.Remove(method);
            await _context.SaveChangesAsync();
        }
        return new OkResult();
    }

    private async Task<Store> GetOrCreateSettings()
    {
        var settings = await _context.Stores.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new Store
            {
                Id = Guid.NewGuid(),
                Name = "My Store",
                Currency = "USD",
                CurrencySymbol = "$",
                IsActive = true,
                CreatedAt = _dateTime.UtcNow
            };
            _context.Stores.Add(settings);
        }
        return settings;
    }
}
