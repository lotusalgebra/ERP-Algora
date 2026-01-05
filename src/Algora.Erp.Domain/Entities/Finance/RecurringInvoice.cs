using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Sales;

namespace Algora.Erp.Domain.Entities.Finance;

public class RecurringInvoice : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RecurringInvoiceStatus Status { get; set; } = RecurringInvoiceStatus.Active;

    // Customer
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    // Schedule
    public RecurrenceFrequency Frequency { get; set; } = RecurrenceFrequency.Monthly;
    public int FrequencyInterval { get; set; } = 1; // e.g., every 2 months
    public int? DayOfMonth { get; set; } // For monthly: 1-31 (null = same day as start)
    public DayOfWeek? DayOfWeek { get; set; } // For weekly
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; } // null = indefinite
    public int? MaxOccurrences { get; set; } // null = unlimited
    public int OccurrencesGenerated { get; set; }
    public DateTime? NextGenerationDate { get; set; }
    public DateTime? LastGeneratedDate { get; set; }

    // Invoice Template
    public InvoiceType InvoiceType { get; set; } = InvoiceType.SalesInvoice;
    public int PaymentTermDays { get; set; } = 30;
    public string? PaymentTerms { get; set; } = "Net 30";
    public string Currency { get; set; } = "USD";

    // Billing Address (copied from customer or overridden)
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

    // Auto-send options
    public bool AutoSend { get; set; }
    public bool AutoSendEmail { get; set; }
    public string? EmailRecipients { get; set; } // Comma-separated emails

    // Line Items (template)
    public ICollection<RecurringInvoiceLine> Lines { get; set; } = new List<RecurringInvoiceLine>();

    // Generated Invoices
    public ICollection<Invoice> GeneratedInvoices { get; set; } = new List<Invoice>();

    public DateTime CalculateNextGenerationDate()
    {
        var baseDate = LastGeneratedDate ?? StartDate;

        return Frequency switch
        {
            RecurrenceFrequency.Daily => baseDate.AddDays(FrequencyInterval),
            RecurrenceFrequency.Weekly => baseDate.AddDays(7 * FrequencyInterval),
            RecurrenceFrequency.Monthly => CalculateNextMonthlyDate(baseDate),
            RecurrenceFrequency.Quarterly => baseDate.AddMonths(3 * FrequencyInterval),
            RecurrenceFrequency.Yearly => baseDate.AddYears(FrequencyInterval),
            _ => baseDate.AddMonths(FrequencyInterval)
        };
    }

    private DateTime CalculateNextMonthlyDate(DateTime baseDate)
    {
        var nextDate = baseDate.AddMonths(FrequencyInterval);

        if (DayOfMonth.HasValue)
        {
            var day = Math.Min(DayOfMonth.Value, DateTime.DaysInMonth(nextDate.Year, nextDate.Month));
            nextDate = new DateTime(nextDate.Year, nextDate.Month, day);
        }

        return nextDate;
    }

    public bool ShouldGenerate()
    {
        if (Status != RecurringInvoiceStatus.Active)
            return false;

        if (EndDate.HasValue && DateTime.Today > EndDate.Value)
            return false;

        if (MaxOccurrences.HasValue && OccurrencesGenerated >= MaxOccurrences.Value)
            return false;

        if (!NextGenerationDate.HasValue)
            return false;

        return DateTime.Today >= NextGenerationDate.Value.Date;
    }
}

public class RecurringInvoiceLine : AuditableEntity
{
    public Guid RecurringInvoiceId { get; set; }
    public RecurringInvoice RecurringInvoice { get; set; } = null!;

    public int LineNumber { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }

    public Guid? AccountId { get; set; }
    public string? Notes { get; set; }
}

public enum RecurringInvoiceStatus
{
    Active = 0,
    Paused = 1,
    Completed = 2,
    Cancelled = 3
}

public enum RecurrenceFrequency
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2,
    Quarterly = 3,
    Yearly = 4
}
