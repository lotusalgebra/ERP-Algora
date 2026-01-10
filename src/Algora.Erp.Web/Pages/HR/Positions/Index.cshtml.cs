using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.HR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.HR.Positions;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalPositions { get; set; }
    public int ActivePositions { get; set; }
    public int TotalEmployeesInPositions { get; set; }
    public decimal AvgSalaryRange { get; set; }
    public List<Department> DepartmentFilter { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalPositions = await _context.Positions.CountAsync();
        ActivePositions = await _context.Positions.CountAsync(p => p.IsActive);
        TotalEmployeesInPositions = await _context.Employees.CountAsync(e => e.PositionId != null);

        var positions = await _context.Positions.Where(p => p.MinSalary.HasValue && p.MaxSalary.HasValue).ToListAsync();
        AvgSalaryRange = positions.Any() ? positions.Average(p => (p.MinSalary!.Value + p.MaxSalary!.Value) / 2) : 0;

        DepartmentFilter = await _context.Departments.Where(d => d.IsActive).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, string? departmentFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.Positions
            .Include(p => p.Department)
            .Include(p => p.Employees)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(search) ||
                p.Code.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && bool.TryParse(statusFilter, out var isActive))
        {
            query = query.Where(p => p.IsActive == isActive);
        }

        if (!string.IsNullOrWhiteSpace(departmentFilter) && Guid.TryParse(departmentFilter, out var deptId))
        {
            query = query.Where(p => p.DepartmentId == deptId);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var positions = await query
            .OrderBy(p => p.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_PositionsTableRows", new PositionsTableViewModel
        {
            Positions = positions,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();

        return Partial("_PositionForm", new PositionFormViewModel
        {
            IsEdit = false,
            Departments = departments
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var position = await _context.Positions
            .Include(p => p.Department)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (position == null)
            return NotFound();

        var departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();

        return Partial("_PositionForm", new PositionFormViewModel
        {
            IsEdit = true,
            Position = position,
            Departments = departments
        });
    }

    public async Task<IActionResult> OnPostAsync(PositionFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Position? position;

        if (input.Id.HasValue)
        {
            position = await _context.Positions.FindAsync(input.Id.Value);
            if (position == null)
                return NotFound();
        }
        else
        {
            position = new Position
            {
                Id = Guid.NewGuid(),
                Code = await GeneratePositionCodeAsync()
            };
            _context.Positions.Add(position);
        }

        position.Title = input.Title;
        position.Description = input.Description;
        position.DepartmentId = input.DepartmentId;
        position.MinSalary = input.MinSalary;
        position.MaxSalary = input.MaxSalary;
        position.IsActive = input.IsActive;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var position = await _context.Positions.FindAsync(id);
        if (position == null)
            return NotFound();

        var hasEmployees = await _context.Employees.AnyAsync(e => e.PositionId == id);
        if (hasEmployees)
        {
            return BadRequest("Cannot delete position with employees assigned.");
        }

        _context.Positions.Remove(position);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    private async Task<string> GeneratePositionCodeAsync()
    {
        var lastPosition = await _context.Positions
            .IgnoreQueryFilters()
            .OrderByDescending(p => p.Code)
            .FirstOrDefaultAsync();

        if (lastPosition == null)
            return "POS001";

        var lastNumber = int.Parse(lastPosition.Code.Replace("POS", ""));
        return $"POS{(lastNumber + 1):D3}";
    }
}

public class PositionsTableViewModel
{
    public List<Position> Positions { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/HR/Positions",
        Handler = "Table",
        HxTarget = "#tableContent",
        HxInclude = "#searchInput,#statusFilter,#departmentFilter"
    };
}

public class PositionFormViewModel
{
    public bool IsEdit { get; set; }
    public Position? Position { get; set; }
    public List<Department> Departments { get; set; } = new();
}

public class PositionFormInput
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? DepartmentId { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public bool IsActive { get; set; } = true;
}
