using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Ecommerce;

/// <summary>
/// Web/eCommerce order
/// </summary>
public class WebOrder : AuditableEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public Guid? CustomerId { get; set; }

    // Guest checkout info
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }

    // Billing Address
    public string BillingFirstName { get; set; } = string.Empty;
    public string BillingLastName { get; set; } = string.Empty;
    public string? BillingCompany { get; set; }
    public string BillingAddress1 { get; set; } = string.Empty;
    public string? BillingAddress2 { get; set; }
    public string BillingCity { get; set; } = string.Empty;
    public string? BillingState { get; set; }
    public string BillingPostalCode { get; set; } = string.Empty;
    public string BillingCountry { get; set; } = string.Empty;

    // Shipping Address
    public string ShippingFirstName { get; set; } = string.Empty;
    public string ShippingLastName { get; set; } = string.Empty;
    public string? ShippingCompany { get; set; }
    public string ShippingAddress1 { get; set; } = string.Empty;
    public string? ShippingAddress2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string? ShippingState { get; set; }
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public string? ShippingPhone { get; set; }

    // Totals
    public string Currency { get; set; } = "USD";
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? CouponCode { get; set; }
    public decimal ShippingAmount { get; set; }
    public string? ShippingMethod { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }

    // Payment
    public string? PaymentMethod { get; set; }
    public string? PaymentTransactionId { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }

    // Status
    public WebOrderStatus Status { get; set; } = WebOrderStatus.Pending;
    public FulfillmentStatus FulfillmentStatus { get; set; } = FulfillmentStatus.Unfulfilled;

    // Shipping
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public string? ShippingCarrier { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    // Notes
    public string? CustomerNotes { get; set; }
    public string? InternalNotes { get; set; }

    // Refund
    public decimal RefundedAmount { get; set; }
    public string? RefundReason { get; set; }

    // Cancellation
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }

    // Notes (public version)
    public string? Notes { get; set; }

    // IP & Source
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Source { get; set; }  // web, mobile, api

    // Navigation
    public WebCustomer? Customer { get; set; }
    public ICollection<WebOrderItem> Items { get; set; } = new List<WebOrderItem>();
}

public enum WebOrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}

public enum PaymentStatus
{
    Pending,
    Authorized,
    Paid,
    PartiallyRefunded,
    Refunded,
    Failed,
    Voided
}

public enum FulfillmentStatus
{
    Unfulfilled,
    PartiallyFulfilled,
    Fulfilled,
    Returned
}
