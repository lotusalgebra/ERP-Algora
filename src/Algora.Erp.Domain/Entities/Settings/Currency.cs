using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Settings;

/// <summary>
/// Represents a currency in the system
/// </summary>
public class Currency : AuditableEntity
{
    public string Code { get; set; } = string.Empty; // ISO 4217 code (USD, INR, EUR)
    public string Name { get; set; } = string.Empty; // US Dollar, Indian Rupee
    public string Symbol { get; set; } = string.Empty; // $, ₹, €
    public string SymbolPosition { get; set; } = "before"; // before or after
    public int DecimalPlaces { get; set; } = 2;
    public string DecimalSeparator { get; set; } = ".";
    public string ThousandsSeparator { get; set; } = ",";
    public decimal ExchangeRate { get; set; } = 1.0m; // Rate relative to base currency
    public bool IsBaseCurrency { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Indian state/UT for GST purposes
/// </summary>
public class IndianState : AuditableEntity
{
    public string Code { get; set; } = string.Empty; // 2-digit state code (01, 27, etc.)
    public string Name { get; set; } = string.Empty; // State name
    public string ShortName { get; set; } = string.Empty; // MH, DL, KA, etc.
    public bool IsUnionTerritory { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// GST rate slabs
/// </summary>
public class GstSlab : AuditableEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "GST 18%", "GST 5%"
    public decimal Rate { get; set; } // Total rate (e.g., 18 for 18%)
    public decimal CgstRate { get; set; } // Central GST rate (9 for 18% total)
    public decimal SgstRate { get; set; } // State GST rate (9 for 18% total)
    public decimal IgstRate { get; set; } // Interstate GST rate (18 for 18% total)
    public string? HsnCodes { get; set; } // Comma-separated HSN codes this rate applies to
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Office/Branch location with GST registration
/// </summary>
public class OfficeLocation : AuditableEntity
{
    public string Code { get; set; } = string.Empty; // e.g., "HO", "BR01"
    public string Name { get; set; } = string.Empty; // e.g., "Head Office", "Mumbai Branch"
    public OfficeType Type { get; set; } = OfficeType.Branch;

    // Address
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public Guid? StateId { get; set; }
    public IndianState? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "India";

    // GST Details
    public string? GstNumber { get; set; } // e.g., "27AABCU9603R1ZM"
    public bool IsGstRegistered { get; set; } = true;
    public GstRegistrationType GstRegistrationType { get; set; } = GstRegistrationType.Regular;

    // Default Currency for this location
    public Guid? DefaultCurrencyId { get; set; }
    public Currency? DefaultCurrency { get; set; }

    // Contact
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }

    // Settings
    public bool IsHeadOffice { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

public enum OfficeType
{
    HeadOffice = 0,
    Branch = 1,
    Warehouse = 2,
    Factory = 3,
    RegisteredOffice = 4
}

public enum GstRegistrationType
{
    Regular = 0,
    Composition = 1,
    Unregistered = 2,
    InputServiceDistributor = 3,
    CasualTaxablePerson = 4,
    NonResidentTaxablePerson = 5,
    UNBody = 6,
    Embassy = 7,
    SEZ = 8
}
