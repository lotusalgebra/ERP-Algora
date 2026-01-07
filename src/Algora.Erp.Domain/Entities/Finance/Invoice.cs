using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Sales;
using Algora.Erp.Domain.Entities.Settings;

namespace Algora.Erp.Domain.Entities.Finance;

public class Invoice : AuditableEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public InvoiceType Type { get; set; } = InvoiceType.SalesInvoice;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    // Customer/Supplier
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? SupplierId { get; set; }

    // Related documents
    public Guid? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? RecurringInvoiceId { get; set; }
    public RecurringInvoice? RecurringInvoice { get; set; }

    // Dates
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }

    // Amounts
    public decimal SubTotal { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public string Currency { get; set; } = "INR";

    // GST Fields
    public Guid? GstSlabId { get; set; }
    public GstSlab? GstSlab { get; set; }
    public bool IsInterState { get; set; } // True = IGST, False = CGST+SGST
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstAmount { get; set; }
    public decimal CgstRate { get; set; }
    public decimal SgstRate { get; set; }
    public decimal IgstRate { get; set; }

    // Location for GST
    public Guid? FromLocationId { get; set; }
    public OfficeLocation? FromLocation { get; set; }
    public string? CustomerGstin { get; set; }
    public string? CustomerStateCode { get; set; }

    // Payment Terms
    public string? PaymentTerms { get; set; }
    public int PaymentTermDays { get; set; } = 30;

    // Billing Address
    public string? BillingName { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; }

    // Additional Info
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? AttachmentUrl { get; set; }

    // Tracking
    public Guid? SentBy { get; set; }
    public DateTime? SentAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    public ICollection<InvoicePayment> Payments { get; set; } = new List<InvoicePayment>();
}

public class InvoiceLine : AuditableEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public int LineNumber { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    // GST Fields for line item
    public Guid? GstSlabId { get; set; }
    public GstSlab? GstSlab { get; set; }
    public decimal CgstRate { get; set; }
    public decimal SgstRate { get; set; }
    public decimal IgstRate { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstAmount { get; set; }
    public string? HsnCode { get; set; }

    public Guid? AccountId { get; set; }
    public string? Notes { get; set; }
}

public class InvoicePayment : AuditableEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }

    public Guid? ReceivedBy { get; set; }
}

public enum InvoiceType
{
    SalesInvoice = 0,
    PurchaseInvoice = 1,
    CreditNote = 2,
    DebitNote = 3
}

public enum InvoiceStatus
{
    Draft = 0,
    Pending = 1,
    Sent = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Overdue = 5,
    Void = 6,
    Cancelled = 7
}

public enum PaymentMethod
{
    Cash = 0,
    Check = 1,
    BankTransfer = 2,
    CreditCard = 3,
    DebitCard = 4,
    PayPal = 5,
    Other = 6
}
