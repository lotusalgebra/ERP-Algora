using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Payroll;
using Algora.Erp.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Payroll.Components;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalComponents { get; set; }
    public int EarningsCount { get; set; }
    public int DeductionsCount { get; set; }
    public int ActiveCount { get; set; }

    public async Task OnGetAsync()
    {
        TotalComponents = await _context.SalaryComponents.CountAsync();
        EarningsCount = await _context.SalaryComponents.CountAsync(c => c.ComponentType == SalaryComponentType.Earning);
        DeductionsCount = await _context.SalaryComponents.CountAsync(c => c.ComponentType == SalaryComponentType.Deduction);
        ActiveCount = await _context.SalaryComponents.CountAsync(c => c.IsActive);
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? typeFilter, string? statusFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.SalaryComponents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c =>
                c.Code.ToLower().Contains(search) ||
                c.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(typeFilter) && Enum.TryParse<SalaryComponentType>(typeFilter, out var type))
        {
            query = query.Where(c => c.ComponentType == type);
        }

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            var isActive = statusFilter == "true";
            query = query.Where(c => c.IsActive == isActive);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var components = await query
            .OrderBy(c => c.ComponentType).ThenBy(c => c.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_ComponentsTableRows", new ComponentsTableViewModel
        {
            Components = components,
            Pagination = new PaginationViewModel
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                PageUrl = "/Payroll/Components",
                HxTarget = "#componentsTableBody",
                HxInclude = "#searchInput,#typeFilter"
            }
        });
    }

    public IActionResult OnGetCreateForm()
    {
        return Partial("_ComponentForm", new ComponentFormViewModel { IsEdit = false });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var component = await _context.SalaryComponents.FindAsync(id);
        if (component == null)
            return NotFound();

        return Partial("_ComponentForm", new ComponentFormViewModel
        {
            IsEdit = true,
            Component = component
        });
    }

    public async Task<IActionResult> OnPostAsync(ComponentFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        SalaryComponent? component;

        if (input.Id.HasValue)
        {
            component = await _context.SalaryComponents.FindAsync(input.Id.Value);
            if (component == null)
                return NotFound();
        }
        else
        {
            component = new SalaryComponent
            {
                Id = Guid.NewGuid(),
                Code = await GenerateCodeAsync()
            };
            _context.SalaryComponents.Add(component);
        }

        component.Name = input.Name;
        component.Description = input.Description;
        component.ComponentType = input.ComponentType;
        component.CalculationType = input.CalculationType;
        component.DefaultValue = input.DefaultValue;
        component.MinValue = input.MinValue;
        component.MaxValue = input.MaxValue;
        component.IsTaxable = input.IsTaxable;
        component.IsRecurring = input.IsRecurring;
        component.IsActive = input.IsActive;
        component.SortOrder = input.SortOrder;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var component = await _context.SalaryComponents.FindAsync(id);
        if (component == null)
            return NotFound();

        // Check if used in any salary structure
        var isUsed = await _context.SalaryStructureLines.AnyAsync(l => l.SalaryComponentId == id);
        if (isUsed)
        {
            return BadRequest("Cannot delete component that is used in salary structures.");
        }

        _context.SalaryComponents.Remove(component);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    private async Task<string> GenerateCodeAsync()
    {
        var lastComponent = await _context.SalaryComponents
            .IgnoreQueryFilters()
            .OrderByDescending(c => c.Code)
            .FirstOrDefaultAsync(c => c.Code.StartsWith("SC"));

        if (lastComponent == null)
            return "SC001";

        var lastNumber = int.Parse(lastComponent.Code.Substring(2));
        return $"SC{(lastNumber + 1):D3}";
    }
}

public class ComponentsTableViewModel
{
    public List<SalaryComponent> Components { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
}

public class ComponentFormViewModel
{
    public bool IsEdit { get; set; }
    public SalaryComponent? Component { get; set; }
}

public class ComponentFormInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SalaryComponentType ComponentType { get; set; } = SalaryComponentType.Earning;
    public CalculationType CalculationType { get; set; } = CalculationType.Fixed;
    public decimal DefaultValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public bool IsTaxable { get; set; } = true;
    public bool IsRecurring { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
