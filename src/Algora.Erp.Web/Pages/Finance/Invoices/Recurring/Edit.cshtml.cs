using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Finance.Invoices.Recurring;

public class EditModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public EditModel(IApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public RecurringInvoiceEditViewModel Input { get; set; } = new();

    public List<Customer> Customers { get; set; } = new();
    public List<Product> Products { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var recurring = await _context.RecurringInvoices
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recurring == null)
            return NotFound();

        // Don't allow editing cancelled recurring invoices
        if (recurring.Status == RecurringInvoiceStatus.Cancelled)
        {
            TempData["Error"] = "Cancelled recurring invoices cannot be edited.";
            return RedirectToPage("./Details", new { id });
        }

        await LoadDataAsync();

        // Map to view model
        Input = new RecurringInvoiceEditViewModel
        {
            Id = recurring.Id,
            Name = recurring.Name,
            Description = recurring.Description,
            CustomerId = recurring.CustomerId,
            Frequency = recurring.Frequency,
            FrequencyInterval = recurring.FrequencyInterval,
            DayOfMonth = recurring.DayOfMonth,
            DayOfWeek = recurring.DayOfWeek,
            StartDate = recurring.StartDate,
            EndDate = recurring.EndDate,
            MaxOccurrences = recurring.MaxOccurrences,
            InvoiceType = recurring.InvoiceType,
            PaymentTermDays = recurring.PaymentTermDays,
            PaymentTerms = recurring.PaymentTerms,
            Currency = recurring.Currency,
            BillingName = recurring.BillingName,
            BillingAddress = recurring.BillingAddress,
            BillingCity = recurring.BillingCity,
            BillingState = recurring.BillingState,
            BillingPostalCode = recurring.BillingPostalCode,
            BillingCountry = recurring.BillingCountry,
            Reference = recurring.Reference,
            Notes = recurring.Notes,
            InternalNotes = recurring.InternalNotes,
            AutoSend = recurring.AutoSend,
            AutoSendEmail = recurring.AutoSendEmail,
            EmailRecipients = recurring.EmailRecipients,
            Lines = recurring.Lines.OrderBy(l => l.LineNumber).Select(l => new RecurringLineInput
            {
                ProductId = l.ProductId,
                ProductCode = l.ProductCode,
                Description = l.Description,
                Quantity = l.Quantity,
                UnitOfMeasure = l.UnitOfMeasure,
                UnitPrice = l.UnitPrice,
                DiscountPercent = l.DiscountPercent,
                TaxPercent = l.TaxPercent,
                AccountId = l.AccountId,
                Notes = l.Notes
            }).ToList()
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        var recurring = await _context.RecurringInvoices
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == Input.Id);

        if (recurring == null)
            return NotFound();

        // Update properties
        recurring.Name = Input.Name;
        recurring.Description = Input.Description;
        recurring.CustomerId = Input.CustomerId!.Value;
        recurring.Frequency = Input.Frequency;
        recurring.FrequencyInterval = Input.FrequencyInterval;
        recurring.DayOfMonth = Input.DayOfMonth;
        recurring.DayOfWeek = Input.DayOfWeek;
        recurring.StartDate = Input.StartDate;
        recurring.EndDate = Input.EndDate;
        recurring.MaxOccurrences = Input.MaxOccurrences;
        recurring.InvoiceType = Input.InvoiceType;
        recurring.PaymentTermDays = Input.PaymentTermDays;
        recurring.PaymentTerms = Input.PaymentTerms;
        recurring.Currency = Input.Currency;
        recurring.BillingName = Input.BillingName;
        recurring.BillingAddress = Input.BillingAddress;
        recurring.BillingCity = Input.BillingCity;
        recurring.BillingState = Input.BillingState;
        recurring.BillingPostalCode = Input.BillingPostalCode;
        recurring.BillingCountry = Input.BillingCountry;
        recurring.Reference = Input.Reference;
        recurring.Notes = Input.Notes;
        recurring.InternalNotes = Input.InternalNotes;
        recurring.AutoSend = Input.AutoSend;
        recurring.AutoSendEmail = Input.AutoSendEmail;
        recurring.EmailRecipients = Input.EmailRecipients;

        // Recalculate next generation date if schedule changed
        if (recurring.Status == RecurringInvoiceStatus.Active)
        {
            recurring.NextGenerationDate = recurring.CalculateNextGenerationDate();
        }

        // Clear and re-add lines
        foreach (var line in recurring.Lines.ToList())
        {
            _context.RecurringInvoiceLines.Remove(line);
        }

        var lineNumber = 1;
        foreach (var lineInput in Input.Lines.Where(l => !string.IsNullOrEmpty(l.Description) || l.ProductId.HasValue))
        {
            var line = new RecurringInvoiceLine
            {
                RecurringInvoiceId = recurring.Id,
                LineNumber = lineNumber++,
                ProductId = lineInput.ProductId,
                ProductCode = lineInput.ProductCode,
                Description = lineInput.Description,
                Quantity = lineInput.Quantity,
                UnitOfMeasure = lineInput.UnitOfMeasure,
                UnitPrice = lineInput.UnitPrice,
                DiscountPercent = lineInput.DiscountPercent,
                TaxPercent = lineInput.TaxPercent,
                AccountId = lineInput.AccountId,
                Notes = lineInput.Notes
            };

            recurring.Lines.Add(line);
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Recurring invoice '{Input.Name}' updated successfully.";
        return RedirectToPage("./Details", new { id = recurring.Id });
    }

    public async Task<IActionResult> OnGetCustomerDetailsAsync(Guid customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
            return new JsonResult(new { });

        return new JsonResult(new
        {
            billingName = customer.Name,
            billingAddress = customer.BillingAddress,
            billingCity = customer.BillingCity,
            billingState = customer.BillingState,
            billingPostalCode = customer.BillingPostalCode,
            billingCountry = customer.BillingCountry,
            email = customer.Email,
            currency = customer.Currency ?? "USD"
        });
    }

    private async Task LoadDataAsync()
    {
        Customers = await _context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        Products = await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}

public class RecurringInvoiceEditViewModel : RecurringInvoiceCreateViewModel
{
    public Guid Id { get; set; }
}
