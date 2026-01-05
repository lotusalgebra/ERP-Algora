using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Sales;

public class Customer : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; } = CustomerType.Company;

    // Contact Info
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Website { get; set; }

    // Billing Address
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingPostalCode { get; set; }

    // Shipping Address
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingCountry { get; set; }
    public string? ShippingPostalCode { get; set; }

    // Tax Info
    public string? TaxId { get; set; }
    public bool IsTaxExempt { get; set; }

    // Credit Terms
    public int PaymentTermsDays { get; set; } = 30;
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = "USD";

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
}

public class SalesOrder : AuditableEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? DueDate { get; set; }

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    public SalesOrderType OrderType { get; set; } = SalesOrderType.Standard;

    // Shipping Info
    public string? ShippingAddress { get; set; }
    public string? ShippingMethod { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }

    // Totals
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";

    // Payment
    public decimal AmountPaid { get; set; }
    public decimal AmountDue => TotalAmount - AmountPaid;

    public string? Reference { get; set; }
    public string? Notes { get; set; }

    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
}

public class SalesOrderLine : AuditableEntity
{
    public Guid SalesOrderId { get; set; }
    public SalesOrder SalesOrder { get; set; } = null!;

    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSku { get; set; }

    public int LineNumber { get; set; }
    public decimal Quantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    public decimal QuantityShipped { get; set; }
    public decimal QuantityReturned { get; set; }

    public string? Notes { get; set; }
}

public enum CustomerType
{
    Individual = 0,
    Company = 1
}

public enum SalesOrderStatus
{
    Draft = 0,
    Confirmed = 1,
    PartiallyShipped = 2,
    Shipped = 3,
    Delivered = 4,
    PartiallyInvoiced = 5,
    Invoiced = 6,
    Paid = 7,
    Cancelled = 8,
    OnHold = 9
}

public enum SalesOrderType
{
    Standard = 0,
    Return = 1,
    Quote = 2
}
