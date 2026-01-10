using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Inventory.Warehouses;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalWarehouses { get; set; }
    public int ActiveWarehouses { get; set; }
    public int TotalLocations { get; set; }
    public decimal TotalStockValue { get; set; }

    public async Task OnGetAsync()
    {
        TotalWarehouses = await _context.Warehouses.CountAsync();
        ActiveWarehouses = await _context.Warehouses.CountAsync(w => w.IsActive);
        TotalLocations = await _context.WarehouseLocations.CountAsync();

        var stockLevels = await _context.StockLevels
            .Include(s => s.Product)
            .ToListAsync();

        TotalStockValue = stockLevels.Sum(s => s.QuantityOnHand * s.Product.CostPrice);
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.Warehouses
            .Include(w => w.Locations)
            .Include(w => w.StockLevels)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(w =>
                w.Name.ToLower().Contains(search) ||
                w.Code.ToLower().Contains(search) ||
                (w.City != null && w.City.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && bool.TryParse(statusFilter, out var isActive))
        {
            query = query.Where(w => w.IsActive == isActive);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var warehouses = await query
            .OrderBy(w => w.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_WarehousesTableRows", new WarehousesTableViewModel
        {
            Warehouses = warehouses,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public IActionResult OnGetCreateForm()
    {
        return Partial("_WarehouseForm", new WarehouseFormViewModel
        {
            IsEdit = false
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id);

        if (warehouse == null)
            return NotFound();

        return Partial("_WarehouseForm", new WarehouseFormViewModel
        {
            IsEdit = true,
            Warehouse = warehouse
        });
    }

    public async Task<IActionResult> OnPostAsync(WarehouseFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Warehouse? warehouse;

        if (input.Id.HasValue)
        {
            warehouse = await _context.Warehouses.FindAsync(input.Id.Value);
            if (warehouse == null)
                return NotFound();
        }
        else
        {
            warehouse = new Warehouse
            {
                Id = Guid.NewGuid(),
                Code = await GenerateWarehouseCodeAsync()
            };
            _context.Warehouses.Add(warehouse);
        }

        warehouse.Name = input.Name;
        warehouse.Description = input.Description;
        warehouse.Address = input.Address;
        warehouse.City = input.City;
        warehouse.State = input.State;
        warehouse.Country = input.Country;
        warehouse.PostalCode = input.PostalCode;
        warehouse.ManagerName = input.ManagerName;
        warehouse.Phone = input.Phone;
        warehouse.Email = input.Email;
        warehouse.IsActive = input.IsActive;
        warehouse.IsDefault = input.IsDefault;

        // If this is marked as default, unset other defaults
        if (input.IsDefault)
        {
            var otherDefaults = await _context.Warehouses
                .Where(w => w.IsDefault && w.Id != warehouse.Id)
                .ToListAsync();
            foreach (var other in otherDefaults)
            {
                other.IsDefault = false;
            }
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var warehouse = await _context.Warehouses.FindAsync(id);
        if (warehouse == null)
            return NotFound();

        // Check if warehouse has stock
        var hasStock = await _context.StockLevels.AnyAsync(s => s.WarehouseId == id && s.QuantityOnHand > 0);
        if (hasStock)
        {
            return BadRequest("Cannot delete warehouse with existing stock. Transfer stock first.");
        }

        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    // Locations Management
    public async Task<IActionResult> OnGetLocationsAsync(Guid warehouseId)
    {
        var warehouse = await _context.Warehouses
            .Include(w => w.Locations)
            .FirstOrDefaultAsync(w => w.Id == warehouseId);

        if (warehouse == null)
            return NotFound();

        return Partial("_LocationsTable", new LocationsViewModel
        {
            Warehouse = warehouse,
            Locations = warehouse.Locations.ToList()
        });
    }

    public async Task<IActionResult> OnGetLocationFormAsync(Guid warehouseId, Guid? locationId = null)
    {
        var warehouse = await _context.Warehouses.FindAsync(warehouseId);
        if (warehouse == null)
            return NotFound();

        WarehouseLocation? location = null;
        if (locationId.HasValue)
        {
            location = await _context.WarehouseLocations.FindAsync(locationId.Value);
        }

        return Partial("_LocationForm", new LocationFormViewModel
        {
            IsEdit = location != null,
            WarehouseId = warehouseId,
            WarehouseName = warehouse.Name,
            Location = location
        });
    }

    public async Task<IActionResult> OnPostLocationAsync(LocationFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        WarehouseLocation? location;

        if (input.Id.HasValue)
        {
            location = await _context.WarehouseLocations.FindAsync(input.Id.Value);
            if (location == null)
                return NotFound();
        }
        else
        {
            location = new WarehouseLocation
            {
                Id = Guid.NewGuid(),
                WarehouseId = input.WarehouseId,
                Code = await GenerateLocationCodeAsync(input.WarehouseId)
            };
            _context.WarehouseLocations.Add(location);
        }

        location.Name = input.Name;
        location.Zone = input.Zone;
        location.Aisle = input.Aisle;
        location.Rack = input.Rack;
        location.Shelf = input.Shelf;
        location.Bin = input.Bin;
        location.IsActive = input.IsActive;

        await _context.SaveChangesAsync();

        return await OnGetLocationsAsync(input.WarehouseId);
    }

    public async Task<IActionResult> OnDeleteLocationAsync(Guid id)
    {
        var location = await _context.WarehouseLocations.FindAsync(id);
        if (location == null)
            return NotFound();

        var warehouseId = location.WarehouseId;

        // Check if location has stock
        var hasStock = await _context.StockLevels.AnyAsync(s => s.LocationId == id && s.QuantityOnHand > 0);
        if (hasStock)
        {
            return BadRequest("Cannot delete location with existing stock.");
        }

        _context.WarehouseLocations.Remove(location);
        await _context.SaveChangesAsync();

        return await OnGetLocationsAsync(warehouseId);
    }

    private async Task<string> GenerateWarehouseCodeAsync()
    {
        var lastWarehouse = await _context.Warehouses
            .IgnoreQueryFilters()
            .OrderByDescending(w => w.Code)
            .FirstOrDefaultAsync(w => w.Code.StartsWith("WH"));

        if (lastWarehouse == null)
            return "WH001";

        var lastNumber = int.Parse(lastWarehouse.Code.Replace("WH", ""));
        return $"WH{(lastNumber + 1):D3}";
    }

    private async Task<string> GenerateLocationCodeAsync(Guid warehouseId)
    {
        var lastLocation = await _context.WarehouseLocations
            .IgnoreQueryFilters()
            .Where(l => l.WarehouseId == warehouseId)
            .OrderByDescending(l => l.Code)
            .FirstOrDefaultAsync();

        if (lastLocation == null)
            return "LOC001";

        var lastNumber = int.Parse(lastLocation.Code.Replace("LOC", ""));
        return $"LOC{(lastNumber + 1):D3}";
    }
}

public class WarehousesTableViewModel
{
    public List<Warehouse> Warehouses { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/Inventory/Warehouses",
        Handler = "Table",
        HxTarget = "#warehousesTableBody",
        HxInclude = "#searchInput,#statusFilter,#pageSizeSelect"
    };
}

public class WarehouseFormViewModel
{
    public bool IsEdit { get; set; }
    public Warehouse? Warehouse { get; set; }
}

public class WarehouseFormInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? ManagerName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
}

public class LocationsViewModel
{
    public Warehouse Warehouse { get; set; } = null!;
    public List<WarehouseLocation> Locations { get; set; } = new();
}

public class LocationFormViewModel
{
    public bool IsEdit { get; set; }
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public WarehouseLocation? Location { get; set; }
}

public class LocationFormInput
{
    public Guid? Id { get; set; }
    public Guid WarehouseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Zone { get; set; }
    public string? Aisle { get; set; }
    public string? Rack { get; set; }
    public string? Shelf { get; set; }
    public string? Bin { get; set; }
    public bool IsActive { get; set; } = true;
}
