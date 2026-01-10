using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Procurement;
using Algora.Erp.Domain.Entities.Quality;
using Algora.Erp.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Quality.Rejections;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalRejections { get; set; }
    public int PendingDisposition { get; set; }
    public decimal TotalRejectedValue { get; set; }
    public int ReturnedToSupplier { get; set; }

    public List<Supplier> Suppliers { get; set; } = new();
    public List<Warehouse> Warehouses { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalRejections = await _context.RejectionNotes.CountAsync();
        PendingDisposition = await _context.RejectionNotes
            .CountAsync(r => r.DispositionStatus == RejectionDisposition.Pending || r.DispositionStatus == RejectionDisposition.HoldForReview);
        TotalRejectedValue = await _context.RejectionNotes.SumAsync(r => r.TotalValue);
        ReturnedToSupplier = await _context.RejectionNotes
            .CountAsync(r => r.DispositionStatus == RejectionDisposition.ReturnToSupplier);

        Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
        Warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, Guid? supplierFilter, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.RejectionNotes
            .Include(r => r.Product)
            .Include(r => r.Warehouse)
            .Include(r => r.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(r =>
                r.RejectionNumber.ToLower().Contains(search) ||
                r.ProductName.ToLower().Contains(search) ||
                r.ProductSku.ToLower().Contains(search) ||
                (r.SourceDocumentNumber != null && r.SourceDocumentNumber.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<RejectionDisposition>(statusFilter, out var status))
        {
            query = query.Where(r => r.DispositionStatus == status);
        }

        if (supplierFilter.HasValue)
        {
            query = query.Where(r => r.SupplierId == supplierFilter.Value);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var rejections = await query
            .OrderByDescending(r => r.RejectionDate)
            .ThenByDescending(r => r.RejectionNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_RejectionsTableRows", new RejectionsTableViewModel
        {
            Rejections = rejections,
            Pagination = new PaginationViewModel
            {
                Page = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                PageUrl = "/Quality/Rejections",
                HxTarget = "#rejectionsTableBody",
                HxInclude = "#searchInput,#statusFilter,#pageSizeSelect"
            }
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
        var products = await _context.Products.Where(p => p.IsActive).ToListAsync();
        var suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();

        // Get failed QC inspections that haven't been processed yet
        var failedInspections = await _context.QualityInspections
            .Where(q => q.OverallResult == QCOverallResult.Fail || q.OverallResult == QCOverallResult.PartialPass)
            .Include(q => q.Product)
            .OrderByDescending(q => q.InspectionDate)
            .Take(50)
            .ToListAsync();

        return Partial("_RejectionForm", new RejectionFormViewModel
        {
            IsEdit = false,
            Warehouses = warehouses,
            Products = products,
            Suppliers = suppliers,
            FailedInspections = failedInspections
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var rejection = await _context.RejectionNotes.FirstOrDefaultAsync(r => r.Id == id);

        if (rejection == null)
            return NotFound();

        var warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
        var products = await _context.Products.Where(p => p.IsActive).ToListAsync();
        var suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();

        return Partial("_RejectionForm", new RejectionFormViewModel
        {
            IsEdit = true,
            Rejection = rejection,
            Warehouses = warehouses,
            Products = products,
            Suppliers = suppliers
        });
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var rejection = await _context.RejectionNotes
            .Include(r => r.Product)
            .Include(r => r.Warehouse)
            .Include(r => r.Supplier)
            .Include(r => r.QualityInspection)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (rejection == null)
            return NotFound();

        return Partial("_RejectionDetails", rejection);
    }

    public async Task<IActionResult> OnPostAsync(RejectionFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        RejectionNote? rejection;

        if (input.Id.HasValue)
        {
            rejection = await _context.RejectionNotes.FirstOrDefaultAsync(r => r.Id == input.Id.Value);
            if (rejection == null)
                return NotFound();
        }
        else
        {
            rejection = new RejectionNote
            {
                Id = Guid.NewGuid(),
                RejectionNumber = await GenerateRejectionNumberAsync()
            };
            _context.RejectionNotes.Add(rejection);
        }

        var product = await _context.Products.FindAsync(input.ProductId);

        rejection.RejectionDate = input.RejectionDate;
        rejection.SourceDocumentType = input.SourceDocumentType;
        rejection.SourceDocumentId = input.SourceDocumentId;
        rejection.SourceDocumentNumber = input.SourceDocumentNumber;
        rejection.QualityInspectionId = input.QualityInspectionId;
        rejection.ProductId = input.ProductId;
        rejection.ProductName = product?.Name ?? "Unknown";
        rejection.ProductSku = product?.Sku ?? "";
        rejection.WarehouseId = input.WarehouseId;
        rejection.SupplierId = input.SupplierId;
        rejection.RejectedQuantity = input.RejectedQuantity;
        rejection.UnitOfMeasure = input.UnitOfMeasure ?? "EA";
        rejection.UnitCost = input.UnitCost;
        rejection.TotalValue = input.RejectedQuantity * input.UnitCost;
        rejection.RejectionReason = input.RejectionReason;
        rejection.RejectionCategory = input.RejectionCategory;
        rejection.DefectDescription = input.DefectDescription;
        rejection.Notes = input.Notes;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostUpdateDispositionAsync(Guid id, DispositionInput input)
    {
        var rejection = await _context.RejectionNotes.FindAsync(id);

        if (rejection == null)
            return NotFound();

        rejection.DispositionStatus = input.DispositionStatus;
        rejection.DispositionAction = input.DispositionAction;
        rejection.DispositionDate = DateTime.UtcNow;
        rejection.DisposerName = input.DisposerName;

        // Handle specific dispositions
        switch (input.DispositionStatus)
        {
            case RejectionDisposition.ReturnToSupplier:
                rejection.ReturnDate = input.ReturnDate ?? DateTime.UtcNow;
                rejection.ReturnReference = input.ReturnReference;
                // TODO: Create debit note
                break;
            case RejectionDisposition.Scrapped:
                rejection.ScrapDate = DateTime.UtcNow;
                rejection.ScrapReference = input.ScrapReference;
                rejection.ScrapValue = input.ScrapValue;
                break;
            case RejectionDisposition.Reworked:
                rejection.ReworkInstructions = input.ReworkInstructions;
                break;
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var rejection = await _context.RejectionNotes.FirstOrDefaultAsync(r => r.Id == id);

        if (rejection == null)
            return NotFound();

        if (rejection.DispositionStatus != RejectionDisposition.Pending)
        {
            return BadRequest("Only pending rejection notes can be deleted.");
        }

        _context.RejectionNotes.Remove(rejection);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    private async Task<string> GenerateRejectionNumberAsync()
    {
        var lastRejection = await _context.RejectionNotes
            .IgnoreQueryFilters()
            .OrderByDescending(r => r.RejectionNumber)
            .FirstOrDefaultAsync(r => r.RejectionNumber.StartsWith("REJ"));

        if (lastRejection == null)
            return $"REJ{DateTime.UtcNow:yyyyMM}001";

        var lastNumber = int.Parse(lastRejection.RejectionNumber.Substring(9));
        return $"REJ{DateTime.UtcNow:yyyyMM}{(lastNumber + 1):D3}";
    }
}

public class RejectionsTableViewModel
{
    public List<RejectionNote> Rejections { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
}

public class RejectionFormViewModel
{
    public bool IsEdit { get; set; }
    public RejectionNote? Rejection { get; set; }
    public List<Warehouse> Warehouses { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public List<Supplier> Suppliers { get; set; } = new();
    public List<QualityInspection> FailedInspections { get; set; } = new();
}

public class RejectionFormInput
{
    public Guid? Id { get; set; }
    public DateTime RejectionDate { get; set; } = DateTime.Today;
    public string SourceDocumentType { get; set; } = string.Empty;
    public Guid SourceDocumentId { get; set; }
    public string? SourceDocumentNumber { get; set; }
    public Guid? QualityInspectionId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? SupplierId { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal UnitCost { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public RejectionCategory RejectionCategory { get; set; }
    public string? DefectDescription { get; set; }
    public string? Notes { get; set; }
}

public class DispositionInput
{
    public RejectionDisposition DispositionStatus { get; set; }
    public string? DispositionAction { get; set; }
    public string? DisposerName { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? ReturnReference { get; set; }
    public string? ScrapReference { get; set; }
    public decimal? ScrapValue { get; set; }
    public string? ReworkInstructions { get; set; }
}
