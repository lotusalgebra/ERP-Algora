using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.HR.Departments;

[Authorize(Policy = "CanViewHR")]
[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalDepartments { get; set; }
    public int ActiveDepartments { get; set; }
    public int TotalEmployeesInDepartments { get; set; }
    public int SubDepartments { get; set; }

    public async Task OnGetAsync()
    {
        TotalDepartments = await _context.Departments.CountAsync();
        ActiveDepartments = await _context.Departments.CountAsync(d => d.IsActive);
        TotalEmployeesInDepartments = await _context.Employees.CountAsync(e => e.DepartmentId != null);
        SubDepartments = await _context.Departments.CountAsync(d => d.ParentDepartmentId != null);
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.Employees)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(d =>
                d.Name.ToLower().Contains(search) ||
                d.Code.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && bool.TryParse(statusFilter, out var isActive))
        {
            query = query.Where(d => d.IsActive == isActive);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var departments = await query
            .OrderBy(d => d.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_DepartmentsTableRows", new DepartmentsTableViewModel
        {
            Departments = departments,
            Page = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();
        var employees = await _context.Employees
            .Where(e => e.EmploymentStatus == Domain.Enums.EmploymentStatus.Active)
            .Select(e => new { e.Id, Name = e.FirstName + " " + e.LastName })
            .ToListAsync();

        return Partial("_DepartmentForm", new DepartmentFormViewModel
        {
            IsEdit = false,
            ParentDepartments = departments,
            Managers = employees.ToDictionary(e => e.Id, e => e.Name)
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var department = await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
            return NotFound();

        var departments = await _context.Departments
            .Where(d => d.IsActive && d.Id != id)
            .ToListAsync();

        var employees = await _context.Employees
            .Where(e => e.EmploymentStatus == Domain.Enums.EmploymentStatus.Active)
            .Select(e => new { e.Id, Name = e.FirstName + " " + e.LastName })
            .ToListAsync();

        return Partial("_DepartmentForm", new DepartmentFormViewModel
        {
            IsEdit = true,
            Department = department,
            ParentDepartments = departments,
            Managers = employees.ToDictionary(e => e.Id, e => e.Name)
        });
    }

    public async Task<IActionResult> OnPostAsync(DepartmentFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Department? department;

        if (input.Id.HasValue)
        {
            department = await _context.Departments.FindAsync(input.Id.Value);
            if (department == null)
                return NotFound();
        }
        else
        {
            department = new Department
            {
                Id = Guid.NewGuid(),
                Code = await GenerateDepartmentCodeAsync()
            };
            _context.Departments.Add(department);
        }

        department.Name = input.Name;
        department.Description = input.Description;
        department.ParentDepartmentId = input.ParentDepartmentId;
        department.ManagerId = input.ManagerId;
        department.IsActive = input.IsActive;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
            return NotFound();

        // Check if department has employees
        var hasEmployees = await _context.Employees.AnyAsync(e => e.DepartmentId == id);
        if (hasEmployees)
        {
            return BadRequest("Cannot delete department with employees assigned.");
        }

        // Check if department has sub-departments
        var hasSubDepartments = await _context.Departments.AnyAsync(d => d.ParentDepartmentId == id);
        if (hasSubDepartments)
        {
            return BadRequest("Cannot delete department with sub-departments.");
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    private async Task<string> GenerateDepartmentCodeAsync()
    {
        var lastDepartment = await _context.Departments
            .IgnoreQueryFilters()
            .OrderByDescending(d => d.Code)
            .FirstOrDefaultAsync();

        if (lastDepartment == null)
            return "DEPT001";

        var lastNumber = int.Parse(lastDepartment.Code.Replace("DEPT", ""));
        return $"DEPT{(lastNumber + 1):D3}";
    }
}

public class DepartmentsTableViewModel
{
    public List<Department> Departments { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/HR/Departments",
        Handler = "Table",
        HxTarget = "#tableContent",
        HxInclude = "#searchInput,#statusFilter"
    };
}

public class DepartmentFormViewModel
{
    public bool IsEdit { get; set; }
    public Department? Department { get; set; }
    public List<Department> ParentDepartments { get; set; } = new();
    public Dictionary<Guid, string> Managers { get; set; } = new();
}

public class DepartmentFormInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
    public bool IsActive { get; set; } = true;
}
