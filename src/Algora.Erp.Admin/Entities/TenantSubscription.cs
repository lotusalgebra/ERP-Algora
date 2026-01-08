namespace Algora.Erp.Admin.Entities;

/// <summary>
/// Tenant subscription to a billing plan
/// </summary>
public class TenantSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public Guid PlanId { get; set; }
    public BillingPlan? Plan { get; set; }

    // Billing Cycle
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

    // Period
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }

    // Status
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    // Pricing (at time of subscription)
    public decimal Amount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "INR";

    // Payment
    public string? PaymentMethodId { get; set; }
    public string? PaymentGateway { get; set; } // razorpay, stripe, etc.
    public string? ExternalSubscriptionId { get; set; } // Gateway subscription ID

    // Renewal
    public bool AutoRenew { get; set; } = true;
    public DateTime? NextBillingDate { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public bool CancelAtPeriodEnd { get; set; }

    // Trial
    public bool IsTrialPeriod { get; set; }
    public DateTime? TrialEndDate { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public Guid? ModifiedBy { get; set; }

    public ICollection<TenantBillingInvoice> Invoices { get; set; } = new List<TenantBillingInvoice>();
}

public enum BillingCycle
{
    Monthly = 1,
    Quarterly = 3,
    SemiAnnual = 6,
    Annual = 12
}

public enum SubscriptionStatus
{
    Trial = 0,
    Active = 1,
    PastDue = 2,
    Paused = 3,
    Cancelled = 4,
    Expired = 5
}

/// <summary>
/// Billing invoice for tenant
/// </summary>
public class TenantBillingInvoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }

    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public Guid? SubscriptionId { get; set; }
    public TenantSubscription? Subscription { get; set; }

    // Period
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    // Amounts
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue => TotalAmount - AmountPaid;
    public string Currency { get; set; } = "INR";

    // Status
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    // Payment
    public DateTime? PaidAt { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentTransactionId { get; set; }
    public string? PaymentGateway { get; set; }

    // PDF
    public string? PdfUrl { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    public ICollection<TenantBillingInvoiceLine> Lines { get; set; } = new List<TenantBillingInvoiceLine>();
}

public class TenantBillingInvoiceLine
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid InvoiceId { get; set; }
    public TenantBillingInvoice? Invoice { get; set; }

    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }

    // Reference
    public string? ItemType { get; set; } // subscription, addon, overage, etc.
    public string? ItemCode { get; set; }
}

public enum InvoiceStatus
{
    Draft = 0,
    Pending = 1,
    Sent = 2,
    Paid = 3,
    PartiallyPaid = 4,
    Overdue = 5,
    Cancelled = 6,
    Refunded = 7
}
