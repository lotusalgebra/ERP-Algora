namespace Algora.Erp.Admin.Entities;

/// <summary>
/// Tenant (Customer organization) with soft delete support
/// </summary>
public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Basic Info
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty; // unique subdomain for tenant
    public string? CustomDomain { get; set; }

    // Contact
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactPerson { get; set; }

    // Company Details
    public string? CompanyName { get; set; }
    public string? TaxId { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }

    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    // Database
    public string? DatabaseName { get; set; }
    public string? ConnectionString { get; set; }

    // Status
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public bool IsActive { get; set; } = true;

    // Subscription
    public Guid? CurrentSubscriptionId { get; set; }
    public TenantSubscription? CurrentSubscription { get; set; }

    // Trial
    public DateTime? TrialStartedAt { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public bool IsInTrial => TrialEndsAt.HasValue && DateTime.UtcNow < TrialEndsAt;

    // Limits (cached from plan)
    public int MaxUsers { get; set; }
    public string? CurrencyCode { get; set; }
    public string? TimeZone { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public string? DeletionReason { get; set; }

    // Suspension
    public bool IsSuspended { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public Guid? SuspendedBy { get; set; }
    public string? SuspensionReason { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public Guid? ModifiedBy { get; set; }

    // Navigation
    public ICollection<TenantSubscription> Subscriptions { get; set; } = new List<TenantSubscription>();
    public ICollection<TenantBillingInvoice> Invoices { get; set; } = new List<TenantBillingInvoice>();
    public ICollection<TenantUser> Users { get; set; } = new List<TenantUser>();
}

public enum TenantStatus
{
    Pending = 0,      // Awaiting setup
    Active = 1,       // Active and running
    Suspended = 2,    // Temporarily suspended
    Cancelled = 3,    // Subscription cancelled
    Deleted = 4       // Soft deleted
}

/// <summary>
/// Users within a tenant (synced from tenant database)
/// </summary>
public class TenantUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public bool IsOwner { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}
