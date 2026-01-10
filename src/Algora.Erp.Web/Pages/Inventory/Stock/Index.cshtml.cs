using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Inventory.Stock;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalProducts { get; set; }
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }
    public decimal TotalStockValue { get; set; }

    public List<Warehouse> Warehouses { get; set; } = new();
    public List<ProductCategory> Categories { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalProducts = await _context.Products.CountAsync(p => p.TrackInventory);

        LowStockItems = await _context.StockLevels
            .Include(s => s.Product)
            .Where(s => s.QuantityOnHand <= s.Product.ReorderLevel && s.QuantityOnHand > 0)
            .Select(s => s.ProductId)
            .Distinct()
            .CountAsync();

        OutOfStockItems = await _context.StockLevels
            .Where(s => s.QuantityOnHand <= 0)
            .Select(s => s.ProductId)
            .Distinct()
            .CountAsync();

        TotalStockValue = await _context.StockLevels
            .Include(s => s.Product)
            .SumAsync(s => s.QuantityOnHand * s.Product.CostPrice);

        Warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
        Categories = await _context.ProductCategories.Where(c => c.IsActive).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, Guid? warehouseFilter, Guid? categoryFilter, string? stockFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.StockLevels
            .Include(s => s.Product)
                .ThenInclude(p => p.Category)
            .Include(s => s.Warehouse)
            .Include(s => s.Location)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(s =>
                s.Product.Name.ToLower().Contains(search) ||
                s.Product.Sku.ToLower().Contains(search) ||
                (s.Product.Barcode != null && s.Product.Barcode.ToLower().Contains(search)));
        }

        if (warehouseFilter.HasValue)
        {
            query = query.Where(s => s.WarehouseId == warehouseFilter.Value);
        }

        if (categoryFilter.HasValue)
        {
            query = query.Where(s => s.Product.CategoryId == categoryFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(stockFilter))
        {
            query = stockFilter switch
            {
                "low" => query.Where(s => s.QuantityOnHand <= s.Product.ReorderLevel && s.QuantityOnHand > 0),
                "out" => query.Where(s => s.QuantityOnHand <= 0),
                "overstock" => query.Where(s => s.QuantityOnHand > s.Product.MaximumStock && s.Product.MaximumStock > 0),
                _ => query
            };
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var stockLevels = await query
            .OrderBy(s => s.Product.Name)
            .ThenBy(s => s.Warehouse.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_StockTableRows", new StockTableViewModel
        {
            StockLevels = stockLevels,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnPostAdjustAsync(Guid stockLevelId, decimal adjustment, string reason)
    {
        var stockLevel = await _context.StockLevels
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == stockLevelId);

        if (stockLevel == null)
            return NotFound();

        var previousQty = stockLevel.QuantityOnHand;
        stockLevel.QuantityOnHand += adjustment;

        // Create stock movement record
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = stockLevel.ProductId,
            WarehouseId = stockLevel.WarehouseId,
            ToLocationId = stockLevel.LocationId,
            MovementType = StockMovementType.Adjustment,
            Quantity = adjustment,
            Reference = $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}",
            MovementDate = DateTime.UtcNow,
            Notes = $"Adjustment: {previousQty} -> {stockLevel.QuantityOnHand}. Reason: {reason}"
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null, null);
    }
}

public class StockTableViewModel
{
    public List<StockLevel> StockLevels { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/Inventory/Stock",
        Handler = "Table",
        HxTarget = "#stockTableBody",
        HxInclude = "#searchInput,#warehouseFilter,#statusFilter,#pageSizeSelect"
    };
}
