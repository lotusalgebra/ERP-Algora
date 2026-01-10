using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.HR;
using Algora.Erp.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.HR.Attendance;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int PresentToday { get; set; }
    public int AbsentToday { get; set; }
    public int LateToday { get; set; }
    public int WfhToday { get; set; }

    public async Task OnGetAsync()
    {
        var today = DateTime.UtcNow.Date;
        var totalActiveEmployees = await _context.Employees.CountAsync(e => e.EmploymentStatus == EmploymentStatus.Active);

        PresentToday = await _context.Attendances.CountAsync(a => a.Date == today && a.Status == AttendanceStatus.Present);
        LateToday = await _context.Attendances.CountAsync(a => a.Date == today && a.IsLate);
        WfhToday = await _context.Attendances.CountAsync(a => a.Date == today && a.Status == AttendanceStatus.WorkFromHome);

        var recordedToday = await _context.Attendances.CountAsync(a => a.Date == today);
        AbsentToday = totalActiveEmployees - recordedToday;
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, string? dateFilter, int pageNumber = 1, int pageSize = 10)
    {
        var date = string.IsNullOrEmpty(dateFilter) ? DateTime.UtcNow.Date : DateTime.Parse(dateFilter).Date;

        var query = _context.Attendances
            .Include(a => a.Employee)
            .ThenInclude(e => e.Department)
            .Where(a => a.Date == date)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(a =>
                a.Employee.FirstName.ToLower().Contains(search) ||
                a.Employee.LastName.ToLower().Contains(search) ||
                a.Employee.EmployeeCode.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && int.TryParse(statusFilter, out var status))
        {
            query = query.Where(a => (int)a.Status == status);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var attendances = await query
            .OrderBy(a => a.Employee.FirstName)
            .ThenBy(a => a.Employee.LastName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_AttendanceTableRows", new AttendanceTableViewModel
        {
            Attendances = attendances,
            Page = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            DateFilter = dateFilter ?? DateTime.UtcNow.ToString("yyyy-MM-dd")
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var employees = await _context.Employees
            .Where(e => e.EmploymentStatus == EmploymentStatus.Active)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();

        return Partial("_AttendanceForm", new AttendanceFormViewModel
        {
            IsEdit = false,
            Employees = employees
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var attendance = await _context.Attendances
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attendance == null)
            return NotFound();

        var employees = await _context.Employees
            .Where(e => e.EmploymentStatus == EmploymentStatus.Active)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();

        return Partial("_AttendanceForm", new AttendanceFormViewModel
        {
            IsEdit = true,
            Attendance = attendance,
            Employees = employees
        });
    }

    public async Task<IActionResult> OnGetBulkCheckInAsync()
    {
        var today = DateTime.UtcNow.Date;
        var employeesWithoutAttendance = await _context.Employees
            .Where(e => e.EmploymentStatus == EmploymentStatus.Active)
            .Where(e => !_context.Attendances.Any(a => a.EmployeeId == e.Id && a.Date == today))
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();

        return Partial("_BulkCheckInForm", new BulkCheckInViewModel
        {
            Employees = employeesWithoutAttendance,
            Date = today
        });
    }

    public async Task<IActionResult> OnPostAsync(AttendanceFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Domain.Entities.HR.Attendance? attendance;

        if (input.Id.HasValue)
        {
            attendance = await _context.Attendances.FindAsync(input.Id.Value);
            if (attendance == null)
                return NotFound();
        }
        else
        {
            // Check if attendance already exists for this employee on this date
            var exists = await _context.Attendances.AnyAsync(a =>
                a.EmployeeId == input.EmployeeId && a.Date == input.Date);

            if (exists)
            {
                return BadRequest("Attendance record already exists for this employee on this date.");
            }

            attendance = new Domain.Entities.HR.Attendance
            {
                Id = Guid.NewGuid()
            };
            _context.Attendances.Add(attendance);
        }

        attendance.EmployeeId = input.EmployeeId;
        attendance.Date = input.Date;
        attendance.CheckInTime = input.CheckInTime;
        attendance.CheckOutTime = input.CheckOutTime;
        attendance.Status = input.Status;
        attendance.Notes = input.Notes;
        attendance.IsLate = input.IsLate;
        attendance.IsApproved = true;

        // Calculate work hours
        if (input.CheckInTime.HasValue && input.CheckOutTime.HasValue)
        {
            var workHours = input.CheckOutTime.Value - input.CheckInTime.Value;
            if (workHours.TotalHours > 0)
            {
                attendance.TotalWorkHours = workHours;

                // Calculate overtime (assuming 8 hour workday)
                if (workHours.TotalHours > 8)
                {
                    attendance.OvertimeHours = TimeSpan.FromHours(workHours.TotalHours - 8);
                }
            }
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, input.Date.ToString("yyyy-MM-dd"));
    }

    public async Task<IActionResult> OnPostBulkCheckInAsync(BulkCheckInInput input)
    {
        foreach (var employeeId in input.EmployeeIds)
        {
            var exists = await _context.Attendances.AnyAsync(a =>
                a.EmployeeId == employeeId && a.Date == input.Date);

            if (!exists)
            {
                var attendance = new Domain.Entities.HR.Attendance
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = employeeId,
                    Date = input.Date,
                    CheckInTime = input.CheckInTime,
                    Status = input.Status,
                    IsApproved = true
                };
                _context.Attendances.Add(attendance);
            }
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, input.Date.ToString("yyyy-MM-dd"));
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var attendance = await _context.Attendances.FindAsync(id);
        if (attendance == null)
            return NotFound();

        _context.Attendances.Remove(attendance);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, attendance.Date.ToString("yyyy-MM-dd"));
    }
}

public class AttendanceTableViewModel
{
    public List<Domain.Entities.HR.Attendance> Attendances { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public string DateFilter { get; set; } = string.Empty;

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/HR/Attendance",
        Handler = "Table",
        HxTarget = "#tableContent",
        HxInclude = "#searchInput,#statusFilter,#dateFilter"
    };
}

public class AttendanceFormViewModel
{
    public bool IsEdit { get; set; }
    public Domain.Entities.HR.Attendance? Attendance { get; set; }
    public List<Employee> Employees { get; set; } = new();
}

public class AttendanceFormInput
{
    public Guid? Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
    public bool IsLate { get; set; }
}

public class BulkCheckInViewModel
{
    public List<Employee> Employees { get; set; } = new();
    public DateTime Date { get; set; }
}

public class BulkCheckInInput
{
    public List<Guid> EmployeeIds { get; set; } = new();
    public DateTime Date { get; set; }
    public TimeSpan CheckInTime { get; set; }
    public AttendanceStatus Status { get; set; }
}
