using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Finance.Invoices.Recurring;

public class CreateModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public CreateModel(IApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public RecurringInvoiceCreateViewModel Input { get; set; } = new();

    public List<Customer> Customers { get; set; } = new();
    public List<Product> Products { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadDataAsync();

        // Set defaults
        Input.StartDate = DateTime.Today;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        var recurring = new RecurringInvoice
        {
            Name = Input.Name,
            Description = Input.Description,
            Status = RecurringInvoiceStatus.Active,
            CustomerId = Input.CustomerId!.Value,
            Frequency = Input.Frequency,
            FrequencyInterval = Input.FrequencyInterval,
            DayOfMonth = Input.DayOfMonth,
            DayOfWeek = Input.DayOfWeek,
            StartDate = Input.StartDate,
            EndDate = Input.EndDate,
            MaxOccurrences = Input.MaxOccurrences,
            InvoiceType = Input.InvoiceType,
            PaymentTermDays = Input.PaymentTermDays,
            PaymentTerms = Input.PaymentTerms,
            Currency = Input.Currency,
            BillingName = Input.BillingName,
            BillingAddress = Input.BillingAddress,
            BillingCity = Input.BillingCity,
            BillingState = Input.BillingState,
            BillingPostalCode = Input.BillingPostalCode,
            BillingCountry = Input.BillingCountry,
            Reference = Input.Reference,
            Notes = Input.Notes,
            InternalNotes = Input.InternalNotes,
            AutoSend = Input.AutoSend,
            AutoSendEmail = Input.AutoSendEmail,
            EmailRecipients = Input.EmailRecipients
        };

        // Set next generation date
        recurring.NextGenerationDate = Input.StartDate;

        // Add lines
        var lineNumber = 1;
        foreach (var lineInput in Input.Lines.Where(l => !string.IsNullOrEmpty(l.Description) || l.ProductId.HasValue))
        {
            var line = new RecurringInvoiceLine
            {
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

        _context.RecurringInvoices.Add(recurring);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Recurring invoice '{Input.Name}' created successfully.";
        return RedirectToPage("./Index");
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

public class RecurringInvoiceCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CustomerId { get; set; }

    // Schedule
    public RecurrenceFrequency Frequency { get; set; } = RecurrenceFrequency.Monthly;
    public int FrequencyInterval { get; set; } = 1;
    public int? DayOfMonth { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }
    public int? MaxOccurrences { get; set; }

    // Invoice Template
    public InvoiceType InvoiceType { get; set; } = InvoiceType.SalesInvoice;
    public int PaymentTermDays { get; set; } = 30;
    public string? PaymentTerms { get; set; } = "Net 30";
    public string Currency { get; set; } = "USD";

    // Billing
    public string? BillingName { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; }

    // Additional
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    // Auto-send
    public bool AutoSend { get; set; }
    public bool AutoSendEmail { get; set; }
    public string? EmailRecipients { get; set; }

    public List<RecurringLineInput> Lines { get; set; } = new();
}

public class RecurringLineInput
{
    public Guid? ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public string? UnitOfMeasure { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public Guid? AccountId { get; set; }
    public string? Notes { get; set; }
}
