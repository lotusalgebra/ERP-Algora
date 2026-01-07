using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Dispatch;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Dispatch.DeliveryChallans;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalChallans { get; set; }
    public int PendingDispatch { get; set; }
    public int DispatchedToday { get; set; }
    public int DeliveredThisWeek { get; set; }

    public List<Customer> Customers { get; set; } = new();
    public List<Warehouse> Warehouses { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalChallans = await _context.DeliveryChallans.CountAsync();
        PendingDispatch = await _context.DeliveryChallans
            .CountAsync(d => d.Status == DeliveryChallanStatus.Confirmed);
        DispatchedToday = await _context.DeliveryChallans
            .CountAsync(d => d.DispatchedAt.HasValue && d.DispatchedAt.Value.Date == DateTime.UtcNow.Date);
        DeliveredThisWeek = await _context.DeliveryChallans
            .CountAsync(d => d.Status == DeliveryChallanStatus.Delivered &&
                            d.DeliveredAt.HasValue && d.DeliveredAt >= DateTime.UtcNow.AddDays(-7));

        Customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
        Warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, Guid? customerFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.DeliveryChallans
            .Include(d => d.Lines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(d =>
                d.ChallanNumber.ToLower().Contains(search) ||
                d.CustomerName.ToLower().Contains(search) ||
                (d.VehicleNumber != null && d.VehicleNumber.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<DeliveryChallanStatus>(statusFilter, out var status))
        {
            query = query.Where(d => d.Status == status);
        }

        if (customerFilter.HasValue)
        {
            query = query.Where(d => d.CustomerId == customerFilter.Value);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var challans = await query
            .OrderByDescending(d => d.ChallanDate)
            .ThenByDescending(d => d.ChallanNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_DeliveryChallansTableRows", new DeliveryChallansTableViewModel
        {
            DeliveryChallans = challans,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync(Guid? salesOrderId = null)
    {
        var customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
        var warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
        var products = await _context.Products.Where(p => p.IsActive).ToListAsync();

        // Get sales orders ready for dispatch
        var salesOrders = await _context.SalesOrders
            .Where(s => s.Status == SalesOrderStatus.Confirmed || s.Status == SalesOrderStatus.PartiallyShipped)
            .Include(s => s.Lines)
            .Include(s => s.Customer)
            .OrderByDescending(s => s.OrderDate)
            .ToListAsync();

        SalesOrder? selectedSo = null;
        if (salesOrderId.HasValue)
        {
            selectedSo = await _context.SalesOrders
                .Include(s => s.Lines)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == salesOrderId.Value);
        }

        return Partial("_DeliveryChallanForm", new DeliveryChallanFormViewModel
        {
            IsEdit = false,
            Customers = customers,
            Warehouses = warehouses,
            Products = products,
            SalesOrders = salesOrders,
            SelectedSalesOrder = selectedSo
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var challan = await _context.DeliveryChallans
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (challan == null)
            return NotFound();

        var customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
        var warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
        var products = await _context.Products.Where(p => p.IsActive).ToListAsync();

        return Partial("_DeliveryChallanForm", new DeliveryChallanFormViewModel
        {
            IsEdit = true,
            DeliveryChallan = challan,
            Customers = customers,
            Warehouses = warehouses,
            Products = products
        });
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var challan = await _context.DeliveryChallans
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (challan == null)
            return NotFound();

        return Partial("_DeliveryChallanDetails", challan);
    }

    public async Task<IActionResult> OnPostAsync(DeliveryChallanFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        DeliveryChallan? challan;

        if (input.Id.HasValue)
        {
            challan = await _context.DeliveryChallans
                .Include(d => d.Lines)
                .FirstOrDefaultAsync(d => d.Id == input.Id.Value);
            if (challan == null)
                return NotFound();

            // Clear existing lines
            foreach (var line in challan.Lines.ToList())
            {
                _context.DeliveryChallanLines.Remove(line);
            }
        }
        else
        {
            challan = new DeliveryChallan
            {
                Id = Guid.NewGuid(),
                ChallanNumber = await GenerateChallanNumberAsync()
            };
            _context.DeliveryChallans.Add(challan);
        }

        // Get customer name
        var customer = await _context.Customers.FindAsync(input.CustomerId);

        challan.ChallanDate = input.ChallanDate;
        challan.SalesOrderId = input.SalesOrderId;
        challan.CustomerId = input.CustomerId;
        challan.CustomerName = customer?.Name ?? "Unknown";
        challan.WarehouseId = input.WarehouseId;
        challan.VehicleNumber = input.VehicleNumber;
        challan.DriverName = input.DriverName;
        challan.DriverPhone = input.DriverPhone;
        challan.TransportMode = input.TransportMode;
        challan.TransporterName = input.TransporterName;
        challan.ShipToName = input.ShipToName;
        challan.ShipToAddress1 = input.ShipToAddress1;
        challan.ShipToAddress2 = input.ShipToAddress2;
        challan.ShipToCity = input.ShipToCity;
        challan.ShipToState = input.ShipToState;
        challan.ShipToPostalCode = input.ShipToPostalCode;
        challan.ShipToCountry = input.ShipToCountry;
        challan.ShipToPhone = input.ShipToPhone;
        challan.Notes = input.Notes;

        // Add lines
        decimal totalQty = 0;
        int lineNumber = 1;

        if (input.Lines != null)
        {
            foreach (var lineInput in input.Lines.Where(l => l.ProductId != Guid.Empty))
            {
                var product = await _context.Products.FindAsync(lineInput.ProductId);

                var line = new DeliveryChallanLine
                {
                    Id = Guid.NewGuid(),
                    DeliveryChallanId = challan.Id,
                    LineNumber = lineNumber++,
                    SalesOrderLineId = lineInput.SalesOrderLineId,
                    ProductId = lineInput.ProductId,
                    ProductName = product?.Name ?? lineInput.ProductName ?? "Unknown",
                    ProductSku = product?.Sku ?? lineInput.ProductSku ?? "",
                    Quantity = lineInput.Quantity,
                    UnitOfMeasure = lineInput.UnitOfMeasure ?? "EA",
                    Notes = lineInput.Notes
                };

                _context.DeliveryChallanLines.Add(line);
                totalQty += lineInput.Quantity;
            }
        }

        challan.TotalQuantity = totalQty;
        challan.TotalPackages = input.TotalPackages;
        challan.TotalWeight = input.TotalWeight;
        challan.WeightUnit = input.WeightUnit ?? "KG";

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, DeliveryChallanStatus status)
    {
        var challan = await _context.DeliveryChallans
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (challan == null)
            return NotFound();

        var oldStatus = challan.Status;
        challan.Status = status;

        if (status == DeliveryChallanStatus.Dispatched && oldStatus != DeliveryChallanStatus.Dispatched)
        {
            challan.DispatchedAt = DateTime.UtcNow;

            // Deduct stock
            await DeductStockAsync(challan);
        }
        else if (status == DeliveryChallanStatus.Delivered)
        {
            challan.DeliveredAt = DateTime.UtcNow;

            // Update sales order if linked
            if (challan.SalesOrderId.HasValue)
            {
                await UpdateSalesOrderStatusAsync(challan.SalesOrderId.Value);
            }
        }
        else if (status == DeliveryChallanStatus.Cancelled && oldStatus == DeliveryChallanStatus.Dispatched)
        {
            // Reverse stock if cancelling a dispatched challan
            await ReverseStockAsync(challan);
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var challan = await _context.DeliveryChallans
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (challan == null)
            return NotFound();

        if (challan.Status != DeliveryChallanStatus.Draft)
        {
            return BadRequest("Only draft challans can be deleted. Cancel the challan instead.");
        }

        _context.DeliveryChallans.Remove(challan);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    private async Task<string> GenerateChallanNumberAsync()
    {
        var lastChallan = await _context.DeliveryChallans
            .IgnoreQueryFilters()
            .OrderByDescending(d => d.ChallanNumber)
            .FirstOrDefaultAsync(d => d.ChallanNumber.StartsWith("DC"));

        if (lastChallan == null)
            return $"DC{DateTime.UtcNow:yyyyMM}001";

        var lastNumber = int.Parse(lastChallan.ChallanNumber.Substring(8));
        return $"DC{DateTime.UtcNow:yyyyMM}{(lastNumber + 1):D3}";
    }

    private async Task DeductStockAsync(DeliveryChallan challan)
    {
        foreach (var line in challan.Lines)
        {
            var stockLevel = await _context.StockLevels
                .FirstOrDefaultAsync(s => s.ProductId == line.ProductId && s.WarehouseId == challan.WarehouseId);

            if (stockLevel != null)
            {
                stockLevel.QuantityOnHand -= line.Quantity;

                // Create stock movement
                var movement = new StockMovement
                {
                    Id = Guid.NewGuid(),
                    ProductId = line.ProductId,
                    WarehouseId = challan.WarehouseId,
                    MovementType = StockMovementType.Issue,
                    Quantity = -line.Quantity,
                    SourceDocumentType = "DeliveryChallan",
                    SourceDocumentId = challan.Id,
                    SourceDocumentNumber = challan.ChallanNumber,
                    MovementDate = DateTime.UtcNow,
                    Notes = $"Dispatched to customer: {challan.CustomerName}"
                };
                _context.StockMovements.Add(movement);
            }
        }
    }

    private async Task ReverseStockAsync(DeliveryChallan challan)
    {
        foreach (var line in challan.Lines)
        {
            var stockLevel = await _context.StockLevels
                .FirstOrDefaultAsync(s => s.ProductId == line.ProductId && s.WarehouseId == challan.WarehouseId);

            if (stockLevel != null)
            {
                stockLevel.QuantityOnHand += line.Quantity;

                // Create reversal stock movement
                var movement = new StockMovement
                {
                    Id = Guid.NewGuid(),
                    ProductId = line.ProductId,
                    WarehouseId = challan.WarehouseId,
                    MovementType = StockMovementType.Adjustment,
                    Quantity = line.Quantity,
                    SourceDocumentType = "DeliveryChallan",
                    SourceDocumentId = challan.Id,
                    SourceDocumentNumber = challan.ChallanNumber,
                    MovementDate = DateTime.UtcNow,
                    Notes = $"Reversal due to DC cancellation"
                };
                _context.StockMovements.Add(movement);
            }
        }
    }

    private async Task UpdateSalesOrderStatusAsync(Guid salesOrderId)
    {
        var so = await _context.SalesOrders
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == salesOrderId);

        if (so == null) return;

        // Calculate total shipped across all DCs for this SO
        var shippedQuantities = await _context.DeliveryChallanLines
            .Where(l => l.SalesOrderLineId.HasValue &&
                       so.Lines.Select(x => x.Id).Contains(l.SalesOrderLineId.Value))
            .GroupBy(l => l.SalesOrderLineId)
            .Select(g => new { SoLineId = g.Key, ShippedQty = g.Sum(x => x.Quantity) })
            .ToListAsync();

        // Update SO line shipped quantities
        foreach (var soLine in so.Lines)
        {
            soLine.QuantityShipped = shippedQuantities
                .FirstOrDefault(s => s.SoLineId == soLine.Id)?.ShippedQty ?? 0;
        }

        // Determine SO status
        var totalOrdered = so.Lines.Sum(l => l.Quantity);
        var totalShipped = so.Lines.Sum(l => l.QuantityShipped);

        if (totalShipped >= totalOrdered)
        {
            so.Status = SalesOrderStatus.Shipped;
        }
        else if (totalShipped > 0)
        {
            so.Status = SalesOrderStatus.PartiallyShipped;
        }
    }
}

public class DeliveryChallansTableViewModel
{
    public List<DeliveryChallan> DeliveryChallans { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}

public class DeliveryChallanFormViewModel
{
    public bool IsEdit { get; set; }
    public DeliveryChallan? DeliveryChallan { get; set; }
    public List<Customer> Customers { get; set; } = new();
    public List<Warehouse> Warehouses { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public List<SalesOrder> SalesOrders { get; set; } = new();
    public SalesOrder? SelectedSalesOrder { get; set; }
}

public class DeliveryChallanFormInput
{
    public Guid? Id { get; set; }
    public DateTime ChallanDate { get; set; } = DateTime.Today;
    public Guid? SalesOrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid WarehouseId { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string? TransportMode { get; set; }
    public string? TransporterName { get; set; }
    public string? ShipToName { get; set; }
    public string? ShipToAddress1 { get; set; }
    public string? ShipToAddress2 { get; set; }
    public string? ShipToCity { get; set; }
    public string? ShipToState { get; set; }
    public string? ShipToPostalCode { get; set; }
    public string? ShipToCountry { get; set; }
    public string? ShipToPhone { get; set; }
    public int TotalPackages { get; set; }
    public decimal? TotalWeight { get; set; }
    public string? WeightUnit { get; set; }
    public string? Notes { get; set; }
    public List<DeliveryChallanLineInput>? Lines { get; set; }
}

public class DeliveryChallanLineInput
{
    public Guid? SalesOrderLineId { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSku { get; set; }
    public decimal Quantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public string? Notes { get; set; }
}
