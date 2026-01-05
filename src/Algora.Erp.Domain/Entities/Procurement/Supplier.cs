using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Procurement;

public class Supplier : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    // Contact Info
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Website { get; set; }

    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    // Bank Info
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankRoutingNumber { get; set; }

    // Tax Info
    public string? TaxId { get; set; }

    // Payment Terms
    public int PaymentTermsDays { get; set; } = 30;
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = "USD";

    // Lead Time
    public int LeadTimeDays { get; set; }
    public decimal MinimumOrderAmount { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}

public class PurchaseOrder : AuditableEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }

    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public Guid? WarehouseId { get; set; }

    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    // Shipping Info
    public string? ShippingAddress { get; set; }
    public string? ShippingMethod { get; set; }

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

    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}

public class PurchaseOrderLine : AuditableEntity
{
    public Guid PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

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

    public decimal QuantityReceived { get; set; }
    public decimal QuantityReturned { get; set; }

    public string? Notes { get; set; }
}

public enum PurchaseOrderStatus
{
    Draft = 0,
    Pending = 1,
    Approved = 2,
    Sent = 3,
    PartiallyReceived = 4,
    Received = 5,
    PartiallyInvoiced = 6,
    Invoiced = 7,
    Paid = 8,
    Cancelled = 9,
    OnHold = 10
}
