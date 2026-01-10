using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Procurement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Algora.Erp.Web.Pages.Procurement.PurchaseOrders;

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
    public decimal TotalValue { get; set; }
    public int OverdueOrders { get; set; }

    public List<Supplier> Suppliers { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalOrders = await _context.PurchaseOrders.CountAsync();
        PendingOrders = await _context.PurchaseOrders
            .CountAsync(p => p.Status == PurchaseOrderStatus.Pending || p.Status == PurchaseOrderStatus.Approved);
        TotalValue = await _context.PurchaseOrders.SumAsync(p => p.TotalAmount);
        OverdueOrders = await _context.PurchaseOrders
            .CountAsync(p => p.DueDate.HasValue && p.DueDate < DateTime.UtcNow &&
                           p.Status != PurchaseOrderStatus.Received &&
                           p.Status != PurchaseOrderStatus.Paid &&
                           p.Status != PurchaseOrderStatus.Cancelled);

        Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, Guid? supplierFilter, string? statusFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.PurchaseOrders
            .Include(p => p.Supplier)
            .Include(p => p.Lines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(p =>
                p.OrderNumber.ToLower().Contains(search) ||
                (p.Reference != null && p.Reference.ToLower().Contains(search)) ||
                p.Supplier.Name.ToLower().Contains(search));
        }

        if (supplierFilter.HasValue)
        {
            query = query.Where(p => p.SupplierId == supplierFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<PurchaseOrderStatus>(statusFilter, out var status))
        {
            query = query.Where(p => p.Status == status);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var orders = await query
            .OrderByDescending(p => p.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_PurchaseOrdersTableRows", new PurchaseOrdersTableViewModel
        {
            PurchaseOrders = orders,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
        var products = await _context.Products.Where(p => p.IsActive && p.IsPurchasable).ToListAsync();
        var warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();

        return Partial("_PurchaseOrderForm", new PurchaseOrderFormViewModel
        {
            IsEdit = false,
            Suppliers = suppliers,
            Products = products,
            Warehouses = warehouses
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var order = await _context.PurchaseOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (order == null)
            return NotFound();

        var suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
        var products = await _context.Products.Where(p => p.IsActive && p.IsPurchasable).ToListAsync();
        var warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();

        return Partial("_PurchaseOrderForm", new PurchaseOrderFormViewModel
        {
            IsEdit = true,
            PurchaseOrder = order,
            Suppliers = suppliers,
            Products = products,
            Warehouses = warehouses
        });
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var order = await _context.PurchaseOrders
            .Include(p => p.Supplier)
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (order == null)
            return NotFound();

        return Partial("_PurchaseOrderDetails", order);
    }

    public async Task<IActionResult> OnPostAsync(PurchaseOrderFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        PurchaseOrder? order;

        if (input.Id.HasValue)
        {
            order = await _context.PurchaseOrders
                .Include(p => p.Lines)
                .FirstOrDefaultAsync(p => p.Id == input.Id.Value);
            if (order == null)
                return NotFound();

            // Clear existing lines
            foreach (var line in order.Lines.ToList())
            {
                _context.PurchaseOrderLines.Remove(line);
            }
        }
        else
        {
            order = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = await GenerateOrderNumberAsync()
            };
            _context.PurchaseOrders.Add(order);
        }

        order.SupplierId = input.SupplierId;
        order.WarehouseId = input.WarehouseId;
        order.OrderDate = input.OrderDate;
        order.DueDate = input.DueDate;
        order.ExpectedDeliveryDate = input.ExpectedDeliveryDate;
        order.Status = input.Status;
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

                var line = new PurchaseOrderLine
                {
                    Id = Guid.NewGuid(),
                    PurchaseOrderId = order.Id,
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

                _context.PurchaseOrderLines.Add(line);
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

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, PurchaseOrderStatus status)
    {
        var order = await _context.PurchaseOrders
            .Include(p => p.Lines)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (order == null)
            return NotFound();

        var oldStatus = order.Status;
        order.Status = status;

        if (status == PurchaseOrderStatus.Approved)
        {
            // Set approved info - in real app, get from current user
            order.ApprovedAt = DateTime.UtcNow;
        }
        else if (status == PurchaseOrderStatus.Cancelled)
        {
            // Create cancellation log entry
            var cancellationLog = new CancellationLog
            {
                Id = Guid.NewGuid(),
                DocumentType = "PurchaseOrder",
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
                    SupplierName = order.Supplier?.Name,
                    order.TotalAmount,
                    TotalLines = order.Lines?.Count ?? 0
                }),
                Notes = $"Purchase order cancelled from status: {oldStatus}"
            };
            _context.CancellationLogs.Add(cancellationLog);
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var order = await _context.PurchaseOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (order == null)
            return NotFound();

        // Can only delete draft orders
        if (order.Status != PurchaseOrderStatus.Draft)
        {
            return BadRequest("Only draft orders can be deleted. Cancel the order instead.");
        }

        _context.PurchaseOrders.Remove(order);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var lastOrder = await _context.PurchaseOrders
            .IgnoreQueryFilters()
            .OrderByDescending(p => p.OrderNumber)
            .FirstOrDefaultAsync(p => p.OrderNumber.StartsWith("PO"));

        if (lastOrder == null)
            return $"PO{DateTime.UtcNow:yyyyMM}001";

        var lastNumber = int.Parse(lastOrder.OrderNumber.Substring(8));
        return $"PO{DateTime.UtcNow:yyyyMM}{(lastNumber + 1):D3}";
    }
}

public class PurchaseOrdersTableViewModel
{
    public List<PurchaseOrder> PurchaseOrders { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/Procurement/PurchaseOrders",
        Handler = "Table",
        HxTarget = "#tableContent",
        HxInclude = "#searchInput,#supplierFilter,#statusFilter,#pageSizeSelect"
    };
}

public class PurchaseOrderFormViewModel
{
    public bool IsEdit { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public List<Supplier> Suppliers { get; set; } = new();
    public List<Domain.Entities.Inventory.Product> Products { get; set; } = new();
    public List<Domain.Entities.Inventory.Warehouse> Warehouses { get; set; } = new();
}

public class PurchaseOrderFormInput
{
    public Guid? Id { get; set; }
    public Guid SupplierId { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Today;
    public DateTime? DueDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public string? ShippingAddress { get; set; }
    public string? ShippingMethod { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public List<PurchaseOrderLineInput>? Lines { get; set; }
}

public class PurchaseOrderLineInput
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public string? Notes { get; set; }
}
