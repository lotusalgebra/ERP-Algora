using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Settings;

/// <summary>
/// Stores all configurable tenant-level settings
/// </summary>
public class TenantSettings : AuditableEntity
{
    // Company Information
    public string CompanyName { get; set; } = "My Company";
    public string? CompanyLogo { get; set; }
    public string? CompanyTagline { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? CompanyEmail { get; set; }
    public string? CompanyPhone { get; set; }

    // Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "India";
    public string CountryCode { get; set; } = "IN";

    // Regional Settings
    public string Currency { get; set; } = "INR";
    public string CurrencySymbol { get; set; } = "₹";
    public string CurrencyName { get; set; } = "Indian Rupee";
    public int CurrencyDecimalPlaces { get; set; } = 2;
    public string Language { get; set; } = "en-IN";
    public string TimeZone { get; set; } = "Asia/Kolkata";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "HH:mm";
    public string DateTimeFormat { get; set; } = "dd/MM/yyyy HH:mm";

    // Tax Settings
    public string? TaxId { get; set; } // GSTIN for India
    public string TaxIdLabel { get; set; } = "GSTIN";
    public string? PanNumber { get; set; }
    public bool IsGstRegistered { get; set; } = true;

    // Invoice Settings
    public string InvoicePrefix { get; set; } = "INV";
    public string QuotationPrefix { get; set; } = "QT";
    public string SalesOrderPrefix { get; set; } = "SO";
    public string PurchaseOrderPrefix { get; set; } = "PO";
    public string DeliveryChallanPrefix { get; set; } = "DC";
    public string GoodsReceiptPrefix { get; set; } = "GRN";
    public int DefaultPaymentTermDays { get; set; } = 30;
    public string DefaultPaymentTerms { get; set; } = "Net 30";

    // Invoice PDF Settings
    public string? InvoiceHeaderText { get; set; }
    public string? InvoiceFooterText { get; set; } = "Thank you for your business!";
    public string? InvoiceTermsText { get; set; }
    public bool ShowGstBreakdown { get; set; } = true;
    public bool ShowHsnCode { get; set; } = true;

    // Email Settings (SMTP)
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public bool SmtpEnableSsl { get; set; } = true;
    public string? EmailFromAddress { get; set; }
    public string? EmailFromName { get; set; }

    // Security Settings
    public int PasswordMinLength { get; set; } = 8;
    public bool PasswordRequireUppercase { get; set; } = true;
    public bool PasswordRequireLowercase { get; set; } = true;
    public bool PasswordRequireDigit { get; set; } = true;
    public bool PasswordRequireSpecialChar { get; set; } = true;
    public int SessionTimeoutMinutes { get; set; } = 30;
    public int MaxLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
    public bool EnableTwoFactor { get; set; } = false;

    // Backup Settings
    public bool AutoBackupEnabled { get; set; } = true;
    public string AutoBackupTime { get; set; } = "02:00";
    public int BackupRetentionDays { get; set; } = 30;

    // Feature Flags
    public bool EnableEcommerce { get; set; } = false;
    public bool EnableManufacturing { get; set; } = false;
    public bool EnableProjects { get; set; } = false;
    public bool EnablePayroll { get; set; } = false;
}
