using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Manufacturing;
using Algora.Erp.Web.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Manufacturing.BOM;

[Authorize(Policy = "CanViewManufacturing")]
[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalBoms { get; set; }
    public int ActiveBoms { get; set; }
    public int DraftBoms { get; set; }
    public decimal TotalMaterialCost { get; set; }

    public List<Product> Products { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalBoms = await _context.BillOfMaterials.CountAsync();
        ActiveBoms = await _context.BillOfMaterials.CountAsync(b => b.Status == BomStatus.Active);
        DraftBoms = await _context.BillOfMaterials.CountAsync(b => b.Status == BomStatus.Draft);
        TotalMaterialCost = await _context.BomLines.SumAsync(l => l.TotalCost);

        Products = await _context.Products.Where(p => p.IsActive).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.BillOfMaterials
            .Include(b => b.Product)
            .Include(b => b.Lines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(b =>
                b.BomNumber.ToLower().Contains(search) ||
                b.Name.ToLower().Contains(search) ||
                b.Product!.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<BomStatus>(statusFilter, out var status))
        {
            query = query.Where(b => b.Status == status);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var boms = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_BomTableRows", new BomTableViewModel
        {
            Boms = boms,
            Pagination = new PaginationViewModel
            {
                Page = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                PageUrl = "/Manufacturing/BOM",
                HxTarget = "#bomTableContainer",
                HxInclude = "#searchInput,#statusFilter"
            }
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var products = await _context.Products.Where(p => p.IsActive).ToListAsync();
        return Partial("_BomForm", new BomFormViewModel
        {
            IsEdit = false,
            Products = products
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var bom = await _context.BillOfMaterials
            .Include(b => b.Lines)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bom == null)
            return NotFound();

        var products = await _context.Products.Where(p => p.IsActive).ToListAsync();
        return Partial("_BomForm", new BomFormViewModel
        {
            IsEdit = true,
            Bom = bom,
            Products = products
        });
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var bom = await _context.BillOfMaterials
            .Include(b => b.Product)
            .Include(b => b.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bom == null)
            return NotFound();

        return Partial("_BomDetails", bom);
    }

    public async Task<IActionResult> OnPostAsync(BomFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        BillOfMaterial? bom;

        if (input.Id.HasValue)
        {
            bom = await _context.BillOfMaterials
                .Include(b => b.Lines)
                .FirstOrDefaultAsync(b => b.Id == input.Id.Value);
            if (bom == null)
                return NotFound();

            // Clear existing lines
            foreach (var line in bom.Lines.ToList())
            {
                _context.BomLines.Remove(line);
            }
        }
        else
        {
            bom = new BillOfMaterial
            {
                Id = Guid.NewGuid(),
                BomNumber = await GenerateBomNumberAsync()
            };
            _context.BillOfMaterials.Add(bom);
        }

        bom.Name = input.Name;
        bom.Description = input.Description;
        bom.ProductId = input.ProductId;
        bom.Quantity = input.Quantity;
        bom.UnitOfMeasure = input.UnitOfMeasure;
        bom.Status = input.Status;
        bom.EffectiveFrom = input.EffectiveFrom;
        bom.EffectiveTo = input.EffectiveTo;
        bom.IsActive = input.IsActive;
        bom.Notes = input.Notes;

        // Add lines
        int lineNumber = 1;
        if (input.Lines != null)
        {
            foreach (var lineInput in input.Lines.Where(l => l.ProductId != Guid.Empty))
            {
                var product = await _context.Products.FindAsync(lineInput.ProductId);
                var line = new BomLine
                {
                    Id = Guid.NewGuid(),
                    BillOfMaterialId = bom.Id,
                    ProductId = lineInput.ProductId,
                    LineNumber = lineNumber++,
                    Quantity = lineInput.Quantity,
                    UnitOfMeasure = product?.UnitOfMeasure,
                    UnitCost = product?.CostPrice ?? 0,
                    TotalCost = lineInput.Quantity * (product?.CostPrice ?? 0),
                    WastagePercent = lineInput.WastagePercent,
                    IsOptional = lineInput.IsOptional,
                    Notes = lineInput.Notes
                };
                _context.BomLines.Add(line);
            }
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, BomStatus status)
    {
        var bom = await _context.BillOfMaterials.FindAsync(id);
        if (bom == null)
            return NotFound();

        bom.Status = status;
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var bom = await _context.BillOfMaterials
            .Include(b => b.Lines)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bom == null)
            return NotFound();

        if (bom.Status != BomStatus.Draft)
        {
            return BadRequest("Only draft BOMs can be deleted.");
        }

        _context.BillOfMaterials.Remove(bom);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    private async Task<string> GenerateBomNumberAsync()
    {
        var lastBom = await _context.BillOfMaterials
            .IgnoreQueryFilters()
            .OrderByDescending(b => b.BomNumber)
            .FirstOrDefaultAsync(b => b.BomNumber.StartsWith("BOM"));

        if (lastBom == null)
            return "BOM00001";

        var lastNumber = int.Parse(lastBom.BomNumber.Substring(3));
        return $"BOM{(lastNumber + 1):D5}";
    }
}

public class BomTableViewModel
{
    public List<BillOfMaterial> Boms { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
}

public class BomFormViewModel
{
    public bool IsEdit { get; set; }
    public BillOfMaterial? Bom { get; set; }
    public List<Product> Products { get; set; } = new();
}

public class BomFormInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public string? UnitOfMeasure { get; set; }
    public BomStatus Status { get; set; } = BomStatus.Draft;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public List<BomLineInput>? Lines { get; set; }
}

public class BomLineInput
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal WastagePercent { get; set; }
    public bool IsOptional { get; set; }
    public string? Notes { get; set; }
}
