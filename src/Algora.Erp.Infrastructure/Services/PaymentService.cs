using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Algora.Erp.Infrastructure.Services;

/// <summary>
/// Service for managing invoice payments
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly ICurrentUserService _currentUserService;

    public PaymentService(
        IApplicationDbContext context,
        IDateTime dateTime,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
    }

    public async Task<InvoicePayment> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null)
            throw new InvalidOperationException($"Invoice with ID {request.InvoiceId} not found.");

        if (invoice.Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Invoice is already fully paid.");

        if (invoice.Status == InvoiceStatus.Void || invoice.Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException("Cannot record payment for voided or cancelled invoice.");

        if (request.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.");

        if (request.Amount > invoice.BalanceDue)
            throw new ArgumentException($"Payment amount ({request.Amount:C2}) exceeds balance due ({invoice.BalanceDue:C2}).");

        var paymentNumber = await GeneratePaymentNumberAsync(cancellationToken);

        var payment = new InvoicePayment
        {
            InvoiceId = invoice.Id,
            PaymentNumber = paymentNumber,
            PaymentDate = request.PaymentDate,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Reference = request.Reference,
            Notes = request.Notes,
            ReceivedBy = request.ReceivedBy ?? _currentUserService.UserId
        };

        _context.InvoicePayments.Add(payment);

        // Update invoice
        invoice.PaidAmount += request.Amount;
        invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;

        if (invoice.BalanceDue <= 0)
        {
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidDate = _dateTime.UtcNow;
            invoice.BalanceDue = 0;
        }
        else if (invoice.PaidAmount > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return payment;
    }

    public async Task DeletePaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _context.InvoicePayments
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment == null)
            throw new InvalidOperationException($"Payment with ID {paymentId} not found.");

        var invoice = payment.Invoice;

        // Restore balance
        invoice.PaidAmount -= payment.Amount;
        invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;

        if (invoice.PaidAmount <= 0)
        {
            invoice.Status = invoice.DueDate < _dateTime.UtcNow ? InvoiceStatus.Overdue : InvoiceStatus.Sent;
            invoice.PaidDate = null;
            invoice.PaidAmount = 0;
        }
        else
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        _context.InvoicePayments.Remove(payment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<InvoicePayment?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await _context.InvoicePayments
            .Include(p => p.Invoice)
                .ThenInclude(i => i.Customer)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
    }

    public async Task<PaymentListResult> GetPaymentsAsync(PaymentListRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.InvoicePayments
            .Include(p => p.Invoice)
                .ThenInclude(i => i.Customer)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.PaymentNumber.ToLower().Contains(term) ||
                p.Invoice.InvoiceNumber.ToLower().Contains(term) ||
                (p.Reference != null && p.Reference.ToLower().Contains(term)) ||
                (p.Invoice.Customer != null && p.Invoice.Customer.Name.ToLower().Contains(term)));
        }

        if (request.PaymentMethod.HasValue)
        {
            query = query.Where(p => p.PaymentMethod == request.PaymentMethod.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= request.ToDate.Value.Date);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(p => p.Invoice.CustomerId == request.CustomerId.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy.ToLower() switch
        {
            "paymentnumber" => request.SortDescending
                ? query.OrderByDescending(p => p.PaymentNumber)
                : query.OrderBy(p => p.PaymentNumber),
            "amount" => request.SortDescending
                ? query.OrderByDescending(p => p.Amount)
                : query.OrderBy(p => p.Amount),
            "invoicenumber" => request.SortDescending
                ? query.OrderByDescending(p => p.Invoice.InvoiceNumber)
                : query.OrderBy(p => p.Invoice.InvoiceNumber),
            "customer" => request.SortDescending
                ? query.OrderByDescending(p => p.Invoice.Customer != null ? p.Invoice.Customer.Name : "")
                : query.OrderBy(p => p.Invoice.Customer != null ? p.Invoice.Customer.Name : ""),
            _ => request.SortDescending
                ? query.OrderByDescending(p => p.PaymentDate)
                : query.OrderBy(p => p.PaymentDate)
        };

        // Apply pagination
        var payments = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PaymentListItem
            {
                Id = p.Id,
                PaymentNumber = p.PaymentNumber,
                PaymentDate = p.PaymentDate,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                Reference = p.Reference,
                InvoiceId = p.InvoiceId,
                InvoiceNumber = p.Invoice.InvoiceNumber,
                InvoiceTotal = p.Invoice.TotalAmount,
                CustomerId = p.Invoice.CustomerId,
                CustomerName = p.Invoice.Customer != null ? p.Invoice.Customer.Name : null
            })
            .ToListAsync(cancellationToken);

        return new PaymentListResult
        {
            Payments = payments,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<PaymentStatistics> GetPaymentStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.InvoicePayments.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(p => p.PaymentDate >= fromDate.Value.Date);
        if (toDate.HasValue)
            query = query.Where(p => p.PaymentDate <= toDate.Value.Date);

        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var allPayments = await query.ToListAsync(cancellationToken);

        var stats = new PaymentStatistics
        {
            TotalReceived = allPayments.Sum(p => p.Amount),
            PaymentCount = allPayments.Count,
            AveragePayment = allPayments.Count > 0 ? allPayments.Average(p => p.Amount) : 0,
            TodayReceived = allPayments.Where(p => p.PaymentDate.Date == today).Sum(p => p.Amount),
            ThisWeekReceived = allPayments.Where(p => p.PaymentDate.Date >= startOfWeek).Sum(p => p.Amount),
            ThisMonthReceived = allPayments.Where(p => p.PaymentDate.Date >= startOfMonth).Sum(p => p.Amount),
            ByPaymentMethod = allPayments
                .GroupBy(p => p.PaymentMethod)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount)),
            DailyTrend = allPayments
                .GroupBy(p => p.PaymentDate.Date)
                .OrderBy(g => g.Key)
                .TakeLast(30)
                .Select(g => new DailyPaymentSummary
                {
                    Date = g.Key,
                    Amount = g.Sum(p => p.Amount),
                    Count = g.Count()
                })
                .ToList()
        };

        return stats;
    }

    public async Task MarkInvoiceAsPaidAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices.FindAsync(new object[] { invoiceId }, cancellationToken);
        if (invoice == null)
            throw new InvalidOperationException($"Invoice with ID {invoiceId} not found.");

        if (invoice.BalanceDue <= 0)
            return; // Already paid

        // Record a full payment
        var paymentNumber = await GeneratePaymentNumberAsync(cancellationToken);

        var payment = new InvoicePayment
        {
            InvoiceId = invoice.Id,
            PaymentNumber = paymentNumber,
            PaymentDate = DateTime.Today,
            Amount = invoice.BalanceDue,
            PaymentMethod = PaymentMethod.Other,
            Notes = "Marked as paid",
            ReceivedBy = _currentUserService.UserId
        };

        _context.InvoicePayments.Add(payment);

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidDate = _dateTime.UtcNow;
        invoice.PaidAmount = invoice.TotalAmount;
        invoice.BalanceDue = 0;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> GeneratePaymentNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = _dateTime.UtcNow.Year;
        var count = await _context.InvoicePayments
            .Where(p => p.PaymentDate.Year == year)
            .CountAsync(cancellationToken) + 1;

        return $"PAY-{year}-{count:D5}";
    }

    public async Task<List<InvoicePayment>> GetPaymentsByInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _context.InvoicePayments
            .Where(p => p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<InvoicePayment>> GetPaymentsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.InvoicePayments
            .Include(p => p.Invoice)
            .Where(p => p.Invoice.CustomerId == customerId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public byte[] GenerateReceiptPdf(InvoicePayment payment)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Column(column =>
                    {
                        column.Item().Text("PAYMENT RECEIPT").Bold().FontSize(18).AlignCenter();
                        column.Item().Height(10);
                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                page.Content()
                    .PaddingVertical(20)
                    .Column(column =>
                    {
                        // Receipt details
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Receipt Number:").SemiBold();
                                col.Item().Text(payment.PaymentNumber);
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Date:").SemiBold();
                                col.Item().Text(payment.PaymentDate.ToString("MMMM dd, yyyy"));
                            });
                        });

                        column.Item().Height(20);

                        // Invoice reference
                        column.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
                        {
                            col.Item().Text("Payment For:").SemiBold();
                            col.Item().Text($"Invoice: {payment.Invoice.InvoiceNumber}");
                            if (payment.Invoice.Customer != null)
                            {
                                col.Item().Text($"Customer: {payment.Invoice.Customer.Name}");
                            }
                        });

                        column.Item().Height(20);

                        // Payment details
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                            });

                            table.Cell().Text("Payment Method:").SemiBold();
                            table.Cell().Text(payment.PaymentMethod.ToString());

                            if (!string.IsNullOrEmpty(payment.Reference))
                            {
                                table.Cell().Text("Reference:").SemiBold();
                                table.Cell().Text(payment.Reference);
                            }

                            table.Cell().Text("Amount Paid:").SemiBold();
                            table.Cell().Text(payment.Amount.ToString("C2")).Bold().FontSize(14);
                        });

                        column.Item().Height(20);

                        // Notes
                        if (!string.IsNullOrEmpty(payment.Notes))
                        {
                            column.Item().Text("Notes:").SemiBold();
                            column.Item().Text(payment.Notes).FontColor(Colors.Grey.Darken1);
                        }

                        column.Item().Height(30);

                        // Thank you message
                        column.Item().AlignCenter().Text("Thank you for your payment!").Italic();
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Generated on ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.Span(DateTime.Now.ToString("MMMM dd, yyyy HH:mm")).FontSize(8).FontColor(Colors.Grey.Medium);
                    });
            });
        });

        return document.GeneratePdf();
    }
}
