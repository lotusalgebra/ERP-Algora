using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Finance;

public class JournalEntry : AuditableEntity
{
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string? Reference { get; set; }
    public string Description { get; set; } = string.Empty;

    public JournalEntryType EntryType { get; set; }
    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;

    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public string Currency { get; set; } = "USD";

    public bool IsAdjusting { get; set; }
    public bool IsClosing { get; set; }
    public bool IsReversing { get; set; }
    public Guid? ReversingEntryId { get; set; }

    public Guid? PostedBy { get; set; }
    public DateTime? PostedAt { get; set; }

    public string? Notes { get; set; }
    public string? AttachmentUrl { get; set; }

    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}

public class JournalEntryLine : AuditableEntity
{
    public Guid JournalEntryId { get; set; }
    public JournalEntry JournalEntry { get; set; } = null!;

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public string? Description { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }

    public int LineNumber { get; set; }

    // Optional reference to source document
    public string? SourceDocumentType { get; set; }
    public Guid? SourceDocumentId { get; set; }
}

public enum JournalEntryType
{
    General = 0,
    Sales = 1,
    Purchase = 2,
    CashReceipt = 3,
    CashPayment = 4,
    Payroll = 5,
    Adjustment = 6,
    Closing = 7,
    Opening = 8
}

public enum JournalEntryStatus
{
    Draft = 0,
    Pending = 1,
    Posted = 2,
    Reversed = 3,
    Void = 4
}
