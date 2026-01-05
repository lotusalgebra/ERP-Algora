using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Finance.Invoices;

public class CreateModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public CreateModel(IApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InvoiceCreateViewModel Input { get; set; } = new();

    public List<Customer> Customers { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public List<SalesOrder> SalesOrders { get; set; } = new();

    public async Task OnGetAsync(Guid? salesOrderId = null)
    {
        await LoadDataAsync();

        // Pre-fill from Sales Order if provided
        if (salesOrderId.HasValue)
        {
            var salesOrder = await _context.SalesOrders
                .Include(s => s.Customer)
                .Include(s => s.Lines)
                .FirstOrDefaultAsync(s => s.Id == salesOrderId.Value);

            if (salesOrder != null)
            {
                Input.CustomerId = salesOrder.CustomerId;
                Input.SalesOrderId = salesOrder.Id;
                Input.Reference = $"SO# {salesOrder.OrderNumber}";
                Input.BillingName = salesOrder.Customer?.Name;
                Input.BillingAddress = salesOrder.Customer?.BillingAddress;
                Input.BillingCity = salesOrder.Customer?.BillingCity;
                Input.BillingState = salesOrder.Customer?.BillingState;
                Input.BillingPostalCode = salesOrder.Customer?.BillingPostalCode;
                Input.BillingCountry = salesOrder.Customer?.BillingCountry;
                Input.Currency = salesOrder.Currency ?? "USD";

                // Copy lines
                foreach (var line in salesOrder.Lines.OrderBy(l => l.LineNumber))
                {
                    Input.Lines.Add(new InvoiceLineInput
                    {
                        ProductId = line.ProductId,
                        ProductCode = line.ProductSku,
                        Description = line.ProductName ?? "",
                        Quantity = line.Quantity,
                        UnitPrice = line.UnitPrice,
                        DiscountPercent = line.DiscountPercent,
                        TaxPercent = line.TaxPercent
                    });
                }
            }
        }

        // Set defaults
        Input.InvoiceDate = DateTime.Today;
        Input.DueDate = DateTime.Today.AddDays(Input.PaymentTermDays);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        // Generate invoice number
        var lastInvoice = await _context.Invoices
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastInvoice != null && lastInvoice.InvoiceNumber.StartsWith("INV-"))
        {
            var parts = lastInvoice.InvoiceNumber.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out var num))
            {
                nextNumber = num + 1;
            }
        }

        var invoiceNumber = $"INV-{DateTime.Now.Year}-{nextNumber:D4}";

        // Calculate totals
        decimal subtotal = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            Type = Input.Type,
            Status = InvoiceStatus.Draft,
            CustomerId = Input.CustomerId,
            SalesOrderId = Input.SalesOrderId,
            InvoiceDate = Input.InvoiceDate,
            DueDate = Input.DueDate,
            PaymentTerms = Input.PaymentTerms,
            PaymentTermDays = Input.PaymentTermDays,
            Currency = Input.Currency,
            BillingName = Input.BillingName,
            BillingAddress = Input.BillingAddress,
            BillingCity = Input.BillingCity,
            BillingState = Input.BillingState,
            BillingPostalCode = Input.BillingPostalCode,
            BillingCountry = Input.BillingCountry,
            Reference = Input.Reference,
            Notes = Input.Notes,
            InternalNotes = Input.InternalNotes
        };

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

            invoice.Lines.Add(line);

            subtotal += lineSubtotal;
            totalDiscount += lineDiscountAmt;
            totalTax += lineTaxAmt;
        }

        invoice.SubTotal = subtotal;
        invoice.DiscountPercent = subtotal > 0 ? (totalDiscount / subtotal) * 100 : 0;
        invoice.DiscountAmount = totalDiscount;
        invoice.TaxAmount = totalTax;
        invoice.TotalAmount = subtotal - totalDiscount + totalTax;
        invoice.BalanceDue = invoice.TotalAmount;

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Invoice {invoiceNumber} created successfully.";
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

public class InvoiceCreateViewModel
{
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

public class InvoiceLineInput
{
    public Guid? ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public string? UnitOfMeasure { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
}
