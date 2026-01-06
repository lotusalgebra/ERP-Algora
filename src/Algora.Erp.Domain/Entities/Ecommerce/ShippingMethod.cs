using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Ecommerce;

/// <summary>
/// Shipping method configuration
/// </summary>
public class ShippingMethod : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Carrier { get; set; }  // FedEx, UPS, USPS, etc.

    public ShippingRateType RateType { get; set; } = ShippingRateType.FlatRate;
    public decimal Rate { get; set; }
    public decimal? FreeShippingThreshold { get; set; }

    // Weight-based rates
    public decimal? RatePerKg { get; set; }
    public decimal? MinWeight { get; set; }
    public decimal? MaxWeight { get; set; }

    // Time
    public int? MinDeliveryDays { get; set; }
    public int? MaxDeliveryDays { get; set; }

    // Restrictions
    public string? AllowedCountries { get; set; }  // Comma-separated country codes
    public string? ExcludedCountries { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum ShippingRateType
{
    FlatRate,
    WeightBased,
    PriceBased,
    Free,
    Calculated  // Real-time carrier rates
}
