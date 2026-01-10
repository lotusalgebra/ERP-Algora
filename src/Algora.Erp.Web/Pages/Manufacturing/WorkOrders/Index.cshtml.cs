using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Manufacturing;
using Algora.Erp.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Manufacturing.WorkOrders;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalWorkOrders { get; set; }
    public int InProgressOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int OverdueOrders { get; set; }

    public List<Product> Products { get; set; } = new();
    public List<BillOfMaterial> BillOfMaterials { get; set; } = new();
    public List<Warehouse> Warehouses { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalWorkOrders = await _context.WorkOrders.CountAsync();
        InProgressOrders = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.InProgress);
        CompletedOrders = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.Completed);
        OverdueOrders = await _context.WorkOrders.CountAsync(w =>
            w.Status != WorkOrderStatus.Completed &&
            w.Status != WorkOrderStatus.Cancelled &&
            w.PlannedEndDate < DateTime.UtcNow);

        Products = await _context.Products.Where(p => p.IsActive).ToListAsync();
        BillOfMaterials = await _context.BillOfMaterials.Where(b => b.Status == BomStatus.Active).ToListAsync();
        Warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, string? priorityFilter, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.WorkOrders
            .Include(w => w.Product)
            .Include(w => w.BillOfMaterial)
            .Include(w => w.Warehouse)
            .Include(w => w.Operations)
            .Include(w => w.Materials)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(w =>
                w.WorkOrderNumber.ToLower().Contains(search) ||
                w.Name.ToLower().Contains(search) ||
                w.Product!.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<WorkOrderStatus>(statusFilter, out var status))
        {
            query = query.Where(w => w.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(priorityFilter) && Enum.TryParse<WorkOrderPriority>(priorityFilter, out var priority))
        {
            query = query.Where(w => w.Priority == priority);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var workOrders = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_WorkOrdersTableRows", new WorkOrderTableViewModel
        {
            WorkOrders = workOrders,
            Pagination = new PaginationViewModel
            {
                Page = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                PageUrl = "/Manufacturing/WorkOrders",
                HxTarget = "#workOrdersTableContainer",
                HxInclude = "#searchInput,#statusFilter"
            }
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var products = await _context.Products.Where(p => p.IsActive).ToListAsync();
        var boms = await _context.BillOfMaterials.Where(b => b.Status == BomStatus.Active).Include(b => b.Product).ToListAsync();
        var warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();

        return Partial("_WorkOrderForm", new WorkOrderFormViewModel
        {
            IsEdit = false,
            Products = products,
            BillOfMaterials = boms,
            Warehouses = warehouses
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Operations)
            .Include(w => w.Materials)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workOrder == null)
            return NotFound();

        var products = await _context.Products.Where(p => p.IsActive).ToListAsync();
        var boms = await _context.BillOfMaterials.Where(b => b.Status == BomStatus.Active).Include(b => b.Product).ToListAsync();
        var warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();

        return Partial("_WorkOrderForm", new WorkOrderFormViewModel
        {
            IsEdit = true,
            WorkOrder = workOrder,
            Products = products,
            BillOfMaterials = boms,
            Warehouses = warehouses
        });
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Product)
            .Include(w => w.BillOfMaterial)
            .Include(w => w.Warehouse)
            .Include(w => w.Operations)
            .Include(w => w.Materials)
                .ThenInclude(m => m.Product)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workOrder == null)
            return NotFound();

        return Partial("_WorkOrderDetails", workOrder);
    }

    public async Task<IActionResult> OnGetBomMaterialsAsync(Guid bomId)
    {
        var bom = await _context.BillOfMaterials
            .Include(b => b.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(b => b.Id == bomId);

        if (bom == null)
            return new JsonResult(new { lines = new List<object>() });

        var lines = bom.Lines.Select(l => new
        {
            productId = l.ProductId,
            productName = l.Product?.Name,
            quantity = l.Quantity,
            unitOfMeasure = l.UnitOfMeasure,
            unitCost = l.UnitCost
        });

        return new JsonResult(new { lines });
    }

    public async Task<IActionResult> OnPostAsync(WorkOrderFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        WorkOrder? workOrder;

        if (input.Id.HasValue)
        {
            workOrder = await _context.WorkOrders
                .Include(w => w.Operations)
                .Include(w => w.Materials)
                .FirstOrDefaultAsync(w => w.Id == input.Id.Value);
            if (workOrder == null)
                return NotFound();

            // Clear existing operations and materials
            foreach (var op in workOrder.Operations.ToList())
            {
                _context.WorkOrderOperations.Remove(op);
            }
            foreach (var mat in workOrder.Materials.ToList())
            {
                _context.WorkOrderMaterials.Remove(mat);
            }
        }
        else
        {
            workOrder = new WorkOrder
            {
                Id = Guid.NewGuid(),
                WorkOrderNumber = await GenerateWorkOrderNumberAsync()
            };
            _context.WorkOrders.Add(workOrder);
        }

        workOrder.Name = input.Name;
        workOrder.BillOfMaterialId = input.BillOfMaterialId;
        workOrder.ProductId = input.ProductId;
        workOrder.PlannedQuantity = input.PlannedQuantity;
        workOrder.UnitOfMeasure = input.UnitOfMeasure;
        workOrder.Status = input.Status;
        workOrder.Priority = input.Priority;
        workOrder.PlannedStartDate = input.PlannedStartDate;
        workOrder.PlannedEndDate = input.PlannedEndDate;
        workOrder.WarehouseId = input.WarehouseId;
        workOrder.EstimatedCost = input.EstimatedCost;
        workOrder.Notes = input.Notes;

        // Add operations
        int opNumber = 1;
        if (input.Operations != null)
        {
            foreach (var opInput in input.Operations.Where(o => !string.IsNullOrWhiteSpace(o.Name)))
            {
                var operation = new WorkOrderOperation
                {
                    Id = Guid.NewGuid(),
                    WorkOrderId = workOrder.Id,
                    OperationNumber = opNumber++,
                    Name = opInput.Name,
                    Description = opInput.Description,
                    Workstation = opInput.Workstation,
                    PlannedHours = opInput.PlannedHours
                };
                _context.WorkOrderOperations.Add(operation);
            }
        }

        // Add materials
        int matNumber = 1;
        if (input.Materials != null)
        {
            foreach (var matInput in input.Materials.Where(m => m.ProductId != Guid.Empty))
            {
                var product = await _context.Products.FindAsync(matInput.ProductId);
                var material = new WorkOrderMaterial
                {
                    Id = Guid.NewGuid(),
                    WorkOrderId = workOrder.Id,
                    ProductId = matInput.ProductId,
                    LineNumber = matNumber++,
                    RequiredQuantity = matInput.RequiredQuantity,
                    UnitOfMeasure = product?.UnitOfMeasure,
                    UnitCost = product?.CostPrice ?? 0
                };
                _context.WorkOrderMaterials.Add(material);
            }
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, WorkOrderStatus status)
    {
        var workOrder = await _context.WorkOrders.FindAsync(id);
        if (workOrder == null)
            return NotFound();

        workOrder.Status = status;

        // Set actual dates based on status
        if (status == WorkOrderStatus.InProgress && !workOrder.ActualStartDate.HasValue)
        {
            workOrder.ActualStartDate = DateTime.UtcNow;
        }
        else if (status == WorkOrderStatus.Completed)
        {
            workOrder.ActualEndDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostRecordOutputAsync(Guid id, decimal completedQty, decimal scrapQty)
    {
        var workOrder = await _context.WorkOrders.FindAsync(id);
        if (workOrder == null)
            return NotFound();

        workOrder.CompletedQuantity += completedQty;
        workOrder.ScrapQuantity += scrapQty;

        // Auto-complete if all quantity produced
        if (workOrder.CompletedQuantity >= workOrder.PlannedQuantity)
        {
            workOrder.Status = WorkOrderStatus.Completed;
            workOrder.ActualEndDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Operations)
            .Include(w => w.Materials)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workOrder == null)
            return NotFound();

        if (workOrder.Status != WorkOrderStatus.Draft)
        {
            return BadRequest("Only draft work orders can be deleted.");
        }

        _context.WorkOrders.Remove(workOrder);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    private async Task<string> GenerateWorkOrderNumberAsync()
    {
        var lastWo = await _context.WorkOrders
            .IgnoreQueryFilters()
            .OrderByDescending(w => w.WorkOrderNumber)
            .FirstOrDefaultAsync(w => w.WorkOrderNumber.StartsWith("WO"));

        if (lastWo == null)
            return "WO00001";

        var lastNumber = int.Parse(lastWo.WorkOrderNumber.Substring(2));
        return $"WO{(lastNumber + 1):D5}";
    }
}

public class WorkOrderTableViewModel
{
    public List<WorkOrder> WorkOrders { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
}

public class WorkOrderFormViewModel
{
    public bool IsEdit { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public List<Product> Products { get; set; } = new();
    public List<BillOfMaterial> BillOfMaterials { get; set; } = new();
    public List<Warehouse> Warehouses { get; set; } = new();
}

public class WorkOrderFormInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? BillOfMaterialId { get; set; }
    public Guid ProductId { get; set; }
    public decimal PlannedQuantity { get; set; } = 1;
    public string? UnitOfMeasure { get; set; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;
    public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Normal;
    public DateTime PlannedStartDate { get; set; } = DateTime.Today;
    public DateTime PlannedEndDate { get; set; } = DateTime.Today.AddDays(7);
    public Guid? WarehouseId { get; set; }
    public decimal EstimatedCost { get; set; }
    public string? Notes { get; set; }
    public List<WorkOrderOperationInput>? Operations { get; set; }
    public List<WorkOrderMaterialInput>? Materials { get; set; }
}

public class WorkOrderOperationInput
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Workstation { get; set; }
    public decimal PlannedHours { get; set; }
}

public class WorkOrderMaterialInput
{
    public Guid ProductId { get; set; }
    public decimal RequiredQuantity { get; set; }
}
