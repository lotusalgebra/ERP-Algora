using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Procurement;
using Algora.Erp.Domain.Entities.Quality;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Quality.Inspections;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalInspections { get; set; }
    public int PendingInspections { get; set; }
    public int PassedToday { get; set; }
    public int FailedToday { get; set; }

    public List<Warehouse> Warehouses { get; set; } = new();
    public Guid? GrnId { get; set; }

    public async Task OnGetAsync(Guid? grnId = null)
    {
        GrnId = grnId;

        TotalInspections = await _context.QualityInspections.CountAsync();
        PendingInspections = await _context.QualityInspections
            .CountAsync(q => q.Status == InspectionStatus.Pending || q.Status == InspectionStatus.InProgress);
        PassedToday = await _context.QualityInspections
            .CountAsync(q => q.InspectionDate.Date == DateTime.UtcNow.Date && q.OverallResult == QCOverallResult.Pass);
        FailedToday = await _context.QualityInspections
            .CountAsync(q => q.InspectionDate.Date == DateTime.UtcNow.Date && q.OverallResult == QCOverallResult.Fail);

        Warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();

        // If grnId is provided, auto-open create form
        if (grnId.HasValue)
        {
            // This will be handled by JavaScript in the view
        }
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, string? typeFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.QualityInspections
            .Include(q => q.Parameters)
            .Include(q => q.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(q =>
                q.InspectionNumber.ToLower().Contains(search) ||
                (q.SourceDocumentNumber != null && q.SourceDocumentNumber.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<InspectionStatus>(statusFilter, out var status))
        {
            query = query.Where(q => q.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(typeFilter) && Enum.TryParse<InspectionType>(typeFilter, out var type))
        {
            query = query.Where(q => q.InspectionType == type);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var inspections = await query
            .OrderByDescending(q => q.InspectionDate)
            .ThenByDescending(q => q.InspectionNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_InspectionsTableRows", new InspectionsTableViewModel
        {
            Inspections = inspections,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync(Guid? grnId = null)
    {
        var warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
        var products = await _context.Products.Where(p => p.IsActive).ToListAsync();

        // Get GRNs pending QC
        var pendingGrns = await _context.GoodsReceiptNotes
            .Where(g => g.Status == GoodsReceiptStatus.QCPending)
            .Include(g => g.Lines)
            .OrderByDescending(g => g.GrnDate)
            .ToListAsync();

        GoodsReceiptNote? selectedGrn = null;
        if (grnId.HasValue)
        {
            selectedGrn = await _context.GoodsReceiptNotes
                .Include(g => g.Lines)
                .FirstOrDefaultAsync(g => g.Id == grnId.Value);
        }

        return Partial("_InspectionForm", new InspectionFormViewModel
        {
            IsEdit = false,
            Warehouses = warehouses,
            Products = products,
            PendingGrns = pendingGrns,
            SelectedGrn = selectedGrn
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var inspection = await _context.QualityInspections
            .Include(q => q.Parameters)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (inspection == null)
            return NotFound();

        var warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
        var products = await _context.Products.Where(p => p.IsActive).ToListAsync();

        return Partial("_InspectionForm", new InspectionFormViewModel
        {
            IsEdit = true,
            Inspection = inspection,
            Warehouses = warehouses,
            Products = products
        });
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var inspection = await _context.QualityInspections
            .Include(q => q.Parameters)
            .Include(q => q.Product)
            .Include(q => q.Warehouse)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (inspection == null)
            return NotFound();

        return Partial("_InspectionDetails", inspection);
    }

    public async Task<IActionResult> OnPostAsync(InspectionFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        QualityInspection? inspection;

        if (input.Id.HasValue)
        {
            inspection = await _context.QualityInspections
                .Include(q => q.Parameters)
                .FirstOrDefaultAsync(q => q.Id == input.Id.Value);
            if (inspection == null)
                return NotFound();

            // Clear existing parameters
            foreach (var param in inspection.Parameters.ToList())
            {
                _context.QualityParameters.Remove(param);
            }
        }
        else
        {
            inspection = new QualityInspection
            {
                Id = Guid.NewGuid(),
                InspectionNumber = await GenerateInspectionNumberAsync()
            };
            _context.QualityInspections.Add(inspection);
        }

        inspection.InspectionDate = input.InspectionDate;
        inspection.InspectionType = input.InspectionType;
        inspection.SourceDocumentType = input.SourceDocumentType;
        inspection.SourceDocumentId = input.SourceDocumentId;
        inspection.SourceDocumentNumber = input.SourceDocumentNumber;
        inspection.ProductId = input.ProductId;
        inspection.WarehouseId = input.WarehouseId;
        inspection.SupplierId = input.SupplierId;
        inspection.TotalQuantity = input.TotalQuantity;
        inspection.SampleSize = input.SampleSize;
        inspection.Notes = input.Notes;

        // Add parameters
        int seqNum = 1;
        if (input.Parameters != null)
        {
            foreach (var paramInput in input.Parameters.Where(p => !string.IsNullOrEmpty(p.ParameterName)))
            {
                var param = new QualityParameter
                {
                    Id = Guid.NewGuid(),
                    QualityInspectionId = inspection.Id,
                    SequenceNumber = seqNum++,
                    ParameterName = paramInput.ParameterName,
                    ParameterCode = paramInput.ParameterCode,
                    ExpectedValue = paramInput.ExpectedValue,
                    MinValue = paramInput.MinValue,
                    MaxValue = paramInput.MaxValue,
                    Tolerance = paramInput.Tolerance,
                    Unit = paramInput.Unit,
                    ActualValue = paramInput.ActualValue,
                    MeasuredValue = paramInput.MeasuredValue,
                    Result = paramInput.Result,
                    Remarks = paramInput.Remarks
                };
                _context.QualityParameters.Add(param);
            }
        }

        // Set status based on parameters
        if (inspection.Parameters.Any())
        {
            inspection.Status = InspectionStatus.InProgress;
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostCompleteInspectionAsync(Guid id, CompleteInspectionInput input)
    {
        var inspection = await _context.QualityInspections
            .Include(q => q.Parameters)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (inspection == null)
            return NotFound();

        // Update parameters with actual values
        foreach (var paramUpdate in input.Parameters ?? new List<ParameterResultInput>())
        {
            var param = inspection.Parameters.FirstOrDefault(p => p.Id == paramUpdate.ParameterId);
            if (param != null)
            {
                param.ActualValue = paramUpdate.ActualValue;
                param.MeasuredValue = paramUpdate.MeasuredValue;
                param.Result = paramUpdate.Result;
                param.Remarks = paramUpdate.Remarks;
            }
        }

        // Calculate overall result
        inspection.InspectedQuantity = input.InspectedQuantity;
        inspection.PassedQuantity = input.PassedQuantity;
        inspection.FailedQuantity = input.FailedQuantity;
        inspection.InspectedAt = DateTime.UtcNow;
        inspection.InspectorName = input.InspectorName;

        var failedParams = inspection.Parameters.Count(p => p.Result == QCParameterResult.Fail);
        var passedParams = inspection.Parameters.Count(p => p.Result == QCParameterResult.Pass);
        var totalParams = inspection.Parameters.Count(p => p.Result != QCParameterResult.NotApplicable);

        if (totalParams == 0)
        {
            inspection.OverallResult = QCOverallResult.Pass;
        }
        else if (failedParams == 0)
        {
            inspection.OverallResult = QCOverallResult.Pass;
        }
        else if (passedParams == 0)
        {
            inspection.OverallResult = QCOverallResult.Fail;
        }
        else
        {
            inspection.OverallResult = QCOverallResult.PartialPass;
        }

        inspection.Status = InspectionStatus.Completed;
        inspection.ResultRemarks = input.ResultRemarks;

        // Update source document (GRN) if applicable
        if (inspection.SourceDocumentType == "GoodsReceipt")
        {
            var grn = await _context.GoodsReceiptNotes
                .Include(g => g.Lines)
                .FirstOrDefaultAsync(g => g.Id == inspection.SourceDocumentId);

            if (grn != null)
            {
                // Update GRN line QC status
                foreach (var line in grn.Lines.Where(l => l.ProductId == inspection.ProductId))
                {
                    line.QualityInspectionId = inspection.Id;
                    line.QCStatus = inspection.OverallResult switch
                    {
                        QCOverallResult.Pass => GoodsReceiptLineQCStatus.Passed,
                        QCOverallResult.Fail => GoodsReceiptLineQCStatus.Failed,
                        QCOverallResult.PartialPass => GoodsReceiptLineQCStatus.PartialPass,
                        _ => GoodsReceiptLineQCStatus.Pending
                    };

                    // Update quantities based on inspection result
                    if (inspection.OverallResult == QCOverallResult.Pass)
                    {
                        line.AcceptedQuantity = line.ReceivedQuantity;
                        line.RejectedQuantity = 0;
                    }
                    else if (inspection.OverallResult == QCOverallResult.Fail)
                    {
                        line.AcceptedQuantity = 0;
                        line.RejectedQuantity = line.ReceivedQuantity;
                        line.RejectionReason = input.ResultRemarks;
                    }
                    else if (inspection.OverallResult == QCOverallResult.PartialPass)
                    {
                        line.AcceptedQuantity = inspection.PassedQuantity;
                        line.RejectedQuantity = inspection.FailedQuantity;
                    }
                }

                // Check if all lines have been inspected
                var allLinesInspected = grn.Lines.All(l => l.QCStatus != GoodsReceiptLineQCStatus.Pending);
                if (allLinesInspected)
                {
                    grn.Status = GoodsReceiptStatus.QCCompleted;
                    grn.QCCompletedAt = DateTime.UtcNow;
                    grn.TotalAcceptedQuantity = grn.Lines.Sum(l => l.AcceptedQuantity);
                    grn.TotalRejectedQuantity = grn.Lines.Sum(l => l.RejectedQuantity);
                }
            }
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        var inspection = await _context.QualityInspections.FindAsync(id);
        if (inspection == null)
            return NotFound();

        inspection.Status = InspectionStatus.Approved;
        inspection.ApprovedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var inspection = await _context.QualityInspections
            .Include(q => q.Parameters)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (inspection == null)
            return NotFound();

        if (inspection.Status != InspectionStatus.Pending)
        {
            return BadRequest("Only pending inspections can be deleted.");
        }

        _context.QualityInspections.Remove(inspection);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    private async Task<string> GenerateInspectionNumberAsync()
    {
        var lastInspection = await _context.QualityInspections
            .IgnoreQueryFilters()
            .OrderByDescending(q => q.InspectionNumber)
            .FirstOrDefaultAsync(q => q.InspectionNumber.StartsWith("QC"));

        if (lastInspection == null)
            return $"QC{DateTime.UtcNow:yyyyMM}001";

        var lastNumber = int.Parse(lastInspection.InspectionNumber.Substring(8));
        return $"QC{DateTime.UtcNow:yyyyMM}{(lastNumber + 1):D3}";
    }
}

public class InspectionsTableViewModel
{
    public List<QualityInspection> Inspections { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}

public class InspectionFormViewModel
{
    public bool IsEdit { get; set; }
    public QualityInspection? Inspection { get; set; }
    public List<Warehouse> Warehouses { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public List<GoodsReceiptNote> PendingGrns { get; set; } = new();
    public GoodsReceiptNote? SelectedGrn { get; set; }
}

public class InspectionFormInput
{
    public Guid? Id { get; set; }
    public DateTime InspectionDate { get; set; } = DateTime.Today;
    public InspectionType InspectionType { get; set; } = InspectionType.Incoming;
    public string SourceDocumentType { get; set; } = string.Empty;
    public Guid SourceDocumentId { get; set; }
    public string? SourceDocumentNumber { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? SupplierId { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal SampleSize { get; set; }
    public string? Notes { get; set; }
    public List<ParameterInput>? Parameters { get; set; }
}

public class ParameterInput
{
    public string ParameterName { get; set; } = string.Empty;
    public string? ParameterCode { get; set; }
    public string? ExpectedValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? Tolerance { get; set; }
    public string? Unit { get; set; }
    public string? ActualValue { get; set; }
    public decimal? MeasuredValue { get; set; }
    public QCParameterResult Result { get; set; } = QCParameterResult.Pending;
    public string? Remarks { get; set; }
}

public class CompleteInspectionInput
{
    public decimal InspectedQuantity { get; set; }
    public decimal PassedQuantity { get; set; }
    public decimal FailedQuantity { get; set; }
    public string? InspectorName { get; set; }
    public string? ResultRemarks { get; set; }
    public List<ParameterResultInput>? Parameters { get; set; }
}

public class ParameterResultInput
{
    public Guid ParameterId { get; set; }
    public string? ActualValue { get; set; }
    public decimal? MeasuredValue { get; set; }
    public QCParameterResult Result { get; set; }
    public string? Remarks { get; set; }
}
