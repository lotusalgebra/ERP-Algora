using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Enums;

namespace Algora.Erp.Domain.Entities.Administration;

/// <summary>
/// Represents a tenant (company/organization) in the system
/// Stored in the master database
/// </summary>
public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? TaxId { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string TimeZone { get; set; } = "UTC";
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public TenantPlan Plan { get; set; } = TenantPlan.Basic;
    public DateTime? TrialEndsAt { get; set; }
    public DateTime? SubscriptionEndsAt { get; set; }
    public int MaxUsers { get; set; } = 5;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();
}
