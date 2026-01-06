using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Application.Common.Interfaces.Ecommerce;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Infrastructure.Services.Ecommerce;

/// <summary>
/// Service for managing eCommerce store settings
/// </summary>
public class StoreService : IStoreService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public StoreService(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<Store?> GetStoreAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Stores.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Store> UpdateStoreAsync(UpdateStoreRequest request, CancellationToken cancellationToken = default)
    {
        var store = await _context.Stores.FirstOrDefaultAsync(cancellationToken);

        if (store == null)
        {
            store = new Store();
            _context.Stores.Add(store);
        }

        store.Name = request.Name;
        store.Tagline = request.Tagline;
        store.LogoUrl = request.LogoUrl;
        store.FaviconUrl = request.FaviconUrl;
        store.Currency = request.Currency;
        store.TaxRate = request.TaxRate;
        store.EnableTax = request.TaxRate > 0;
        store.Email = request.ContactEmail;
        store.Phone = request.ContactPhone;
        store.Address = request.Address;
        store.FacebookUrl = request.FacebookUrl;
        store.InstagramUrl = request.InstagramUrl;
        store.TwitterUrl = request.TwitterUrl;
        store.MetaTitle = request.MetaTitle;
        store.MetaDescription = request.MetaDescription;

        await _context.SaveChangesAsync(cancellationToken);
        return store;
    }

    public async Task<List<ShippingMethod>> GetShippingMethodsAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.ShippingMethods.AsQueryable();

        if (activeOnly)
            query = query.Where(s => s.IsActive);

        return await query.OrderBy(s => s.SortOrder).ToListAsync(cancellationToken);
    }

    public async Task<ShippingMethod> SaveShippingMethodAsync(ShippingMethodDto dto, CancellationToken cancellationToken = default)
    {
        ShippingMethod method;

        if (dto.Id.HasValue)
        {
            method = await _context.ShippingMethods.FindAsync(new object[] { dto.Id.Value }, cancellationToken)
                ?? throw new InvalidOperationException($"Shipping method with ID {dto.Id} not found.");
        }
        else
        {
            method = new ShippingMethod();
            _context.ShippingMethods.Add(method);
        }

        method.Name = dto.Name;
        method.Description = dto.Description;
        method.Carrier = dto.Carrier;
        method.RateType = dto.RateType;
        method.Rate = dto.Rate;
        method.FreeShippingThreshold = dto.FreeShippingThreshold;
        method.RatePerKg = dto.RatePerKg;
        method.MinDeliveryDays = dto.EstimatedDaysMin;
        method.MaxDeliveryDays = dto.EstimatedDaysMax;
        method.IsActive = dto.IsActive;
        method.SortOrder = dto.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);
        return method;
    }

    public async Task DeleteShippingMethodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var method = await _context.ShippingMethods.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new InvalidOperationException($"Shipping method with ID {id} not found.");

        _context.ShippingMethods.Remove(method);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<WebPaymentMethod>> GetPaymentMethodsAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.WebPaymentMethods.AsQueryable();

        if (activeOnly)
            query = query.Where(p => p.IsActive);

        return await query.OrderBy(p => p.SortOrder).ToListAsync(cancellationToken);
    }

    public async Task<WebPaymentMethod> SavePaymentMethodAsync(PaymentMethodDto dto, CancellationToken cancellationToken = default)
    {
        WebPaymentMethod method;

        if (dto.Id.HasValue)
        {
            method = await _context.WebPaymentMethods.FindAsync(new object[] { dto.Id.Value }, cancellationToken)
                ?? throw new InvalidOperationException($"Payment method with ID {dto.Id} not found.");
        }
        else
        {
            method = new WebPaymentMethod();
            _context.WebPaymentMethods.Add(method);
        }

        method.Name = dto.Name;
        method.Code = dto.Code;
        method.Gateway = dto.Gateway;
        method.ApiKey = dto.GatewayPublicKey;
        method.SecretKey = dto.GatewaySecretKey;
        method.IsSandbox = dto.IsSandbox;
        method.Instructions = dto.Instructions;
        method.IsActive = dto.IsActive;
        method.SortOrder = dto.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);
        return method;
    }

    public async Task DeletePaymentMethodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var method = await _context.WebPaymentMethods.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new InvalidOperationException($"Payment method with ID {id} not found.");

        _context.WebPaymentMethods.Remove(method);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Banner>> GetBannersAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Banners.AsQueryable();

        if (activeOnly)
        {
            var now = _dateTime.UtcNow;
            query = query.Where(b => b.IsActive &&
                (b.StartsAt == null || b.StartsAt <= now) &&
                (b.EndsAt == null || b.EndsAt >= now));
        }

        return await query.OrderBy(b => b.SortOrder).ToListAsync(cancellationToken);
    }

    public async Task<Banner> SaveBannerAsync(BannerDto dto, CancellationToken cancellationToken = default)
    {
        Banner banner;

        if (dto.Id.HasValue)
        {
            banner = await _context.Banners.FindAsync(new object[] { dto.Id.Value }, cancellationToken)
                ?? throw new InvalidOperationException($"Banner with ID {dto.Id} not found.");
        }
        else
        {
            banner = new Banner();
            _context.Banners.Add(banner);
        }

        banner.Title = dto.Title;
        banner.Subtitle = dto.Subtitle;
        banner.ImageUrl = dto.ImageUrl;
        banner.MobileImageUrl = dto.MobileImageUrl;
        banner.LinkUrl = dto.LinkUrl;
        banner.ButtonText = dto.ButtonText;
        banner.Position = dto.Position;
        banner.StartsAt = dto.StartsAt;
        banner.EndsAt = dto.EndsAt;
        banner.IsActive = dto.IsActive;
        banner.SortOrder = dto.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);
        return banner;
    }

    public async Task DeleteBannerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var banner = await _context.Banners.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new InvalidOperationException($"Banner with ID {id} not found.");

        _context.Banners.Remove(banner);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
