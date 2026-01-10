using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Algora.Erp.Web.Pages.Sales.Quotations;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalQuotations { get; set; }
    public int PendingQuotations { get; set; }
    public int AcceptedQuotations { get; set; }
    public decimal TotalValue { get; set; }

    public List<Customer> Customers { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Count all quotations (OrderType = Quote)
        TotalQuotations = await _context.SalesOrders
            .CountAsync(o => o.OrderType == SalesOrderType.Quote);

        PendingQuotations = await _context.SalesOrders
            .CountAsync(o => o.OrderType == SalesOrderType.Quote &&
                           o.Status == SalesOrderStatus.Draft);

        AcceptedQuotations = await _context.SalesOrders
            .CountAsync(o => o.OrderType == SalesOrderType.Quote &&
                           o.Status == SalesOrderStatus.Confirmed);

        TotalValue = await _context.SalesOrders
            .Where(o => o.OrderType == SalesOrderType.Quote &&
                       o.Status != SalesOrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount);

        Customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, Guid? customerFilter, string? statusFilter, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .Where(o => o.OrderType == SalesOrderType.Quote)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(search) ||
                (o.Reference != null && o.Reference.ToLower().Contains(search)) ||
                o.Customer.Name.ToLower().Contains(search));
        }

        if (customerFilter.HasValue)
        {
            query = query.Where(o => o.CustomerId == customerFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            query = statusFilter switch
            {
                "Draft" => query.Where(o => o.Status == SalesOrderStatus.Draft),
                "Sent" => query.Where(o => o.Status == SalesOrderStatus.Confirmed),
                "Accepted" => query.Where(o => o.Status == SalesOrderStatus.Paid),
                "Rejected" => query.Where(o => o.Status == SalesOrderStatus.Cancelled),
                "Expired" => query.Where(o => o.Status == SalesOrderStatus.OnHold),
                _ => query
            };
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var quotations = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_QuotationsTableRows", new QuotationsTableViewModel
        {
            Quotations = quotations,
            Page = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
        var products = await _context.Products.Where(p => p.IsActive && p.IsSellable).ToListAsync();

        return Partial("_QuotationForm", new QuotationFormViewModel
        {
            IsEdit = false,
            Customers = customers,
            Products = products
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var quotation = await _context.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id && o.OrderType == SalesOrderType.Quote);

        if (quotation == null)
            return NotFound();

        var customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
        var products = await _context.Products.Where(p => p.IsActive && p.IsSellable).ToListAsync();

        return Partial("_QuotationForm", new QuotationFormViewModel
        {
            IsEdit = true,
            Quotation = quotation,
            Customers = customers,
            Products = products
        });
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var quotation = await _context.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id && o.OrderType == SalesOrderType.Quote);

        if (quotation == null)
            return NotFound();

        return Partial("_QuotationDetails", quotation);
    }

    public async Task<IActionResult> OnPostAsync(QuotationFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        SalesOrder? quotation;

        if (input.Id.HasValue)
        {
            quotation = await _context.SalesOrders
                .Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.Id == input.Id.Value);
            if (quotation == null)
                return NotFound();

            // Clear existing lines
            foreach (var line in quotation.Lines.ToList())
            {
                _context.SalesOrderLines.Remove(line);
            }
        }
        else
        {
            quotation = new SalesOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = await GenerateQuoteNumberAsync(),
                OrderType = SalesOrderType.Quote
            };
            _context.SalesOrders.Add(quotation);
        }

        quotation.CustomerId = input.CustomerId;
        quotation.OrderDate = input.QuoteDate;
        quotation.DueDate = input.ValidUntil;
        quotation.Status = input.Status;
        quotation.OrderType = SalesOrderType.Quote;
        quotation.Currency = input.Currency;
        quotation.Reference = input.Reference;
        quotation.Notes = input.Notes;

        // Add lines
        decimal subTotal = 0;
        decimal taxTotal = 0;
        int lineNumber = 1;

        if (input.Lines != null)
        {
            foreach (var lineInput in input.Lines.Where(l => l.ProductId != Guid.Empty))
            {
                var product = await _context.Products.FindAsync(lineInput.ProductId);
                var lineTotal = lineInput.Quantity * lineInput.UnitPrice * (1 - lineInput.DiscountPercent / 100);
                var lineTax = lineTotal * lineInput.TaxPercent / 100;

                var line = new SalesOrderLine
                {
                    Id = Guid.NewGuid(),
                    SalesOrderId = quotation.Id,
                    ProductId = lineInput.ProductId,
                    ProductName = product?.Name,
                    ProductSku = product?.Sku,
                    LineNumber = lineNumber++,
                    Quantity = lineInput.Quantity,
                    UnitOfMeasure = product?.UnitOfMeasure,
                    UnitPrice = lineInput.UnitPrice,
                    DiscountPercent = lineInput.DiscountPercent,
                    DiscountAmount = lineInput.Quantity * lineInput.UnitPrice * lineInput.DiscountPercent / 100,
                    TaxPercent = lineInput.TaxPercent,
                    TaxAmount = lineTax,
                    LineTotal = lineTotal + lineTax,
                    Notes = lineInput.Notes
                };

                _context.SalesOrderLines.Add(line);
                subTotal += lineTotal;
                taxTotal += lineTax;
            }
        }

        quotation.SubTotal = subTotal;
        quotation.TaxAmount = taxTotal;
        quotation.DiscountAmount = input.DiscountAmount;
        quotation.TotalAmount = subTotal + taxTotal - input.DiscountAmount;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, string status)
    {
        var quotation = await _context.SalesOrders.FindAsync(id);
        if (quotation == null)
            return NotFound();

        quotation.Status = status switch
        {
            "Sent" => SalesOrderStatus.Confirmed,
            "Accepted" => SalesOrderStatus.Paid,
            "Rejected" => SalesOrderStatus.Cancelled,
            "Expired" => SalesOrderStatus.OnHold,
            _ => SalesOrderStatus.Draft
        };

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostConvertToOrderAsync(Guid id)
    {
        var quotation = await _context.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id && o.OrderType == SalesOrderType.Quote);

        if (quotation == null)
            return NotFound();

        // Create new sales order from quotation
        var salesOrder = new SalesOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = await GenerateOrderNumberAsync(),
            OrderDate = DateTime.UtcNow,
            DueDate = quotation.DueDate,
            CustomerId = quotation.CustomerId,
            Status = SalesOrderStatus.Draft,
            OrderType = SalesOrderType.Standard,
            ShippingAddress = quotation.ShippingAddress,
            SubTotal = quotation.SubTotal,
            DiscountAmount = quotation.DiscountAmount,
            TaxAmount = quotation.TaxAmount,
            TotalAmount = quotation.TotalAmount,
            Currency = quotation.Currency,
            Reference = $"From Quote: {quotation.OrderNumber}",
            Notes = quotation.Notes
        };

        _context.SalesOrders.Add(salesOrder);

        // Copy lines
        foreach (var quoteLine in quotation.Lines)
        {
            var line = new SalesOrderLine
            {
                Id = Guid.NewGuid(),
                SalesOrderId = salesOrder.Id,
                ProductId = quoteLine.ProductId,
                ProductName = quoteLine.ProductName,
                ProductSku = quoteLine.ProductSku,
                LineNumber = quoteLine.LineNumber,
                Quantity = quoteLine.Quantity,
                UnitOfMeasure = quoteLine.UnitOfMeasure,
                UnitPrice = quoteLine.UnitPrice,
                DiscountPercent = quoteLine.DiscountPercent,
                DiscountAmount = quoteLine.DiscountAmount,
                TaxPercent = quoteLine.TaxPercent,
                TaxAmount = quoteLine.TaxAmount,
                LineTotal = quoteLine.LineTotal,
                Notes = quoteLine.Notes
            };
            _context.SalesOrderLines.Add(line);
        }

        // Mark quotation as accepted
        quotation.Status = SalesOrderStatus.Paid;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var quotation = await _context.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id && o.OrderType == SalesOrderType.Quote);

        if (quotation == null)
            return NotFound();

        if (quotation.Status != SalesOrderStatus.Draft)
        {
            return BadRequest("Only draft quotations can be deleted.");
        }

        _context.SalesOrders.Remove(quotation);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnGetExportAsync(string? search, Guid? customerFilter, string? statusFilter)
    {
        var query = _context.SalesOrders
            .Include(o => o.Customer)
            .Where(o => o.OrderType == SalesOrderType.Quote)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(search) ||
                o.Customer.Name.ToLower().Contains(search));
        }

        if (customerFilter.HasValue)
        {
            query = query.Where(o => o.CustomerId == customerFilter.Value);
        }

        var quotations = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Quote #,Customer,Date,Valid Until,Amount,Status");

        foreach (var q in quotations)
        {
            var status = q.Status switch
            {
                SalesOrderStatus.Draft => "Draft",
                SalesOrderStatus.Confirmed => "Sent",
                SalesOrderStatus.Paid => "Accepted",
                SalesOrderStatus.Cancelled => "Rejected",
                SalesOrderStatus.OnHold => "Expired",
                _ => q.Status.ToString()
            };
            csv.AppendLine($"\"{q.OrderNumber}\",\"{q.Customer?.Name}\",{q.OrderDate:yyyy-MM-dd},{q.DueDate:yyyy-MM-dd},{q.TotalAmount:F2},{status}");
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"quotations-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private async Task<string> GenerateQuoteNumberAsync()
    {
        var lastQuote = await _context.SalesOrders
            .IgnoreQueryFilters()
            .Where(o => o.OrderType == SalesOrderType.Quote)
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync(o => o.OrderNumber.StartsWith("QT"));

        if (lastQuote == null)
            return $"QT{DateTime.UtcNow:yyyyMM}001";

        var lastNumber = int.Parse(lastQuote.OrderNumber.Substring(8));
        return $"QT{DateTime.UtcNow:yyyyMM}{(lastNumber + 1):D3}";
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var lastOrder = await _context.SalesOrders
            .IgnoreQueryFilters()
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync(o => o.OrderNumber.StartsWith("SO"));

        if (lastOrder == null)
            return $"SO{DateTime.UtcNow:yyyyMM}001";

        var lastNumber = int.Parse(lastOrder.OrderNumber.Substring(8));
        return $"SO{DateTime.UtcNow:yyyyMM}{(lastNumber + 1):D3}";
    }
}

public class QuotationsTableViewModel
{
    public List<SalesOrder> Quotations { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/Sales/Quotations",
        Handler = "Table",
        HxTarget = "#quotationsTableBody",
        HxInclude = "#searchInput,#customerFilter,#statusFilter"
    };
}

public class QuotationFormViewModel
{
    public bool IsEdit { get; set; }
    public SalesOrder? Quotation { get; set; }
    public List<Customer> Customers { get; set; } = new();
    public List<Domain.Entities.Inventory.Product> Products { get; set; } = new();
}

public class QuotationFormInput
{
    public Guid? Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime QuoteDate { get; set; } = DateTime.Today;
    public DateTime? ValidUntil { get; set; }
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    public string Currency { get; set; } = "USD";
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public decimal DiscountAmount { get; set; }
    public List<QuotationLineInput>? Lines { get; set; }
}

public class QuotationLineInput
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public string? Notes { get; set; }
}
