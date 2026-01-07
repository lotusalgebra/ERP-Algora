using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Settings;

/// <summary>
/// Tax system types supported by different countries
/// </summary>
public enum TaxSystem
{
    None = 0,
    GST = 1,           // India, Australia, Singapore, Malaysia, etc.
    VAT = 2,           // UK, EU countries
    SalesTax = 3,      // USA (state-level)
    HST = 4,           // Canada (Harmonized Sales Tax)
    GST_PST = 5,       // Canada (GST + Provincial Sales Tax)
    Consumption = 6,   // Japan (Consumption Tax)
    Custom = 99        // Custom tax system
}

/// <summary>
/// Tax calculation method
/// </summary>
public enum TaxCalculationMethod
{
    Exclusive = 0,     // Tax added on top of price
    Inclusive = 1      // Tax included in price
}

/// <summary>
/// Tenant-level tax configuration
/// Each tenant can have different tax settings based on their country
/// </summary>
public class TaxConfiguration : AuditableEntity
{
    public string Name { get; set; } = "Default Tax Configuration";

    // Country settings
    public string CountryCode { get; set; } = "IN";  // ISO 3166-1 alpha-2
    public string CountryName { get; set; } = "India";

    // Tax system
    public TaxSystem TaxSystem { get; set; } = TaxSystem.GST;
    public string TaxSystemName { get; set; } = "GST";  // Display name (e.g., "Goods and Services Tax")

    // Tax ID format and labels
    public string TaxIdLabel { get; set; } = "GSTIN";  // Label for tax ID (GSTIN, VAT Number, EIN, etc.)
    public string? TaxIdFormat { get; set; }  // Regex pattern for validation
    public string? TaxIdExample { get; set; } = "27AABCU9603R1ZM";

    // Regional tax support (for GST India, Sales Tax USA, etc.)
    public bool HasRegionalTax { get; set; } = true;
    public string RegionLabel { get; set; } = "State";  // State, Province, Region, etc.
    public bool HasInterRegionalTax { get; set; } = true;  // Different tax for inter-state (IGST)

    // Tax component labels (for split tax display)
    public string? CentralTaxLabel { get; set; } = "CGST";  // Central GST, Federal Tax, etc.
    public string? RegionalTaxLabel { get; set; } = "SGST";  // State GST, Provincial Tax, etc.
    public string? InterRegionalTaxLabel { get; set; } = "IGST";  // Inter-state GST, etc.
    public string? CombinedTaxLabel { get; set; } = "GST";  // For simple display

    // Product classification
    public string? ProductCodeLabel { get; set; } = "HSN Code";  // HSN, HS Code, Product Code, etc.
    public string? ServiceCodeLabel { get; set; } = "SAC Code";  // SAC, Service Code, etc.

    // Calculation settings
    public TaxCalculationMethod CalculationMethod { get; set; } = TaxCalculationMethod.Exclusive;
    public int DecimalPlaces { get; set; } = 2;
    public bool RoundAtLineLevel { get; set; } = true;

    // Currency
    public string DefaultCurrencyCode { get; set; } = "INR";
    public string DefaultCurrencySymbol { get; set; } = "₹";

    // Status
    public bool IsDefault { get; set; } = true;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<TaxSlab> TaxSlabs { get; set; } = new List<TaxSlab>();
    public ICollection<TaxRegion> TaxRegions { get; set; } = new List<TaxRegion>();
}

/// <summary>
/// Tax rate slabs - generic replacement for GstSlab
/// Works with GST, VAT, Sales Tax, etc.
/// </summary>
public class TaxSlab : AuditableEntity
{
    public Guid TaxConfigurationId { get; set; }
    public TaxConfiguration TaxConfiguration { get; set; } = null!;

    public string Name { get; set; } = string.Empty;  // e.g., "GST 18%", "VAT 20%", "Standard Rate"
    public string? Code { get; set; }  // e.g., "GST18", "VAT20", "STD"
    public string? Description { get; set; }

    // Tax rates
    public decimal Rate { get; set; }  // Total rate (e.g., 18 for 18%)

    // Split rates (for systems like India GST)
    public decimal CentralRate { get; set; }  // CGST rate (e.g., 9%)
    public decimal RegionalRate { get; set; }  // SGST rate (e.g., 9%)
    public decimal InterRegionalRate { get; set; }  // IGST rate (e.g., 18%)

    // Product/Service codes this rate applies to
    public string? ApplicableCodes { get; set; }  // Comma-separated HSN/SAC codes

    // Settings
    public bool IsDefault { get; set; } = false;
    public bool IsZeroRated { get; set; } = false;  // For zero-rated supplies
    public bool IsExempt { get; set; } = false;  // For exempt supplies
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Tax regions (states, provinces, territories)
/// Used for regional tax calculations
/// </summary>
public class TaxRegion : AuditableEntity
{
    public Guid TaxConfigurationId { get; set; }
    public TaxConfiguration TaxConfiguration { get; set; } = null!;

    public string Code { get; set; } = string.Empty;  // State code (e.g., "MH", "CA", "ON")
    public string Name { get; set; } = string.Empty;  // Full name (e.g., "Maharashtra", "California")
    public string? ShortName { get; set; }  // Short form if different from Code

    // Region-specific tax rate override (for US Sales Tax)
    public decimal? RegionalTaxRate { get; set; }  // Override regional rate for this region

    // Additional info
    public bool IsUnionTerritory { get; set; } = false;  // For India UTs
    public bool HasLocalTax { get; set; } = false;  // For city/county level taxes
    public decimal? LocalTaxRate { get; set; }

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Predefined tax configurations for common countries
/// </summary>
public static class TaxConfigurationTemplates
{
    public static TaxConfiguration India => new()
    {
        Name = "India GST",
        CountryCode = "IN",
        CountryName = "India",
        TaxSystem = TaxSystem.GST,
        TaxSystemName = "Goods and Services Tax",
        TaxIdLabel = "GSTIN",
        TaxIdFormat = @"^\d{2}[A-Z]{5}\d{4}[A-Z]{1}[A-Z\d]{1}[Z]{1}[A-Z\d]{1}$",
        TaxIdExample = "27AABCU9603R1ZM",
        HasRegionalTax = true,
        RegionLabel = "State",
        HasInterRegionalTax = true,
        CentralTaxLabel = "CGST",
        RegionalTaxLabel = "SGST",
        InterRegionalTaxLabel = "IGST",
        CombinedTaxLabel = "GST",
        ProductCodeLabel = "HSN Code",
        ServiceCodeLabel = "SAC Code",
        DefaultCurrencyCode = "INR",
        DefaultCurrencySymbol = "₹"
    };

    public static TaxConfiguration UnitedKingdom => new()
    {
        Name = "UK VAT",
        CountryCode = "GB",
        CountryName = "United Kingdom",
        TaxSystem = TaxSystem.VAT,
        TaxSystemName = "Value Added Tax",
        TaxIdLabel = "VAT Number",
        TaxIdFormat = @"^GB\d{9}$|^GB\d{12}$|^GBGD\d{3}$|^GBHA\d{3}$",
        TaxIdExample = "GB123456789",
        HasRegionalTax = false,
        RegionLabel = "Region",
        HasInterRegionalTax = false,
        CentralTaxLabel = null,
        RegionalTaxLabel = null,
        InterRegionalTaxLabel = null,
        CombinedTaxLabel = "VAT",
        ProductCodeLabel = "Commodity Code",
        ServiceCodeLabel = null,
        DefaultCurrencyCode = "GBP",
        DefaultCurrencySymbol = "£"
    };

    public static TaxConfiguration USA => new()
    {
        Name = "US Sales Tax",
        CountryCode = "US",
        CountryName = "United States",
        TaxSystem = TaxSystem.SalesTax,
        TaxSystemName = "Sales Tax",
        TaxIdLabel = "EIN",
        TaxIdFormat = @"^\d{2}-\d{7}$",
        TaxIdExample = "12-3456789",
        HasRegionalTax = true,
        RegionLabel = "State",
        HasInterRegionalTax = false,  // No inter-state tax concept
        CentralTaxLabel = null,
        RegionalTaxLabel = "State Tax",
        InterRegionalTaxLabel = null,
        CombinedTaxLabel = "Sales Tax",
        ProductCodeLabel = "Product Code",
        ServiceCodeLabel = null,
        DefaultCurrencyCode = "USD",
        DefaultCurrencySymbol = "$"
    };

    public static TaxConfiguration Canada => new()
    {
        Name = "Canada GST/HST",
        CountryCode = "CA",
        CountryName = "Canada",
        TaxSystem = TaxSystem.GST_PST,
        TaxSystemName = "GST/HST/PST",
        TaxIdLabel = "GST/HST Number",
        TaxIdFormat = @"^\d{9}RT\d{4}$",
        TaxIdExample = "123456789RT0001",
        HasRegionalTax = true,
        RegionLabel = "Province",
        HasInterRegionalTax = false,
        CentralTaxLabel = "GST",
        RegionalTaxLabel = "PST",
        InterRegionalTaxLabel = "HST",
        CombinedTaxLabel = "Tax",
        ProductCodeLabel = "HS Code",
        ServiceCodeLabel = null,
        DefaultCurrencyCode = "CAD",
        DefaultCurrencySymbol = "$"
    };

    public static TaxConfiguration Australia => new()
    {
        Name = "Australia GST",
        CountryCode = "AU",
        CountryName = "Australia",
        TaxSystem = TaxSystem.GST,
        TaxSystemName = "Goods and Services Tax",
        TaxIdLabel = "ABN",
        TaxIdFormat = @"^\d{11}$",
        TaxIdExample = "12345678901",
        HasRegionalTax = false,
        RegionLabel = "State",
        HasInterRegionalTax = false,
        CentralTaxLabel = null,
        RegionalTaxLabel = null,
        InterRegionalTaxLabel = null,
        CombinedTaxLabel = "GST",
        ProductCodeLabel = "HS Code",
        ServiceCodeLabel = null,
        DefaultCurrencyCode = "AUD",
        DefaultCurrencySymbol = "$"
    };

    public static TaxConfiguration EU => new()
    {
        Name = "EU VAT",
        CountryCode = "EU",
        CountryName = "European Union",
        TaxSystem = TaxSystem.VAT,
        TaxSystemName = "Value Added Tax",
        TaxIdLabel = "VAT ID",
        TaxIdFormat = null,  // Varies by country
        TaxIdExample = "DE123456789",
        HasRegionalTax = false,
        RegionLabel = "Country",
        HasInterRegionalTax = true,  // Cross-border EU transactions
        CentralTaxLabel = null,
        RegionalTaxLabel = null,
        InterRegionalTaxLabel = "EU VAT",
        CombinedTaxLabel = "VAT",
        ProductCodeLabel = "CN Code",
        ServiceCodeLabel = null,
        DefaultCurrencyCode = "EUR",
        DefaultCurrencySymbol = "€"
    };

    public static TaxConfiguration UAE => new()
    {
        Name = "UAE VAT",
        CountryCode = "AE",
        CountryName = "United Arab Emirates",
        TaxSystem = TaxSystem.VAT,
        TaxSystemName = "Value Added Tax",
        TaxIdLabel = "TRN",
        TaxIdFormat = @"^\d{15}$",
        TaxIdExample = "100123456789012",
        HasRegionalTax = false,
        RegionLabel = "Emirate",
        HasInterRegionalTax = false,
        CentralTaxLabel = null,
        RegionalTaxLabel = null,
        InterRegionalTaxLabel = null,
        CombinedTaxLabel = "VAT",
        ProductCodeLabel = "HS Code",
        ServiceCodeLabel = null,
        DefaultCurrencyCode = "AED",
        DefaultCurrencySymbol = "د.إ"
    };

    public static TaxConfiguration Singapore => new()
    {
        Name = "Singapore GST",
        CountryCode = "SG",
        CountryName = "Singapore",
        TaxSystem = TaxSystem.GST,
        TaxSystemName = "Goods and Services Tax",
        TaxIdLabel = "GST Reg No",
        TaxIdFormat = @"^[MF]\d{8}[A-Z]$",
        TaxIdExample = "M12345678A",
        HasRegionalTax = false,
        RegionLabel = "Region",
        HasInterRegionalTax = false,
        CentralTaxLabel = null,
        RegionalTaxLabel = null,
        InterRegionalTaxLabel = null,
        CombinedTaxLabel = "GST",
        ProductCodeLabel = "HS Code",
        ServiceCodeLabel = null,
        DefaultCurrencyCode = "SGD",
        DefaultCurrencySymbol = "$"
    };

    public static TaxConfiguration NoTax => new()
    {
        Name = "No Tax",
        CountryCode = "",
        CountryName = "",
        TaxSystem = TaxSystem.None,
        TaxSystemName = "No Tax",
        TaxIdLabel = "Tax ID",
        HasRegionalTax = false,
        HasInterRegionalTax = false,
        CombinedTaxLabel = "Tax",
        DefaultCurrencyCode = "USD",
        DefaultCurrencySymbol = "$"
    };
}
