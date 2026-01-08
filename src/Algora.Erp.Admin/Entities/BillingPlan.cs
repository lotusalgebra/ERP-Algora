namespace Algora.Erp.Admin.Entities;

/// <summary>
/// ERP billing plans for customers
/// </summary>
public class BillingPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty; // free, basic, professional, enterprise
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Pricing
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public string Currency { get; set; } = "INR";

    // Discount
    public decimal AnnualDiscountPercent { get; set; } // e.g., 20% off for annual

    // Limits
    public int MaxUsers { get; set; }
    public int MaxWarehouses { get; set; }
    public int MaxProducts { get; set; }
    public int MaxTransactionsPerMonth { get; set; }
    public long StorageLimitMB { get; set; }

    // Features (JSON array of feature codes)
    public string Features { get; set; } = "[]";

    // Modules included (JSON array)
    public string IncludedModules { get; set; } = "[]";

    // Display
    public int DisplayOrder { get; set; }
    public bool IsPopular { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = true; // Show on pricing page

    // Trial
    public int TrialDays { get; set; } = 14;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    public ICollection<TenantSubscription> Subscriptions { get; set; } = new List<TenantSubscription>();
}

/// <summary>
/// Available ERP modules
/// </summary>
public static class ErpModules
{
    public const string Finance = "finance";
    public const string Inventory = "inventory";
    public const string Sales = "sales";
    public const string Procurement = "procurement";
    public const string Manufacturing = "manufacturing";
    public const string HR = "hr";
    public const string Payroll = "payroll";
    public const string Projects = "projects";
    public const string CRM = "crm";
    public const string Ecommerce = "ecommerce";
    public const string Reports = "reports";
    public const string API = "api";

    public static readonly string[] All = new[]
    {
        Finance, Inventory, Sales, Procurement, Manufacturing,
        HR, Payroll, Projects, CRM, Ecommerce, Reports, API
    };
}

/// <summary>
/// Plan features
/// </summary>
public static class PlanFeatures
{
    public const string MultiCurrency = "multi_currency";
    public const string MultiWarehouse = "multi_warehouse";
    public const string CustomReports = "custom_reports";
    public const string EmailSupport = "email_support";
    public const string PhoneSupport = "phone_support";
    public const string PrioritySupport = "priority_support";
    public const string DedicatedManager = "dedicated_manager";
    public const string ApiAccess = "api_access";
    public const string Webhooks = "webhooks";
    public const string CustomBranding = "custom_branding";
    public const string AuditLogs = "audit_logs";
    public const string AdvancedSecurity = "advanced_security";
    public const string DataExport = "data_export";
    public const string Integrations = "integrations";
    public const string Training = "training";
    public const string Onboarding = "onboarding";
}
