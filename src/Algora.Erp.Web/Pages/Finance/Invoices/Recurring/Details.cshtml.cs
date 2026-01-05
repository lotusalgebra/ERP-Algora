using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Finance.Invoices.Recurring;

public class DetailsModel : PageModel
{
    private readonly IApplicationDbContext _context;
    private readonly IRecurringInvoiceService _recurringInvoiceService;

    public DetailsModel(IApplicationDbContext context, IRecurringInvoiceService recurringInvoiceService)
    {
        _context = context;
        _recurringInvoiceService = recurringInvoiceService;
    }

    public RecurringInvoice RecurringInvoice { get; set; } = null!;
    public List<Invoice> GeneratedInvoices { get; set; } = new();
    public decimal EstimatedMonthlyAmount { get; set; }
    public decimal TotalGenerated { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var recurring = await _context.RecurringInvoices
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .Include(r => r.GeneratedInvoices)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recurring == null)
            return NotFound();

        RecurringInvoice = recurring;

        GeneratedInvoices = await _context.Invoices
            .Where(i => i.RecurringInvoiceId == id)
            .OrderByDescending(i => i.InvoiceDate)
            .Take(20)
            .ToListAsync();

        // Calculate totals
        var lineTotal = recurring.Lines.Sum(l => l.Quantity * l.UnitPrice * (1 - l.DiscountPercent / 100) * (1 + l.TaxPercent / 100));

        EstimatedMonthlyAmount = recurring.Frequency switch
        {
            RecurrenceFrequency.Daily => lineTotal * 30 / recurring.FrequencyInterval,
            RecurrenceFrequency.Weekly => lineTotal * 4.33m / recurring.FrequencyInterval,
            RecurrenceFrequency.Monthly => lineTotal / recurring.FrequencyInterval,
            RecurrenceFrequency.Quarterly => lineTotal / (3 * recurring.FrequencyInterval),
            RecurrenceFrequency.Yearly => lineTotal / (12 * recurring.FrequencyInterval),
            _ => lineTotal
        };

        TotalGenerated = GeneratedInvoices.Sum(i => i.TotalAmount);

        return Page();
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

        return RedirectToPage(new { id });
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

        return RedirectToPage(new { id });
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

        return RedirectToPage(new { id });
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
            return RedirectToPage(new { id });
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

        return RedirectToPage("./Index");
    }
}
