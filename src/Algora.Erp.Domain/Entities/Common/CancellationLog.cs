namespace Algora.Erp.Domain.Entities.Common;

public class CancellationLog : AuditableEntity
{
    public string DocumentType { get; set; } = string.Empty;
    public Guid DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;

    public DateTime CancelledAt { get; set; }
    public Guid CancelledBy { get; set; }
    public string? CancelledByName { get; set; }

    public string CancellationReason { get; set; } = string.Empty;
    public CancellationReasonCategory ReasonCategory { get; set; }

    // Original Document State (JSON)
    public string? OriginalDocumentState { get; set; }

    // Reversal Details
    public bool StockReversed { get; set; }
    public string? StockReversalDetails { get; set; } // JSON

    public bool FinancialReversed { get; set; }
    public string? FinancialReversalDetails { get; set; } // JSON

    public string? RelatedDocuments { get; set; } // JSON array of affected documents

    public string? Notes { get; set; }
    public string? ApprovalReference { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public enum CancellationReasonCategory
{
    CustomerRequest = 0,
    DuplicateEntry = 1,
    DataEntryError = 2,
    SupplierIssue = 3,
    QualityIssue = 4,
    PriceDispute = 5,
    DeliveryIssue = 6,
    PaymentIssue = 7,
    OutOfStock = 8,
    SystemError = 9,
    Other = 99
}
