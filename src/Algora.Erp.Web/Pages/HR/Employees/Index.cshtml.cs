using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.HR;
using Algora.Erp.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.HR.Employees;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int OnLeaveEmployees { get; set; }
    public int NewThisMonth { get; set; }

    public async Task OnGetAsync()
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        TotalEmployees = await _context.Employees.CountAsync();
        ActiveEmployees = await _context.Employees.CountAsync(e => e.EmploymentStatus == EmploymentStatus.Active);
        OnLeaveEmployees = await _context.Employees.CountAsync(e => e.EmploymentStatus == EmploymentStatus.OnLeave);
        NewThisMonth = await _context.Employees.CountAsync(e => e.HireDate >= startOfMonth);
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(search) ||
                e.LastName.ToLower().Contains(search) ||
                e.Email.ToLower().Contains(search) ||
                e.EmployeeCode.ToLower().Contains(search));
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<EmploymentStatus>(statusFilter, out var status))
        {
            query = query.Where(e => e.EmploymentStatus == status);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var employees = await query
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_EmployeesTableRows", new EmployeesTableViewModel
        {
            Employees = employees,
            Page = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();
        var positions = await _context.Positions.Where(p => p.IsActive).ToListAsync();

        return Partial("_EmployeeForm", new EmployeeFormViewModel
        {
            IsEdit = false,
            Departments = departments,
            Positions = positions
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return NotFound();

        var departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();
        var positions = await _context.Positions.Where(p => p.IsActive).ToListAsync();
        var managers = await _context.Employees
            .Where(e => e.Id != id && e.EmploymentStatus == EmploymentStatus.Active)
            .Select(e => new { e.Id, Name = e.FirstName + " " + e.LastName })
            .ToListAsync();

        return Partial("_EmployeeForm", new EmployeeFormViewModel
        {
            IsEdit = true,
            Employee = employee,
            Departments = departments,
            Positions = positions,
            Managers = managers.ToDictionary(m => m.Id, m => m.Name)
        });
    }

    public async Task<IActionResult> OnPostAsync(EmployeeFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Employee? employee;

        if (input.Id.HasValue)
        {
            // Update existing
            employee = await _context.Employees.FindAsync(input.Id.Value);
            if (employee == null)
                return NotFound();
        }
        else
        {
            // Create new
            employee = new Employee
            {
                Id = Guid.NewGuid(),
                EmployeeCode = await GenerateEmployeeCodeAsync()
            };
            _context.Employees.Add(employee);
        }

        // Map input to employee
        employee.FirstName = input.FirstName;
        employee.LastName = input.LastName;
        employee.Email = input.Email;
        employee.Phone = input.Phone;
        employee.Mobile = input.Mobile;
        employee.DateOfBirth = input.DateOfBirth;
        employee.Gender = input.Gender;
        employee.Address = input.Address;
        employee.City = input.City;
        employee.State = input.State;
        employee.Country = input.Country;
        employee.PostalCode = input.PostalCode;
        employee.DepartmentId = input.DepartmentId;
        employee.PositionId = input.PositionId;
        employee.ManagerId = input.ManagerId;
        employee.HireDate = input.HireDate;
        employee.EmploymentType = input.EmploymentType;
        employee.EmploymentStatus = input.EmploymentStatus;
        employee.BaseSalary = input.BaseSalary;
        employee.SalaryCurrency = input.SalaryCurrency ?? "USD";
        employee.PayFrequency = input.PayFrequency;
        employee.EmergencyContactName = input.EmergencyContactName;
        employee.EmergencyContactPhone = input.EmergencyContactPhone;
        employee.EmergencyContactRelation = input.EmergencyContactRelation;

        await _context.SaveChangesAsync();

        // Return updated table
        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound();

        _context.Employees.Remove(employee); // Soft delete via SaveChangesAsync override
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    private async Task<string> GenerateEmployeeCodeAsync()
    {
        var lastEmployee = await _context.Employees
            .IgnoreQueryFilters()
            .OrderByDescending(e => e.EmployeeCode)
            .FirstOrDefaultAsync();

        if (lastEmployee == null)
            return "EMP001";

        var lastNumber = int.Parse(lastEmployee.EmployeeCode.Replace("EMP", ""));
        return $"EMP{(lastNumber + 1):D3}";
    }
}

public class EmployeesTableViewModel
{
    public List<Employee> Employees { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/HR/Employees",
        Handler = "Table",
        HxTarget = "#employeesTableBody",
        HxInclude = "#searchInput,#statusFilter"
    };
}

public class EmployeeFormViewModel
{
    public bool IsEdit { get; set; }
    public Employee? Employee { get; set; }
    public List<Department> Departments { get; set; } = new();
    public List<Position> Positions { get; set; } = new();
    public Dictionary<Guid, string> Managers { get; set; } = new();
}

public class EmployeeFormInput
{
    public Guid? Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? ManagerId { get; set; }
    public DateTime HireDate { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    public decimal? BaseSalary { get; set; }
    public string? SalaryCurrency { get; set; }
    public PayFrequency PayFrequency { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
}
