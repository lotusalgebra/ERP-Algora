using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Algora.Erp.Domain.Entities.Procurement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Procurement.PurchaseInvoices;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    // Stats
    public int TotalInvoices { get; set; }
    public int PendingCount { get; set; }
    public int OverdueCount { get; set; }
    public decimal TotalOutstanding { get; set; }

    // Dropdowns
    public List<Supplier> Suppliers { get; set; } = new();
    public List<PurchaseOrder> PurchaseOrders { get; set; } = new();
    public List<GoodsReceiptNote> GoodsReceiptNotes { get; set; } = new();

    public async Task OnGetAsync()
    {
        var purchaseInvoices = await _context.Invoices
            .Where(i => i.Type == InvoiceType.PurchaseInvoice && !i.IsDeleted)
            .ToListAsync();

        TotalInvoices = purchaseInvoices.Count;
        PendingCount = purchaseInvoices.Count(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Sent);
        OverdueCount = purchaseInvoices.Count(i => i.Status == InvoiceStatus.Overdue);
        TotalOutstanding = purchaseInvoices.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Void && i.Status != InvoiceStatus.Cancelled)
            .Sum(i => i.BalanceDue);

        Suppliers = await _context.Suppliers
            .Where(s => !s.IsDeleted && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(
        string? search,
        string? statusFilter,
        Guid? supplierFilter,
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.Invoices
            .Include(i => i.Lines)
            .Where(i => i.Type == InvoiceType.PurchaseInvoice && !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(search) ||
                (i.Reference != null && i.Reference.ToLower().Contains(search)) ||
                (i.BillingName != null && i.BillingName.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<InvoiceStatus>(statusFilter, out var status))
        {
            query = query.Where(i => i.Status == status);
        }

        if (supplierFilter.HasValue)
        {
            query = query.Where(i => i.SupplierId == supplierFilter.Value);
        }

        var totalRecords = await query.CountAsync();
        var invoices = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get supplier names
        var supplierIds = invoices.Where(i => i.SupplierId.HasValue).Select(i => i.SupplierId!.Value).Distinct().ToList();
        var suppliers = await _context.Suppliers
            .Where(s => supplierIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name);

        var viewModel = new PurchaseInvoicesTableViewModel
        {
            Invoices = invoices,
            SupplierNames = suppliers,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        };

        return Partial("_PurchaseInvoiceTableRows", viewModel);
    }

    public async Task<IActionResult> OnGetCreateFormAsync(Guid? grnId = null, Guid? poId = null)
    {
        var viewModel = new PurchaseInvoiceFormViewModel
        {
            IsEdit = false,
            Suppliers = await _context.Suppliers
                .Where(s => !s.IsDeleted && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync(),
            PurchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Where(po => !po.IsDeleted &&
                    (po.Status == PurchaseOrderStatus.Received ||
                     po.Status == PurchaseOrderStatus.PartiallyReceived ||
                     po.Status == PurchaseOrderStatus.PartiallyInvoiced))
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync(),
            GoodsReceiptNotes = await _context.GoodsReceiptNotes
                .Include(g => g.Supplier)
                .Where(g => !g.IsDeleted &&
                    (g.Status == GoodsReceiptStatus.Accepted ||
                     g.Status == GoodsReceiptStatus.PartiallyAccepted))
                .OrderByDescending(g => g.GrnDate)
                .ToListAsync()
        };

        // If creating from GRN, pre-populate
        if (grnId.HasValue)
        {
            var grn = await _context.GoodsReceiptNotes
                .Include(g => g.Supplier)
                .Include(g => g.Lines)
                .FirstOrDefaultAsync(g => g.Id == grnId.Value);

            if (grn != null)
            {
                viewModel.SelectedGrn = grn;
            }
        }
        else if (poId.HasValue)
        {
            var po = await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Lines)
                .FirstOrDefaultAsync(p => p.Id == poId.Value);

            if (po != null)
            {
                viewModel.SelectedPurchaseOrder = po;
            }
        }

        return Partial("_PurchaseInvoiceForm", viewModel);
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id && i.Type == InvoiceType.PurchaseInvoice);

        if (invoice == null)
            return NotFound();

        var viewModel = new PurchaseInvoiceFormViewModel
        {
            IsEdit = true,
            Invoice = invoice,
            Suppliers = await _context.Suppliers
                .Where(s => !s.IsDeleted && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync(),
            PurchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Where(po => !po.IsDeleted)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync(),
            GoodsReceiptNotes = await _context.GoodsReceiptNotes
                .Include(g => g.Supplier)
                .Where(g => !g.IsDeleted)
                .OrderByDescending(g => g.GrnDate)
                .ToListAsync()
        };

        return Partial("_PurchaseInvoiceForm", viewModel);
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id && i.Type == InvoiceType.PurchaseInvoice);

        if (invoice == null)
            return NotFound();

        // Get supplier name
        string? supplierName = null;
        if (invoice.SupplierId.HasValue)
        {
            var supplier = await _context.Suppliers.FindAsync(invoice.SupplierId.Value);
            supplierName = supplier?.Name;
        }

        ViewData["SupplierName"] = supplierName;
        return Partial("_PurchaseInvoiceDetails", invoice);
    }

    public async Task<IActionResult> OnPostAsync(
        Guid? id,
        Guid supplierId,
        Guid? purchaseOrderId,
        Guid? grnId,
        DateTime invoiceDate,
        DateTime dueDate,
        string? reference,
        string? notes,
        List<InvoiceLineInput> lines)
    {
        Invoice invoice;
        bool isNew = !id.HasValue;

        if (isNew)
        {
            invoice = new Invoice
            {
                Type = InvoiceType.PurchaseInvoice,
                InvoiceNumber = await GenerateInvoiceNumber(),
                SupplierId = supplierId,
                PurchaseOrderId = purchaseOrderId,
                InvoiceDate = invoiceDate,
                DueDate = dueDate,
                Reference = reference,
                Notes = notes,
                Status = InvoiceStatus.Pending
            };

            _context.Invoices.Add(invoice);
        }
        else
        {
            invoice = await _context.Invoices
                .Include(i => i.Lines)
                .FirstOrDefaultAsync(i => i.Id == id.Value && i.Type == InvoiceType.PurchaseInvoice);

            if (invoice == null)
                return NotFound();

            if (invoice.Status != InvoiceStatus.Draft && invoice.Status != InvoiceStatus.Pending)
            {
                return BadRequest("Cannot edit invoice in current status");
            }

            invoice.SupplierId = supplierId;
            invoice.PurchaseOrderId = purchaseOrderId;
            invoice.InvoiceDate = invoiceDate;
            invoice.DueDate = dueDate;
            invoice.Reference = reference;
            invoice.Notes = notes;

            // Remove existing lines
            foreach (var line in invoice.Lines.ToList())
            {
                line.IsDeleted = true;
            }
        }

        // Add lines
        decimal subTotal = 0;
        decimal taxTotal = 0;
        int lineNum = 1;

        foreach (var lineInput in lines.Where(l => l.ProductId.HasValue))
        {
            var product = await _context.Products.FindAsync(lineInput.ProductId!.Value);

            var lineTotal = lineInput.Quantity * lineInput.UnitPrice;
            var discountAmt = lineTotal * (lineInput.DiscountPercent / 100);
            var taxableAmount = lineTotal - discountAmt;
            var taxAmt = taxableAmount * (lineInput.TaxPercent / 100);

            var invoiceLine = new InvoiceLine
            {
                InvoiceId = invoice.Id,
                LineNumber = lineNum++,
                ProductId = lineInput.ProductId,
                ProductCode = product?.Sku,
                Description = lineInput.Description ?? product?.Name ?? "",
                Quantity = lineInput.Quantity,
                UnitOfMeasure = lineInput.UnitOfMeasure,
                UnitPrice = lineInput.UnitPrice,
                DiscountPercent = lineInput.DiscountPercent,
                DiscountAmount = discountAmt,
                TaxPercent = lineInput.TaxPercent,
                TaxAmount = taxAmt,
                LineTotal = taxableAmount + taxAmt
            };

            _context.InvoiceLines.Add(invoiceLine);
            subTotal += taxableAmount;
            taxTotal += taxAmt;
        }

        invoice.SubTotal = subTotal;
        invoice.TaxAmount = taxTotal;
        invoice.TotalAmount = subTotal + taxTotal;
        invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;

        // Get supplier billing info
        var supplierEntity = await _context.Suppliers.FindAsync(supplierId);
        if (supplierEntity != null)
        {
            invoice.BillingName = supplierEntity.Name;
            invoice.BillingAddress = supplierEntity.Address;
            invoice.BillingCity = supplierEntity.City;
            invoice.BillingState = supplierEntity.State;
            invoice.BillingPostalCode = supplierEntity.PostalCode;
            invoice.BillingCountry = supplierEntity.Country;
            invoice.PaymentTermDays = supplierEntity.PaymentTermsDays;
            invoice.Currency = supplierEntity.Currency;
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        // Update PO status if linked
        if (purchaseOrderId.HasValue)
        {
            await UpdatePurchaseOrderStatus(purchaseOrderId.Value);
        }

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, string status)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null || invoice.Type != InvoiceType.PurchaseInvoice)
            return NotFound();

        if (Enum.TryParse<InvoiceStatus>(status, out var newStatus))
        {
            invoice.Status = newStatus;

            if (newStatus == InvoiceStatus.Sent)
            {
                invoice.SentAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(CancellationToken.None);
        }

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostRecordPaymentAsync(
        Guid invoiceId,
        DateTime paymentDate,
        decimal amount,
        PaymentMethod paymentMethod,
        string? reference,
        string? notes)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.Type == InvoiceType.PurchaseInvoice);

        if (invoice == null)
            return NotFound();

        if (amount > invoice.BalanceDue)
        {
            return BadRequest("Payment amount exceeds balance due");
        }

        var payment = new InvoicePayment
        {
            InvoiceId = invoiceId,
            PaymentNumber = await GeneratePaymentNumber(),
            PaymentDate = paymentDate,
            Amount = amount,
            PaymentMethod = paymentMethod,
            Reference = reference,
            Notes = notes
        };

        _context.InvoicePayments.Add(payment);

        invoice.PaidAmount += amount;
        invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;

        if (invoice.BalanceDue <= 0)
        {
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidDate = paymentDate;
        }
        else if (invoice.PaidAmount > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        await _context.SaveChangesAsync(CancellationToken.None);
        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id && i.Type == InvoiceType.PurchaseInvoice);

        if (invoice == null)
            return NotFound();

        if (invoice.Status != InvoiceStatus.Draft && invoice.Status != InvoiceStatus.Pending)
        {
            return BadRequest("Can only delete draft or pending invoices");
        }

        invoice.IsDeleted = true;
        foreach (var line in invoice.Lines)
        {
            line.IsDeleted = true;
        }

        await _context.SaveChangesAsync(CancellationToken.None);
        return await OnGetTableAsync(null, null, null);
    }

    private async Task<string> GenerateInvoiceNumber()
    {
        var prefix = $"PI{DateTime.UtcNow:yyyyMM}";
        var lastInvoice = await _context.Invoices
            .Where(i => i.Type == InvoiceType.PurchaseInvoice && i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        int nextNum = 1;
        if (lastInvoice != null && lastInvoice.InvoiceNumber.Length > prefix.Length)
        {
            if (int.TryParse(lastInvoice.InvoiceNumber.Substring(prefix.Length), out int lastNum))
            {
                nextNum = lastNum + 1;
            }
        }

        return $"{prefix}{nextNum:D4}";
    }

    private async Task<string> GeneratePaymentNumber()
    {
        var prefix = $"PAY{DateTime.UtcNow:yyyyMM}";
        var lastPayment = await _context.InvoicePayments
            .Where(p => p.PaymentNumber.StartsWith(prefix))
            .OrderByDescending(p => p.PaymentNumber)
            .FirstOrDefaultAsync();

        int nextNum = 1;
        if (lastPayment != null && lastPayment.PaymentNumber.Length > prefix.Length)
        {
            if (int.TryParse(lastPayment.PaymentNumber.Substring(prefix.Length), out int lastNum))
            {
                nextNum = lastNum + 1;
            }
        }

        return $"{prefix}{nextNum:D4}";
    }

    private async Task UpdatePurchaseOrderStatus(Guid purchaseOrderId)
    {
        var po = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
        if (po == null) return;

        var invoices = await _context.Invoices
            .Where(i => i.PurchaseOrderId == purchaseOrderId &&
                        i.Type == InvoiceType.PurchaseInvoice &&
                        !i.IsDeleted)
            .ToListAsync();

        var totalInvoiced = invoices.Sum(i => i.TotalAmount);

        if (totalInvoiced >= po.TotalAmount)
        {
            po.Status = PurchaseOrderStatus.Invoiced;
        }
        else if (totalInvoiced > 0)
        {
            po.Status = PurchaseOrderStatus.PartiallyInvoiced;
        }
    }
}

public class PurchaseInvoicesTableViewModel
{
    public List<Invoice> Invoices { get; set; } = new();
    public Dictionary<Guid, string> SupplierNames { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}

public class PurchaseInvoiceFormViewModel
{
    public bool IsEdit { get; set; }
    public Invoice? Invoice { get; set; }
    public List<Supplier> Suppliers { get; set; } = new();
    public List<PurchaseOrder> PurchaseOrders { get; set; } = new();
    public List<GoodsReceiptNote> GoodsReceiptNotes { get; set; } = new();
    public GoodsReceiptNote? SelectedGrn { get; set; }
    public PurchaseOrder? SelectedPurchaseOrder { get; set; }
}

public class InvoiceLineInput
{
    public Guid? ProductId { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
}
