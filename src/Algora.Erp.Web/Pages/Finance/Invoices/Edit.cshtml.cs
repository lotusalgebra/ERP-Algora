using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Finance.Invoices;

public class EditModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public EditModel(IApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InvoiceEditViewModel Input { get; set; } = new();

    public Invoice Invoice { get; set; } = null!;
    public List<Customer> Customers { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public List<SalesOrder> SalesOrders { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return NotFound();

        // Only allow editing Draft invoices
        if (invoice.Status != InvoiceStatus.Draft)
        {
            TempData["Error"] = "Only draft invoices can be edited.";
            return RedirectToPage("./Details", new { id });
        }

        Invoice = invoice;
        await LoadDataAsync();

        // Map invoice to view model
        Input = new InvoiceEditViewModel
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            Type = invoice.Type,
            CustomerId = invoice.CustomerId,
            SalesOrderId = invoice.SalesOrderId,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            PaymentTerms = invoice.PaymentTerms,
            PaymentTermDays = invoice.PaymentTermDays,
            Currency = invoice.Currency,
            BillingName = invoice.BillingName,
            BillingAddress = invoice.BillingAddress,
            BillingCity = invoice.BillingCity,
            BillingState = invoice.BillingState,
            BillingPostalCode = invoice.BillingPostalCode,
            BillingCountry = invoice.BillingCountry,
            Reference = invoice.Reference,
            Notes = invoice.Notes,
            InternalNotes = invoice.InternalNotes,
            Lines = invoice.Lines.OrderBy(l => l.LineNumber).Select(l => new InvoiceLineInput
            {
                ProductId = l.ProductId,
                ProductCode = l.ProductCode,
                Description = l.Description,
                Quantity = l.Quantity,
                UnitOfMeasure = l.UnitOfMeasure,
                UnitPrice = l.UnitPrice,
                DiscountPercent = l.DiscountPercent,
                TaxPercent = l.TaxPercent
            }).ToList()
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var invoice = await _context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == Input.Id);

        if (invoice == null)
            return NotFound();

        // Only allow editing Draft invoices
        if (invoice.Status != InvoiceStatus.Draft)
        {
            TempData["Error"] = "Only draft invoices can be edited.";
            return RedirectToPage("./Details", new { id = Input.Id });
        }

        if (!ModelState.IsValid)
        {
            Invoice = invoice;
            await LoadDataAsync();
            return Page();
        }

        // Update invoice properties
        invoice.Type = Input.Type;
        invoice.CustomerId = Input.CustomerId;
        invoice.SalesOrderId = Input.SalesOrderId;
        invoice.InvoiceDate = Input.InvoiceDate;
        invoice.DueDate = Input.DueDate;
        invoice.PaymentTerms = Input.PaymentTerms;
        invoice.PaymentTermDays = Input.PaymentTermDays;
        invoice.Currency = Input.Currency;
        invoice.BillingName = Input.BillingName;
        invoice.BillingAddress = Input.BillingAddress;
        invoice.BillingCity = Input.BillingCity;
        invoice.BillingState = Input.BillingState;
        invoice.BillingPostalCode = Input.BillingPostalCode;
        invoice.BillingCountry = Input.BillingCountry;
        invoice.Reference = Input.Reference;
        invoice.Notes = Input.Notes;
        invoice.InternalNotes = Input.InternalNotes;
        invoice.ModifiedAt = DateTime.UtcNow;

        // Remove existing lines
        _context.InvoiceLines.RemoveRange(invoice.Lines);

        // Calculate totals and add new lines
        decimal subtotal = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;

        var lineNumber = 1;
        foreach (var lineInput in Input.Lines.Where(l => !string.IsNullOrEmpty(l.Description) || l.ProductId.HasValue))
        {
            var lineSubtotal = lineInput.Quantity * lineInput.UnitPrice;
            var lineDiscountAmt = lineSubtotal * (lineInput.DiscountPercent / 100);
            var lineAfterDiscount = lineSubtotal - lineDiscountAmt;
            var lineTaxAmt = lineAfterDiscount * (lineInput.TaxPercent / 100);
            var lineTotal = lineAfterDiscount + lineTaxAmt;

            var line = new InvoiceLine
            {
                InvoiceId = invoice.Id,
                LineNumber = lineNumber++,
                ProductId = lineInput.ProductId,
                ProductCode = lineInput.ProductCode,
                Description = lineInput.Description,
                Quantity = lineInput.Quantity,
                UnitOfMeasure = lineInput.UnitOfMeasure,
                UnitPrice = lineInput.UnitPrice,
                DiscountPercent = lineInput.DiscountPercent,
                DiscountAmount = lineDiscountAmt,
                TaxPercent = lineInput.TaxPercent,
                TaxAmount = lineTaxAmt,
                LineTotal = lineTotal
            };

            _context.InvoiceLines.Add(line);

            subtotal += lineSubtotal;
            totalDiscount += lineDiscountAmt;
            totalTax += lineTaxAmt;
        }

        invoice.SubTotal = subtotal;
        invoice.DiscountPercent = subtotal > 0 ? (totalDiscount / subtotal) * 100 : 0;
        invoice.DiscountAmount = totalDiscount;
        invoice.TaxAmount = totalTax;
        invoice.TotalAmount = subtotal - totalDiscount + totalTax;
        invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Invoice {invoice.InvoiceNumber} updated successfully.";
        return RedirectToPage("./Details", new { id = invoice.Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var invoice = await _context.Invoices
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == Input.Id);

        if (invoice == null)
            return NotFound();

        // Only allow deleting Draft invoices with no payments
        if (invoice.Status != InvoiceStatus.Draft)
        {
            TempData["Error"] = "Only draft invoices can be deleted.";
            return RedirectToPage("./Details", new { id = Input.Id });
        }

        if (invoice.Payments.Any())
        {
            TempData["Error"] = "Cannot delete an invoice with payments. Void the invoice instead.";
            return RedirectToPage("./Details", new { id = Input.Id });
        }

        _context.InvoiceLines.RemoveRange(invoice.Lines);
        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Invoice {invoice.InvoiceNumber} deleted successfully.";
        return RedirectToPage("./Index");
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

        SalesOrders = await _context.SalesOrders
            .Include(s => s.Customer)
            .Where(s => s.Status >= SalesOrderStatus.Confirmed)
            .OrderByDescending(s => s.OrderDate)
            .Take(50)
            .ToListAsync();
    }
}

public class InvoiceEditViewModel
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public InvoiceType Type { get; set; } = InvoiceType.SalesInvoice;
    public Guid? CustomerId { get; set; }
    public Guid? SalesOrderId { get; set; }

    public DateTime InvoiceDate { get; set; } = DateTime.Today;
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);

    public string? PaymentTerms { get; set; } = "Net 30";
    public int PaymentTermDays { get; set; } = 30;
    public string Currency { get; set; } = "USD";

    public string? BillingName { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; }

    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    public List<InvoiceLineInput> Lines { get; set; } = new();
}
