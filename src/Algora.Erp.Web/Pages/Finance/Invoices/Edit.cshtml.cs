using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Sales;
using Algora.Erp.Domain.Entities.Settings;
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
    public List<GstSlab> GstSlabs { get; set; } = new();
    public List<OfficeLocation> Locations { get; set; } = new();
    public List<IndianState> States { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .Include(i => i.FromLocation)
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

        // Get customer state ID if we can match it
        Guid? customerStateId = null;
        if (!string.IsNullOrEmpty(invoice.CustomerStateCode))
        {
            var customerState = await _context.IndianStates
                .FirstOrDefaultAsync(s => s.ShortName == invoice.CustomerStateCode);
            customerStateId = customerState?.Id;
        }

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
            // GST Fields
            FromLocationId = invoice.FromLocationId,
            CustomerGstin = invoice.CustomerGstin,
            CustomerStateId = customerStateId,
            IsInterState = invoice.IsInterState,
            Lines = invoice.Lines.OrderBy(l => l.LineNumber).Select(l => new InvoiceLineInput
            {
                ProductId = l.ProductId,
                ProductCode = l.ProductCode,
                Description = l.Description,
                Quantity = l.Quantity,
                UnitOfMeasure = l.UnitOfMeasure,
                UnitPrice = l.UnitPrice,
                DiscountPercent = l.DiscountPercent,
                TaxPercent = l.TaxPercent,
                // GST Fields
                GstSlabId = l.GstSlabId,
                HsnCode = l.HsnCode
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

        // Determine if inter-state transaction
        var fromLocation = Input.FromLocationId.HasValue
            ? await _context.OfficeLocations.Include(l => l.State).FirstOrDefaultAsync(l => l.Id == Input.FromLocationId)
            : null;
        var customerState = Input.CustomerStateId.HasValue
            ? await _context.IndianStates.FirstOrDefaultAsync(s => s.Id == Input.CustomerStateId)
            : null;

        var isInterState = fromLocation?.StateId != Input.CustomerStateId && Input.CustomerStateId.HasValue;

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
        invoice.BillingState = customerState?.Name ?? Input.BillingState;
        invoice.BillingPostalCode = Input.BillingPostalCode;
        invoice.BillingCountry = Input.BillingCountry;
        invoice.Reference = Input.Reference;
        invoice.Notes = Input.Notes;
        invoice.InternalNotes = Input.InternalNotes;
        invoice.ModifiedAt = DateTime.UtcNow;
        // GST Fields
        invoice.FromLocationId = Input.FromLocationId;
        invoice.CustomerGstin = Input.CustomerGstin;
        invoice.CustomerStateCode = customerState?.ShortName;
        invoice.IsInterState = isInterState;

        // Remove existing lines
        _context.InvoiceLines.RemoveRange(invoice.Lines);

        // Calculate totals and add new lines
        decimal subtotal = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;
        decimal totalCgst = 0;
        decimal totalSgst = 0;
        decimal totalIgst = 0;

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

            _context.InvoiceLines.Add(line);

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
