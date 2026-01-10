using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Finance.Invoices.Recurring;

public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;
    private readonly IRecurringInvoiceService _recurringInvoiceService;

    public IndexModel(IApplicationDbContext context, IRecurringInvoiceService recurringInvoiceService)
    {
        _context = context;
        _recurringInvoiceService = recurringInvoiceService;
    }

    public int ActiveCount { get; set; }
    public int PausedCount { get; set; }
    public int CompletedCount { get; set; }
    public decimal MonthlyRecurring { get; set; }

    public async Task OnGetAsync()
    {
        var stats = await _context.RecurringInvoices
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        ActiveCount = stats.FirstOrDefault(s => s.Status == RecurringInvoiceStatus.Active)?.Count ?? 0;
        PausedCount = stats.FirstOrDefault(s => s.Status == RecurringInvoiceStatus.Paused)?.Count ?? 0;
        CompletedCount = stats.FirstOrDefault(s => s.Status == RecurringInvoiceStatus.Completed)?.Count ?? 0;

        // Calculate monthly recurring revenue (approximate)
        var activeRecurring = await _context.RecurringInvoices
            .Include(r => r.Lines)
            .Where(r => r.Status == RecurringInvoiceStatus.Active)
            .ToListAsync();

        MonthlyRecurring = activeRecurring.Sum(r => CalculateMonthlyAmount(r));
    }

    private decimal CalculateMonthlyAmount(RecurringInvoice recurring)
    {
        var lineTotal = recurring.Lines.Sum(l => l.Quantity * l.UnitPrice * (1 - l.DiscountPercent / 100) * (1 + l.TaxPercent / 100));

        return recurring.Frequency switch
        {
            RecurrenceFrequency.Daily => lineTotal * 30 / recurring.FrequencyInterval,
            RecurrenceFrequency.Weekly => lineTotal * 4.33m / recurring.FrequencyInterval,
            RecurrenceFrequency.Monthly => lineTotal / recurring.FrequencyInterval,
            RecurrenceFrequency.Quarterly => lineTotal / (3 * recurring.FrequencyInterval),
            RecurrenceFrequency.Yearly => lineTotal / (12 * recurring.FrequencyInterval),
            _ => lineTotal
        };
    }

    public async Task<IActionResult> OnGetTableAsync(
        string? search,
        int? statusFilter,
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.RecurringInvoices
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(r =>
                r.Name.ToLower().Contains(search) ||
                (r.Customer != null && r.Customer.Name.ToLower().Contains(search)));
        }

        if (statusFilter.HasValue)
        {
            var status = (RecurringInvoiceStatus)statusFilter.Value;
            query = query.Where(r => r.Status == status);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RecurringInvoiceListItem
            {
                Id = r.Id,
                Name = r.Name,
                CustomerName = r.Customer != null ? r.Customer.Name : "Unknown",
                Status = r.Status,
                Frequency = r.Frequency,
                FrequencyInterval = r.FrequencyInterval,
                NextGenerationDate = r.NextGenerationDate,
                OccurrencesGenerated = r.OccurrencesGenerated,
                MaxOccurrences = r.MaxOccurrences,
                Amount = r.Lines.Sum(l => l.Quantity * l.UnitPrice * (1 - l.DiscountPercent / 100) * (1 + l.TaxPercent / 100)),
                Currency = r.Currency
            })
            .ToListAsync();

        return Partial("_RecurringTableRows", new RecurringTableData
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<IActionResult> OnPostPauseAsync(Guid id)
    {
        try
        {
            await _recurringInvoiceService.PauseAsync(id);
            TempData["Success"] = "Recurring invoice paused.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResumeAsync(Guid id)
    {
        try
        {
            await _recurringInvoiceService.ResumeAsync(id);
            TempData["Success"] = "Recurring invoice resumed.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id)
    {
        try
        {
            await _recurringInvoiceService.CancelAsync(id);
            TempData["Success"] = "Recurring invoice cancelled.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGenerateAsync(Guid id)
    {
        try
        {
            var invoice = await _recurringInvoiceService.GenerateInvoiceAsync(id);
            TempData["Success"] = $"Invoice {invoice.InvoiceNumber} generated successfully.";
            return RedirectToPage("/Finance/Invoices/Details", new { id = invoice.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var recurring = await _context.RecurringInvoices.FindAsync(id);
        if (recurring != null)
        {
            _context.RecurringInvoices.Remove(recurring);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Recurring invoice deleted.";
        }

        return RedirectToPage();
    }
}

public class RecurringInvoiceListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public RecurringInvoiceStatus Status { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public int FrequencyInterval { get; set; }
    public DateTime? NextGenerationDate { get; set; }
    public int OccurrencesGenerated { get; set; }
    public int? MaxOccurrences { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";

    public string FrequencyText => FrequencyInterval == 1
        ? Frequency.ToString()
        : $"Every {FrequencyInterval} {Frequency}s";
}

public class RecurringTableData
{
    public List<RecurringInvoiceListItem> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = Total,
        PageUrl = "/Finance/Invoices/Recurring",
        Handler = "Table",
        HxTarget = "#tableContent",
        HxInclude = "#searchInput,#statusFilter"
    };
}
