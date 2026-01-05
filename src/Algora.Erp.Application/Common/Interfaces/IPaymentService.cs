using Algora.Erp.Domain.Entities.Finance;

namespace Algora.Erp.Application.Common.Interfaces;

/// <summary>
/// Service for managing invoice payments
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Records a payment against an invoice
    /// </summary>
    Task<InvoicePayment> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a payment and restores the invoice balance
    /// </summary>
    Task DeletePaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a payment by ID with invoice details
    /// </summary>
    Task<InvoicePayment?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all payments with optional filtering
    /// </summary>
    Task<PaymentListResult> GetPaymentsAsync(PaymentListRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payment statistics for dashboard
    /// </summary>
    Task<PaymentStatistics> GetPaymentStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an invoice as fully paid
    /// </summary>
    Task MarkInvoiceAsPaidAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a unique payment number
    /// </summary>
    Task<string> GeneratePaymentNumberAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payments for a specific invoice
    /// </summary>
    Task<List<InvoicePayment>> GetPaymentsByInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payments for a specific customer
    /// </summary>
    Task<List<InvoicePayment>> GetPaymentsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a payment receipt PDF
    /// </summary>
    byte[] GenerateReceiptPdf(InvoicePayment payment);
}

/// <summary>
/// Request to record a payment
/// </summary>
public class RecordPaymentRequest
{
    public Guid InvoiceId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Today;
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public Guid? ReceivedBy { get; set; }
}

/// <summary>
/// Request for listing payments
/// </summary>
public class PaymentListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? CustomerId { get; set; }
    public string SortBy { get; set; } = "PaymentDate";
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Result of listing payments
/// </summary>
public class PaymentListResult
{
    public List<PaymentListItem> Payments { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// Payment list item with related data
/// </summary>
public class PaymentListItem
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Reference { get; set; }

    // Invoice details
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal InvoiceTotal { get; set; }

    // Customer details
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
}

/// <summary>
/// Payment statistics for dashboard
/// </summary>
public class PaymentStatistics
{
    public decimal TotalReceived { get; set; }
    public int PaymentCount { get; set; }
    public decimal AveragePayment { get; set; }
    public decimal TodayReceived { get; set; }
    public decimal ThisWeekReceived { get; set; }
    public decimal ThisMonthReceived { get; set; }
    public Dictionary<PaymentMethod, decimal> ByPaymentMethod { get; set; } = new();
    public List<DailyPaymentSummary> DailyTrend { get; set; } = new();
}

/// <summary>
/// Daily payment summary for charts
/// </summary>
public class DailyPaymentSummary
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int Count { get; set; }
}
