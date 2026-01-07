using Algora.Erp.Domain.Entities.Settings;

namespace Algora.Erp.Application.Common.Interfaces;

/// <summary>
/// Service for managing tenant-specific tax configurations
/// </summary>
public interface ITaxConfigurationService
{
    /// <summary>
    /// Get the active tax configuration for the current tenant
    /// </summary>
    Task<TaxConfiguration?> GetCurrentTaxConfigurationAsync();

    /// <summary>
    /// Get tax configuration by ID
    /// </summary>
    Task<TaxConfiguration?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all tax configurations for the current tenant
    /// </summary>
    Task<List<TaxConfiguration>> GetAllAsync();

    /// <summary>
    /// Get all active tax slabs for the current tax configuration
    /// </summary>
    Task<List<TaxSlab>> GetActiveTaxSlabsAsync();

    /// <summary>
    /// Get all active tax regions for the current tax configuration
    /// </summary>
    Task<List<TaxRegion>> GetActiveTaxRegionsAsync();

    /// <summary>
    /// Get tax slab by ID
    /// </summary>
    Task<TaxSlab?> GetTaxSlabByIdAsync(Guid id);

    /// <summary>
    /// Calculate tax for an amount using the specified tax slab
    /// </summary>
    TaxCalculationResult CalculateTax(decimal amount, TaxSlab taxSlab, bool isInterRegional = false);

    /// <summary>
    /// Create a new tax configuration from a country template
    /// </summary>
    Task<TaxConfiguration> CreateFromTemplateAsync(string countryCode);

    /// <summary>
    /// Create or update tax configuration
    /// </summary>
    Task<TaxConfiguration> SaveAsync(TaxConfiguration configuration);

    /// <summary>
    /// Create or update tax slab
    /// </summary>
    Task<TaxSlab> SaveTaxSlabAsync(TaxSlab slab);

    /// <summary>
    /// Create or update tax region
    /// </summary>
    Task<TaxRegion> SaveTaxRegionAsync(TaxRegion region);

    /// <summary>
    /// Delete tax configuration (soft delete)
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Set a tax configuration as the default for the tenant
    /// </summary>
    Task SetAsDefaultAsync(Guid id);
}

/// <summary>
/// Result of tax calculation
/// </summary>
public class TaxCalculationResult
{
    public decimal TaxableAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // Split tax amounts (for systems like India GST)
    public decimal CentralTaxAmount { get; set; }
    public decimal RegionalTaxAmount { get; set; }
    public decimal InterRegionalTaxAmount { get; set; }

    // Rates used
    public decimal TaxRate { get; set; }
    public decimal CentralTaxRate { get; set; }
    public decimal RegionalTaxRate { get; set; }
    public decimal InterRegionalTaxRate { get; set; }

    // Labels for display
    public string? CentralTaxLabel { get; set; }
    public string? RegionalTaxLabel { get; set; }
    public string? InterRegionalTaxLabel { get; set; }
    public string? CombinedTaxLabel { get; set; }

    public bool IsInterRegional { get; set; }
}
