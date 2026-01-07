using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Algora.Erp.Web.Pages.Finance.Invoices;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int DraftInvoices { get; set; }
    public int PendingInvoices { get; set; }
    public int OverdueInvoices { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal TotalPaidThisMonth { get; set; }

    public async Task OnGetAsync()
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        DraftInvoices = await _context.Invoices.CountAsync(i => i.Status == InvoiceStatus.Draft);
        PendingInvoices = await _context.Invoices.CountAsync(i =>
            i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Sent);
        OverdueInvoices = await _context.Invoices.CountAsync(i =>
            i.Status == InvoiceStatus.Overdue ||
            (i.DueDate < DateTime.UtcNow && i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Void && i.Status != InvoiceStatus.Cancelled));
        TotalOutstanding = await _context.Invoices
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Void && i.Status != InvoiceStatus.Cancelled)
            .SumAsync(i => i.BalanceDue);
        TotalPaidThisMonth = await _context.InvoicePayments
            .Where(p => p.PaymentDate >= startOfMonth)
            .SumAsync(p => p.Amount);
    }

    public async Task<IActionResult> OnGetTableAsync(
        string? search,
        string? statusFilter,
        string? typeFilter,
        string? dateFrom,
        string? dateTo,
        int page = 1,
        int pageSize = 15)
    {
        var query = _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(search) ||
                (i.Customer != null && i.Customer.Name.ToLower().Contains(search)) ||
                (i.Reference != null && i.Reference.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && int.TryParse(statusFilter, out var status))
        {
            query = query.Where(i => (int)i.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(typeFilter) && int.TryParse(typeFilter, out var type))
        {
            query = query.Where(i => (int)i.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParse(dateFrom, out var fromDate))
        {
            query = query.Where(i => i.InvoiceDate >= fromDate);
        }

        if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParse(dateTo, out var toDate))
        {
            query = query.Where(i => i.InvoiceDate <= toDate);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var invoices = await query
            .OrderByDescending(i => i.InvoiceDate)
            .ThenByDescending(i => i.InvoiceNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_InvoicesTableRows", new InvoicesTableViewModel
        {
            Invoices = invoices,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnPostSendAsync(Guid id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null)
            return NotFound();

        invoice.Status = InvoiceStatus.Sent;
        invoice.SentAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null, null, null);
    }

    public async Task<IActionResult> OnPostMarkPaidAsync(Guid id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null)
            return NotFound();

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidDate = DateTime.UtcNow;
        invoice.PaidAmount = invoice.TotalAmount;
        invoice.BalanceDue = 0;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null, null, null);
    }

    public async Task<IActionResult> OnPostVoidAsync(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null)
            return NotFound();

        var oldStatus = invoice.Status;
        invoice.Status = InvoiceStatus.Void;

        // Create cancellation log entry
        var cancellationLog = new CancellationLog
        {
            Id = Guid.NewGuid(),
            DocumentType = "Invoice",
            DocumentId = invoice.Id,
            DocumentNumber = invoice.InvoiceNumber,
            CancelledAt = DateTime.UtcNow,
            CancelledBy = Guid.Empty,
            CancelledByName = User.Identity?.Name ?? "System",
            CancellationReason = "Voided by user",
            ReasonCategory = CancellationReasonCategory.Other,
            OriginalDocumentState = JsonSerializer.Serialize(new
            {
                Status = oldStatus.ToString(),
                invoice.InvoiceDate,
                invoice.DueDate,
                CustomerName = invoice.BillingName,
                invoice.TotalAmount,
                invoice.PaidAmount,
                TotalLines = invoice.Lines?.Count ?? 0
            }),
            FinancialReversed = invoice.PaidAmount > 0,
            FinancialReversalDetails = invoice.PaidAmount > 0
                ? JsonSerializer.Serialize(new { invoice.PaidAmount, invoice.TotalAmount })
                : null,
            Notes = $"Invoice voided from status: {oldStatus}"
        };
        _context.CancellationLogs.Add(cancellationLog);

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null)
            return NotFound();

        if (invoice.Status == InvoiceStatus.Paid)
        {
            return BadRequest("Cannot delete paid invoices. Void instead.");
        }

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null, null, null);
    }
}

public class InvoicesTableViewModel
{
    public List<Invoice> Invoices { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}
