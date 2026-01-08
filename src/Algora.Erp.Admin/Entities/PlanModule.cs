namespace Algora.Erp.Admin.Entities;

/// <summary>
/// Represents an ERP module with its pricing configuration
/// </summary>
public class PlanModule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Module code (e.g., "finance", "inventory", "sales")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the module
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of module functionality
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon class for display (Bootstrap Icons)
    /// </summary>
    public string Icon { get; set; } = "bi-box";

    /// <summary>
    /// Monthly price for this module
    /// </summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>
    /// Annual price for this module (usually discounted)
    /// </summary>
    public decimal AnnualPrice { get; set; }

    /// <summary>
    /// Currency code (default INR)
    /// </summary>
    public string Currency { get; set; } = "INR";

    /// <summary>
    /// Whether this is a core/base module (always included)
    /// </summary>
    public bool IsCore { get; set; }

    /// <summary>
    /// Whether this module requires other modules (comma-separated codes)
    /// </summary>
    public string? RequiredModules { get; set; }

    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this module is active and available for selection
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Category for grouping (e.g., "Core", "Finance", "Operations", "HR", "Advanced")
    /// </summary>
    public string Category { get; set; } = "Core";

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
}

/// <summary>
/// Links a billing plan to its included modules
/// </summary>
public class BillingPlanModule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PlanId { get; set; }
    public BillingPlan? Plan { get; set; }

    public Guid ModuleId { get; set; }
    public PlanModule? Module { get; set; }

    /// <summary>
    /// Override price for this module in this plan (null = use module default)
    /// </summary>
    public decimal? OverrideMonthlyPrice { get; set; }

    /// <summary>
    /// Override annual price for this module in this plan
    /// </summary>
    public decimal? OverrideAnnualPrice { get; set; }

    /// <summary>
    /// Whether this module is included free in this plan
    /// </summary>
    public bool IsIncludedFree { get; set; }
}

/// <summary>
/// Module categories for grouping
/// </summary>
public static class ModuleCategories
{
    public const string Core = "Core";
    public const string Finance = "Finance";
    public const string Operations = "Operations";
    public const string HumanResources = "HR";
    public const string Advanced = "Advanced";
    public const string Integration = "Integration";
}
