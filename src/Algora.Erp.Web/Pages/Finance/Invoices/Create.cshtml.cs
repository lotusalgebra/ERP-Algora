using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Sales;
using Algora.Erp.Domain.Entities.Settings;
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
    public List<GstSlab> GstSlabs { get; set; } = new();
    public List<OfficeLocation> Locations { get; set; } = new();
    public List<IndianState> States { get; set; } = new();

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

        // Determine if inter-state transaction
        var fromLocation = Input.FromLocationId.HasValue
            ? await _context.OfficeLocations.Include(l => l.State).FirstOrDefaultAsync(l => l.Id == Input.FromLocationId)
            : null;
        var customerState = Input.CustomerStateId.HasValue
            ? await _context.IndianStates.FirstOrDefaultAsync(s => s.Id == Input.CustomerStateId)
            : null;

        var isInterState = fromLocation?.StateId != Input.CustomerStateId && Input.CustomerStateId.HasValue;

        // Calculate totals
        decimal subtotal = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;
        decimal totalCgst = 0;
        decimal totalSgst = 0;
        decimal totalIgst = 0;

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
            BillingState = customerState?.Name ?? Input.BillingState,
            BillingPostalCode = Input.BillingPostalCode,
            BillingCountry = Input.BillingCountry,
            Reference = Input.Reference,
            Notes = Input.Notes,
            InternalNotes = Input.InternalNotes,
            // GST Fields
            FromLocationId = Input.FromLocationId,
            CustomerGstin = Input.CustomerGstin,
            CustomerStateCode = customerState?.ShortName,
            IsInterState = isInterState
        };

        var lineNumber = 1;
        foreach (var lineInput in Input.Lines.Where(l => !string.IsNullOrEmpty(l.Description) || l.ProductId.HasValue))
        {
            var lineSubtotal = lineInput.Quantity * lineInput.UnitPrice;
            var lineDiscountAmt = lineSubtotal * (lineInput.DiscountPercent / 100);
            var lineAfterDiscount = lineSubtotal - lineDiscountAmt;

            // Get GST slab for this line
            var gstSlab = lineInput.GstSlabId.HasValue
                ? await _context.GstSlabs.FirstOrDefaultAsync(g => g.Id == lineInput.GstSlabId)
                : null;

            decimal lineCgst = 0, lineSgst = 0, lineIgst = 0;
            decimal cgstRate = 0, sgstRate = 0, igstRate = 0;

            if (gstSlab != null)
            {
                if (isInterState)
                {
                    // Inter-state: Apply IGST
                    igstRate = gstSlab.IgstRate;
                    lineIgst = lineAfterDiscount * (igstRate / 100);
                }
                else
                {
                    // Intra-state: Apply CGST + SGST
                    cgstRate = gstSlab.CgstRate;
                    sgstRate = gstSlab.SgstRate;
                    lineCgst = lineAfterDiscount * (cgstRate / 100);
                    lineSgst = lineAfterDiscount * (sgstRate / 100);
                }
            }

            var lineTaxAmt = lineCgst + lineSgst + lineIgst;
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
                TaxPercent = gstSlab?.Rate ?? lineInput.TaxPercent,
                TaxAmount = lineTaxAmt,
                LineTotal = lineTotal,
                // GST Fields
                GstSlabId = lineInput.GstSlabId,
                HsnCode = lineInput.HsnCode,
                CgstRate = cgstRate,
                SgstRate = sgstRate,
                IgstRate = igstRate,
                CgstAmount = lineCgst,
                SgstAmount = lineSgst,
                IgstAmount = lineIgst
            };

            invoice.Lines.Add(line);

            subtotal += lineSubtotal;
            totalDiscount += lineDiscountAmt;
            totalTax += lineTaxAmt;
            totalCgst += lineCgst;
            totalSgst += lineSgst;
            totalIgst += lineIgst;
        }

        invoice.SubTotal = subtotal;
        invoice.DiscountPercent = subtotal > 0 ? (totalDiscount / subtotal) * 100 : 0;
        invoice.DiscountAmount = totalDiscount;
        invoice.TaxAmount = totalTax;
        invoice.CgstAmount = totalCgst;
        invoice.SgstAmount = totalSgst;
        invoice.IgstAmount = totalIgst;
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

        GstSlabs = await _context.GstSlabs
            .Where(g => g.IsActive && !g.IsDeleted)
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync();

        Locations = await _context.OfficeLocations
            .Include(l => l.State)
            .Where(l => l.IsActive && !l.IsDeleted)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();

        States = await _context.IndianStates
            .OrderBy(s => s.Name)
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
    public string Currency { get; set; } = "INR";

    public string? BillingName { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; } = "India";

    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    // GST Fields
    public Guid? FromLocationId { get; set; }
    public string? CustomerGstin { get; set; }
    public Guid? CustomerStateId { get; set; }
    public bool IsInterState { get; set; }

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

    // GST Fields
    public Guid? GstSlabId { get; set; }
    public string? HsnCode { get; set; }
}
