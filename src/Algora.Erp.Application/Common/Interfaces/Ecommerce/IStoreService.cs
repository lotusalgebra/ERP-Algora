using Algora.Erp.Domain.Entities.Ecommerce;

namespace Algora.Erp.Application.Common.Interfaces.Ecommerce;

/// <summary>
/// Service for managing eCommerce store settings
/// </summary>
public interface IStoreService
{
    /// <summary>
    /// Gets the store settings
    /// </summary>
    Task<Store?> GetStoreAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates store settings
    /// </summary>
    Task<Store> UpdateStoreAsync(UpdateStoreRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all shipping methods
    /// </summary>
    Task<List<ShippingMethod>> GetShippingMethodsAsync(bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a shipping method
    /// </summary>
    Task<ShippingMethod> SaveShippingMethodAsync(ShippingMethodDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a shipping method
    /// </summary>
    Task DeleteShippingMethodAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all payment methods
    /// </summary>
    Task<List<WebPaymentMethod>> GetPaymentMethodsAsync(bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a payment method
    /// </summary>
    Task<WebPaymentMethod> SavePaymentMethodAsync(PaymentMethodDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a payment method
    /// </summary>
    Task DeletePaymentMethodAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all banners
    /// </summary>
    Task<List<Banner>> GetBannersAsync(bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a banner
    /// </summary>
    Task<Banner> SaveBannerAsync(BannerDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a banner
    /// </summary>
    Task DeleteBannerAsync(Guid id, CancellationToken cancellationToken = default);
}

public class UpdateStoreRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Tagline { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal TaxRate { get; set; }
    public bool TaxIncluded { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}

public class ShippingMethodDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Carrier { get; set; }
    public ShippingRateType RateType { get; set; } = ShippingRateType.FlatRate;
    public decimal Rate { get; set; }
    public decimal? FreeShippingThreshold { get; set; }
    public decimal? RatePerKg { get; set; }
    public int? EstimatedDaysMin { get; set; }
    public int? EstimatedDaysMax { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class PaymentMethodDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public PaymentGateway Gateway { get; set; } = PaymentGateway.Manual;
    public string? GatewayPublicKey { get; set; }
    public string? GatewaySecretKey { get; set; }
    public bool IsSandbox { get; set; }
    public string? Instructions { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class BannerDto
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? MobileImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string? ButtonText { get; set; }
    public BannerPosition Position { get; set; } = BannerPosition.HomeSlider;
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
