using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Algora.Erp.Web.Pages.Sales.Orders;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingRevenue { get; set; }

    public List<Customer> Customers { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalOrders = await _context.SalesOrders.CountAsync();
        PendingOrders = await _context.SalesOrders
            .CountAsync(o => o.Status != SalesOrderStatus.Delivered &&
                           o.Status != SalesOrderStatus.Paid &&
                           o.Status != SalesOrderStatus.Cancelled);
        TotalRevenue = await _context.SalesOrders
            .Where(o => o.Status != SalesOrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount);
        PendingRevenue = await _context.SalesOrders
            .Where(o => o.Status != SalesOrderStatus.Delivered &&
                       o.Status != SalesOrderStatus.Paid &&
                       o.Status != SalesOrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount - o.AmountPaid);

        Customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, Guid? customerFilter, string? statusFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Lines)
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

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<SalesOrderStatus>(statusFilter, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_SalesOrdersTableRows", new SalesOrdersTableViewModel
        {
            SalesOrders = orders,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
        var products = await _context.Products.Where(p => p.IsActive && p.IsSellable).ToListAsync();

        return Partial("_SalesOrderForm", new SalesOrderFormViewModel
        {
            IsEdit = false,
            Customers = customers,
            Products = products
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        var customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
        var products = await _context.Products.Where(p => p.IsActive && p.IsSellable).ToListAsync();

        return Partial("_SalesOrderForm", new SalesOrderFormViewModel
        {
            IsEdit = true,
            SalesOrder = order,
            Customers = customers,
            Products = products
        });
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        return Partial("_SalesOrderDetails", order);
    }

    public async Task<IActionResult> OnPostAsync(SalesOrderFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        SalesOrder? order;

        if (input.Id.HasValue)
        {
            order = await _context.SalesOrders
                .Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.Id == input.Id.Value);
            if (order == null)
                return NotFound();

            // Clear existing lines
            foreach (var line in order.Lines.ToList())
            {
                _context.SalesOrderLines.Remove(line);
            }
        }
        else
        {
            order = new SalesOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = await GenerateOrderNumberAsync()
            };
            _context.SalesOrders.Add(order);
        }

        order.CustomerId = input.CustomerId;
        order.OrderDate = input.OrderDate;
        order.DueDate = input.DueDate;
        order.ExpectedDeliveryDate = input.ExpectedDeliveryDate;
        order.Status = input.Status;
        order.OrderType = input.OrderType;
        order.ShippingAddress = input.ShippingAddress;
        order.ShippingMethod = input.ShippingMethod;
        order.Currency = input.Currency;
        order.Reference = input.Reference;
        order.Notes = input.Notes;

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
                    SalesOrderId = order.Id,
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

        order.SubTotal = subTotal;
        order.TaxAmount = taxTotal;
        order.ShippingAmount = input.ShippingAmount;
        order.DiscountAmount = input.DiscountAmount;
        order.TotalAmount = subTotal + taxTotal + input.ShippingAmount - input.DiscountAmount;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, SalesOrderStatus status)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Lines)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
            return NotFound();

        var oldStatus = order.Status;
        order.Status = status;

        if (status == SalesOrderStatus.Cancelled)
        {
            // Create cancellation log entry
            var cancellationLog = new CancellationLog
            {
                Id = Guid.NewGuid(),
                DocumentType = "SalesOrder",
                DocumentId = order.Id,
                DocumentNumber = order.OrderNumber,
                CancelledAt = DateTime.UtcNow,
                CancelledBy = Guid.Empty,
                CancelledByName = User.Identity?.Name ?? "System",
                CancellationReason = "Cancelled by user",
                ReasonCategory = CancellationReasonCategory.Other,
                OriginalDocumentState = JsonSerializer.Serialize(new
                {
                    Status = oldStatus.ToString(),
                    order.OrderDate,
                    CustomerName = order.Customer?.Name,
                    order.TotalAmount,
                    order.AmountPaid,
                    TotalLines = order.Lines?.Count ?? 0
                }),
                FinancialReversed = order.AmountPaid > 0,
                FinancialReversalDetails = order.AmountPaid > 0
                    ? JsonSerializer.Serialize(new { order.AmountPaid, order.TotalAmount })
                    : null,
                Notes = $"Sales order cancelled from status: {oldStatus}"
            };
            _context.CancellationLogs.Add(cancellationLog);
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        if (order.Status != SalesOrderStatus.Draft)
        {
            return BadRequest("Only draft orders can be deleted. Cancel the order instead.");
        }

        _context.SalesOrders.Remove(order);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
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

public class SalesOrdersTableViewModel
{
    public List<SalesOrder> SalesOrders { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}

public class SalesOrderFormViewModel
{
    public bool IsEdit { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public List<Customer> Customers { get; set; } = new();
    public List<Domain.Entities.Inventory.Product> Products { get; set; } = new();
}

public class SalesOrderFormInput
{
    public Guid? Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Today;
    public DateTime? DueDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    public SalesOrderType OrderType { get; set; } = SalesOrderType.Standard;
    public string? ShippingAddress { get; set; }
    public string? ShippingMethod { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public List<SalesOrderLineInput>? Lines { get; set; }
}

public class SalesOrderLineInput
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public string? Notes { get; set; }
}
