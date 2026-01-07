using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Procurement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Algora.Erp.Web.Pages.Procurement.GoodsReceipt;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalGrns { get; set; }
    public int PendingQC { get; set; }
    public int ReceivedToday { get; set; }
    public decimal TotalValue { get; set; }

    public List<Supplier> Suppliers { get; set; } = new();
    public List<Warehouse> Warehouses { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalGrns = await _context.GoodsReceiptNotes.CountAsync();
        PendingQC = await _context.GoodsReceiptNotes
            .CountAsync(g => g.Status == GoodsReceiptStatus.QCPending);
        ReceivedToday = await _context.GoodsReceiptNotes
            .CountAsync(g => g.GrnDate.Date == DateTime.UtcNow.Date);
        TotalValue = await _context.GoodsReceiptNotes.SumAsync(g => g.TotalValue);

        Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
        Warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, Guid? supplierFilter, string? statusFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.GoodsReceiptNotes
            .Include(g => g.Lines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(g =>
                g.GrnNumber.ToLower().Contains(search) ||
                g.SupplierName.ToLower().Contains(search) ||
                (g.PurchaseOrderNumber != null && g.PurchaseOrderNumber.ToLower().Contains(search)) ||
                (g.SupplierInvoiceNumber != null && g.SupplierInvoiceNumber.ToLower().Contains(search)));
        }

        if (supplierFilter.HasValue)
        {
            query = query.Where(g => g.SupplierId == supplierFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<GoodsReceiptStatus>(statusFilter, out var status))
        {
            query = query.Where(g => g.Status == status);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var grns = await query
            .OrderByDescending(g => g.GrnDate)
            .ThenByDescending(g => g.GrnNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_GrnTableRows", new GrnTableViewModel
        {
            GoodsReceiptNotes = grns,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync(Guid? purchaseOrderId = null)
    {
        var suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
        var warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
        var purchaseOrders = await _context.PurchaseOrders
            .Where(p => p.Status == PurchaseOrderStatus.Approved ||
                       p.Status == PurchaseOrderStatus.Sent ||
                       p.Status == PurchaseOrderStatus.PartiallyReceived)
            .Include(p => p.Supplier)
            .OrderByDescending(p => p.OrderDate)
            .ToListAsync();

        PurchaseOrder? selectedPo = null;
        if (purchaseOrderId.HasValue)
        {
            selectedPo = await _context.PurchaseOrders
                .Include(p => p.Lines)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == purchaseOrderId.Value);
        }

        return Partial("_GrnForm", new GrnFormViewModel
        {
            IsEdit = false,
            Suppliers = suppliers,
            Warehouses = warehouses,
            PurchaseOrders = purchaseOrders,
            SelectedPurchaseOrder = selectedPo
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var grn = await _context.GoodsReceiptNotes
            .Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (grn == null)
            return NotFound();

        var suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
        var warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
        var purchaseOrders = await _context.PurchaseOrders
            .Where(p => p.Status == PurchaseOrderStatus.Approved ||
                       p.Status == PurchaseOrderStatus.Sent ||
                       p.Status == PurchaseOrderStatus.PartiallyReceived ||
                       p.Id == grn.PurchaseOrderId)
            .Include(p => p.Supplier)
            .OrderByDescending(p => p.OrderDate)
            .ToListAsync();

        return Partial("_GrnForm", new GrnFormViewModel
        {
            IsEdit = true,
            GoodsReceiptNote = grn,
            Suppliers = suppliers,
            Warehouses = warehouses,
            PurchaseOrders = purchaseOrders
        });
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var grn = await _context.GoodsReceiptNotes
            .Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (grn == null)
            return NotFound();

        return Partial("_GrnDetails", grn);
    }

    public async Task<IActionResult> OnGetPurchaseOrderLinesAsync(Guid purchaseOrderId)
    {
        var po = await _context.PurchaseOrders
            .Include(p => p.Lines)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == purchaseOrderId);

        if (po == null)
            return NotFound();

        // Calculate remaining quantities (ordered - already received)
        var existingGrnLines = await _context.GoodsReceiptLines
            .Where(l => l.PurchaseOrderLineId.HasValue &&
                       po.Lines.Select(x => x.Id).Contains(l.PurchaseOrderLineId.Value))
            .GroupBy(l => l.PurchaseOrderLineId)
            .Select(g => new { PoLineId = g.Key, ReceivedQty = g.Sum(x => x.ReceivedQuantity) })
            .ToListAsync();

        var linesWithRemaining = po.Lines.Select(line => new PurchaseOrderLineWithRemaining
        {
            Line = line,
            RemainingQuantity = line.Quantity - (existingGrnLines.FirstOrDefault(x => x.PoLineId == line.Id)?.ReceivedQty ?? 0)
        }).Where(l => l.RemainingQuantity > 0).ToList();

        return new JsonResult(new
        {
            purchaseOrder = new
            {
                id = po.Id,
                orderNumber = po.OrderNumber,
                supplierId = po.SupplierId,
                supplierName = po.Supplier?.Name,
                warehouseId = po.WarehouseId
            },
            lines = linesWithRemaining.Select(l => new
            {
                purchaseOrderLineId = l.Line.Id,
                productId = l.Line.ProductId,
                productName = l.Line.ProductName,
                productSku = l.Line.ProductSku,
                orderedQuantity = l.Line.Quantity,
                remainingQuantity = l.RemainingQuantity,
                unitOfMeasure = l.Line.UnitOfMeasure,
                unitPrice = l.Line.UnitPrice
            })
        });
    }

    public async Task<IActionResult> OnPostAsync(GrnFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        GoodsReceiptNote? grn;

        if (input.Id.HasValue)
        {
            grn = await _context.GoodsReceiptNotes
                .Include(g => g.Lines)
                .FirstOrDefaultAsync(g => g.Id == input.Id.Value);
            if (grn == null)
                return NotFound();

            // Clear existing lines
            foreach (var line in grn.Lines.ToList())
            {
                _context.GoodsReceiptLines.Remove(line);
            }
        }
        else
        {
            grn = new GoodsReceiptNote
            {
                Id = Guid.NewGuid(),
                GrnNumber = await GenerateGrnNumberAsync()
            };
            _context.GoodsReceiptNotes.Add(grn);
        }

        // Get supplier name
        var supplier = await _context.Suppliers.FindAsync(input.SupplierId);

        grn.GrnDate = input.GrnDate;
        grn.PurchaseOrderId = input.PurchaseOrderId;
        grn.PurchaseOrderNumber = input.PurchaseOrderNumber;
        grn.SupplierId = input.SupplierId;
        grn.SupplierName = supplier?.Name ?? "Unknown";
        grn.SupplierInvoiceNumber = input.SupplierInvoiceNumber;
        grn.SupplierInvoiceDate = input.SupplierInvoiceDate;
        grn.WarehouseId = input.WarehouseId;
        grn.VehicleNumber = input.VehicleNumber;
        grn.DriverName = input.DriverName;
        grn.TransporterName = input.TransporterName;
        grn.WayBillNumber = input.WayBillNumber;
        grn.Notes = input.Notes;
        grn.QCRequired = input.QCRequired;

        // Add lines
        decimal totalOrdered = 0;
        decimal totalReceived = 0;
        decimal totalValue = 0;
        int lineNumber = 1;

        if (input.Lines != null)
        {
            foreach (var lineInput in input.Lines.Where(l => l.ProductId != Guid.Empty))
            {
                var product = await _context.Products.FindAsync(lineInput.ProductId);

                var line = new GoodsReceiptLine
                {
                    Id = Guid.NewGuid(),
                    GoodsReceiptNoteId = grn.Id,
                    LineNumber = lineNumber++,
                    PurchaseOrderLineId = lineInput.PurchaseOrderLineId,
                    ProductId = lineInput.ProductId,
                    ProductName = product?.Name ?? lineInput.ProductName ?? "Unknown",
                    ProductSku = product?.Sku ?? lineInput.ProductSku ?? "",
                    OrderedQuantity = lineInput.OrderedQuantity,
                    ReceivedQuantity = lineInput.ReceivedQuantity,
                    AcceptedQuantity = input.QCRequired ? 0 : lineInput.ReceivedQuantity,
                    RejectedQuantity = 0,
                    UnitOfMeasure = lineInput.UnitOfMeasure ?? "EA",
                    UnitPrice = lineInput.UnitPrice,
                    LineTotal = lineInput.ReceivedQuantity * lineInput.UnitPrice,
                    QCStatus = input.QCRequired ? GoodsReceiptLineQCStatus.Pending : GoodsReceiptLineQCStatus.Passed,
                    BatchNumber = lineInput.BatchNumber,
                    ExpiryDate = lineInput.ExpiryDate,
                    Notes = lineInput.Notes
                };

                _context.GoodsReceiptLines.Add(line);

                totalOrdered += lineInput.OrderedQuantity;
                totalReceived += lineInput.ReceivedQuantity;
                totalValue += line.LineTotal;
            }
        }

        grn.TotalOrderedQuantity = totalOrdered;
        grn.TotalReceivedQuantity = totalReceived;
        grn.TotalValue = totalValue;

        // Set status based on QC requirement
        if (input.QCRequired)
        {
            grn.Status = GoodsReceiptStatus.QCPending;
        }
        else
        {
            grn.TotalAcceptedQuantity = totalReceived;
            grn.Status = GoodsReceiptStatus.Accepted;
            grn.ReceivedAt = DateTime.UtcNow;

            // Update stock levels
            await UpdateStockLevelsAsync(grn);

            // Update PO status if linked (pass current GRN lines since they're not saved yet)
            if (grn.PurchaseOrderId.HasValue)
            {
                await UpdatePurchaseOrderStatusAsync(grn.PurchaseOrderId.Value, grn.Lines);
            }
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, GoodsReceiptStatus status)
    {
        var grn = await _context.GoodsReceiptNotes
            .Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (grn == null)
            return NotFound();

        var oldStatus = grn.Status;
        grn.Status = status;

        if (status == GoodsReceiptStatus.Accepted)
        {
            grn.ReceivedAt = DateTime.UtcNow;
            grn.TotalAcceptedQuantity = grn.TotalReceivedQuantity;

            // Update stock levels
            await UpdateStockLevelsAsync(grn);

            // Update PO status
            if (grn.PurchaseOrderId.HasValue)
            {
                await UpdatePurchaseOrderStatusAsync(grn.PurchaseOrderId.Value);
            }
        }
        else if (status == GoodsReceiptStatus.Cancelled)
        {
            // Create cancellation log entry
            var cancellationLog = new CancellationLog
            {
                Id = Guid.NewGuid(),
                DocumentType = "GoodsReceiptNote",
                DocumentId = grn.Id,
                DocumentNumber = grn.GrnNumber,
                CancelledAt = DateTime.UtcNow,
                CancelledBy = Guid.Empty,
                CancelledByName = User.Identity?.Name ?? "System",
                CancellationReason = "Cancelled by user",
                ReasonCategory = CancellationReasonCategory.Other,
                OriginalDocumentState = JsonSerializer.Serialize(new
                {
                    Status = oldStatus.ToString(),
                    grn.TotalReceivedQuantity,
                    grn.TotalAcceptedQuantity,
                    grn.TotalValue,
                    grn.GrnDate
                }),
                StockReversed = oldStatus == GoodsReceiptStatus.Accepted,
                Notes = $"GRN cancelled from status: {oldStatus}"
            };
            _context.CancellationLogs.Add(cancellationLog);

            // Reverse stock movements if cancelling an accepted GRN
            if (oldStatus == GoodsReceiptStatus.Accepted)
            {
                await ReverseStockLevelsAsync(grn);
                cancellationLog.StockReversalDetails = JsonSerializer.Serialize(
                    grn.Lines.Where(l => l.AcceptedQuantity > 0)
                        .Select(l => new { l.ProductId, l.ProductName, l.AcceptedQuantity })
                        .ToList());
            }
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var grn = await _context.GoodsReceiptNotes
            .Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (grn == null)
            return NotFound();

        // Can only delete draft GRNs
        if (grn.Status != GoodsReceiptStatus.Draft)
        {
            return BadRequest("Only draft GRNs can be deleted. Cancel the GRN instead.");
        }

        _context.GoodsReceiptNotes.Remove(grn);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    private async Task<string> GenerateGrnNumberAsync()
    {
        var lastGrn = await _context.GoodsReceiptNotes
            .IgnoreQueryFilters()
            .OrderByDescending(g => g.GrnNumber)
            .FirstOrDefaultAsync(g => g.GrnNumber.StartsWith("GRN"));

        if (lastGrn == null)
            return $"GRN{DateTime.UtcNow:yyyyMM}001";

        var lastNumber = int.Parse(lastGrn.GrnNumber.Substring(9));
        return $"GRN{DateTime.UtcNow:yyyyMM}{(lastNumber + 1):D3}";
    }

    private async Task UpdateStockLevelsAsync(GoodsReceiptNote grn)
    {
        foreach (var line in grn.Lines.Where(l => l.AcceptedQuantity > 0))
        {
            var stockLevel = await _context.StockLevels
                .FirstOrDefaultAsync(s => s.ProductId == line.ProductId && s.WarehouseId == grn.WarehouseId);

            if (stockLevel == null)
            {
                stockLevel = new StockLevel
                {
                    Id = Guid.NewGuid(),
                    ProductId = line.ProductId,
                    WarehouseId = grn.WarehouseId,
                    QuantityOnHand = 0,
                    QuantityReserved = 0
                };
                _context.StockLevels.Add(stockLevel);
            }

            stockLevel.QuantityOnHand += line.AcceptedQuantity;

            // Create stock movement
            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = line.ProductId,
                WarehouseId = grn.WarehouseId,
                MovementType = StockMovementType.Receipt,
                Quantity = line.AcceptedQuantity,
                SourceDocumentType = "GoodsReceipt",
                SourceDocumentId = grn.Id,
                SourceDocumentNumber = grn.GrnNumber,
                MovementDate = DateTime.UtcNow,
                Notes = $"Received from supplier: {grn.SupplierName}"
            };
            _context.StockMovements.Add(movement);
        }
    }

    private async Task ReverseStockLevelsAsync(GoodsReceiptNote grn)
    {
        foreach (var line in grn.Lines.Where(l => l.AcceptedQuantity > 0))
        {
            var stockLevel = await _context.StockLevels
                .FirstOrDefaultAsync(s => s.ProductId == line.ProductId && s.WarehouseId == grn.WarehouseId);

            if (stockLevel != null)
            {
                stockLevel.QuantityOnHand -= line.AcceptedQuantity;

                // Create reversal stock movement
                var movement = new StockMovement
                {
                    Id = Guid.NewGuid(),
                    ProductId = line.ProductId,
                    WarehouseId = grn.WarehouseId,
                    MovementType = StockMovementType.Adjustment,
                    Quantity = -line.AcceptedQuantity,
                    SourceDocumentType = "GoodsReceipt",
                    SourceDocumentId = grn.Id,
                    SourceDocumentNumber = grn.GrnNumber,
                    MovementDate = DateTime.UtcNow,
                    Notes = $"Reversal due to GRN cancellation"
                };
                _context.StockMovements.Add(movement);
            }
        }
    }

    private async Task UpdatePurchaseOrderStatusAsync(Guid purchaseOrderId, IEnumerable<GoodsReceiptLine>? currentGrnLines = null)
    {
        var po = await _context.PurchaseOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == purchaseOrderId);

        if (po == null) return;

        var poLineIds = po.Lines.Select(x => x.Id).ToList();

        // Calculate total received across all SAVED GRNs for this PO
        var savedReceivedQuantities = await _context.GoodsReceiptLines
            .Where(l => l.PurchaseOrderLineId.HasValue &&
                       poLineIds.Contains(l.PurchaseOrderLineId.Value))
            .GroupBy(l => l.PurchaseOrderLineId)
            .Select(g => new { PoLineId = g.Key, ReceivedQty = g.Sum(x => x.AcceptedQuantity) })
            .ToDictionaryAsync(x => x.PoLineId!.Value, x => x.ReceivedQty);

        // Add quantities from current unsaved GRN lines (if provided)
        if (currentGrnLines != null)
        {
            foreach (var line in currentGrnLines.Where(l => l.PurchaseOrderLineId.HasValue))
            {
                var poLineId = line.PurchaseOrderLineId!.Value;
                if (savedReceivedQuantities.ContainsKey(poLineId))
                {
                    savedReceivedQuantities[poLineId] += line.AcceptedQuantity;
                }
                else
                {
                    savedReceivedQuantities[poLineId] = line.AcceptedQuantity;
                }
            }
        }

        // Update PO line received quantities
        foreach (var poLine in po.Lines)
        {
            poLine.QuantityReceived = savedReceivedQuantities.GetValueOrDefault(poLine.Id, 0);
        }

        // Determine PO status
        var totalOrdered = po.Lines.Sum(l => l.Quantity);
        var totalReceived = po.Lines.Sum(l => l.QuantityReceived);

        if (totalReceived >= totalOrdered)
        {
            po.Status = PurchaseOrderStatus.Received;
        }
        else if (totalReceived > 0)
        {
            po.Status = PurchaseOrderStatus.PartiallyReceived;
        }
    }
}

public class GrnTableViewModel
{
    public List<GoodsReceiptNote> GoodsReceiptNotes { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}

public class GrnFormViewModel
{
    public bool IsEdit { get; set; }
    public GoodsReceiptNote? GoodsReceiptNote { get; set; }
    public List<Supplier> Suppliers { get; set; } = new();
    public List<Warehouse> Warehouses { get; set; } = new();
    public List<PurchaseOrder> PurchaseOrders { get; set; } = new();
    public PurchaseOrder? SelectedPurchaseOrder { get; set; }
}

public class PurchaseOrderLineWithRemaining
{
    public PurchaseOrderLine Line { get; set; } = null!;
    public decimal RemainingQuantity { get; set; }
}

public class GrnFormInput
{
    public Guid? Id { get; set; }
    public DateTime GrnDate { get; set; } = DateTime.Today;
    public Guid? PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public Guid SupplierId { get; set; }
    public string? SupplierInvoiceNumber { get; set; }
    public DateTime? SupplierInvoiceDate { get; set; }
    public Guid WarehouseId { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? TransporterName { get; set; }
    public string? WayBillNumber { get; set; }
    public string? Notes { get; set; }
    public bool QCRequired { get; set; }
    public List<GrnLineInput>? Lines { get; set; }
}

public class GrnLineInput
{
    public Guid? PurchaseOrderLineId { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSku { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal UnitPrice { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}
