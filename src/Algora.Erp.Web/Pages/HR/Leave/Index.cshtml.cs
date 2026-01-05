using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.HR;
using Algora.Erp.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.HR.Leave;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int OnLeaveToday { get; set; }

    public async Task OnGetAsync()
    {
        var today = DateTime.UtcNow.Date;

        PendingRequests = await _context.LeaveRequests.CountAsync(l => l.Status == LeaveStatus.Pending);
        ApprovedRequests = await _context.LeaveRequests.CountAsync(l => l.Status == LeaveStatus.Approved);
        RejectedRequests = await _context.LeaveRequests.CountAsync(l => l.Status == LeaveStatus.Rejected);
        OnLeaveToday = await _context.LeaveRequests.CountAsync(l =>
            l.Status == LeaveStatus.Approved &&
            l.StartDate <= today &&
            l.EndDate >= today);
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, string? typeFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.LeaveRequests
            .Include(l => l.Employee)
            .ThenInclude(e => e.Department)
            .Include(l => l.Approver)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(l =>
                l.Employee.FirstName.ToLower().Contains(search) ||
                l.Employee.LastName.ToLower().Contains(search) ||
                l.Employee.EmployeeCode.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && int.TryParse(statusFilter, out var status))
        {
            query = query.Where(l => (int)l.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(typeFilter) && int.TryParse(typeFilter, out var type))
        {
            query = query.Where(l => (int)l.LeaveType == type);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var leaveRequests = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_LeaveTableRows", new LeaveTableViewModel
        {
            LeaveRequests = leaveRequests,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var employees = await _context.Employees
            .Where(e => e.EmploymentStatus == EmploymentStatus.Active)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();

        return Partial("_LeaveForm", new LeaveFormViewModel
        {
            IsEdit = false,
            Employees = employees
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (leaveRequest == null)
            return NotFound();

        var employees = await _context.Employees
            .Where(e => e.EmploymentStatus == EmploymentStatus.Active)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();

        return Partial("_LeaveForm", new LeaveFormViewModel
        {
            IsEdit = true,
            LeaveRequest = leaveRequest,
            Employees = employees
        });
    }

    public async Task<IActionResult> OnPostAsync(LeaveFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        LeaveRequest? leaveRequest;

        if (input.Id.HasValue)
        {
            leaveRequest = await _context.LeaveRequests.FindAsync(input.Id.Value);
            if (leaveRequest == null)
                return NotFound();
        }
        else
        {
            leaveRequest = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                Status = LeaveStatus.Pending
            };
            _context.LeaveRequests.Add(leaveRequest);
        }

        leaveRequest.EmployeeId = input.EmployeeId;
        leaveRequest.LeaveType = input.LeaveType;
        leaveRequest.StartDate = input.StartDate;
        leaveRequest.EndDate = input.EndDate;
        leaveRequest.IsHalfDay = input.IsHalfDay;
        leaveRequest.HalfDayType = input.HalfDayType;
        leaveRequest.Reason = input.Reason;
        leaveRequest.EmergencyContact = input.EmergencyContact;
        leaveRequest.HandoverNotes = input.HandoverNotes;

        // Calculate total days
        if (input.IsHalfDay)
        {
            leaveRequest.TotalDays = 0.5m;
        }
        else
        {
            var days = (input.EndDate - input.StartDate).Days + 1;
            leaveRequest.TotalDays = days;
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        var leaveRequest = await _context.LeaveRequests.FindAsync(id);
        if (leaveRequest == null)
            return NotFound();

        leaveRequest.Status = LeaveStatus.Approved;
        leaveRequest.ApprovedAt = DateTime.UtcNow;

        // Update leave balance
        var balance = await _context.LeaveBalances.FirstOrDefaultAsync(b =>
            b.EmployeeId == leaveRequest.EmployeeId &&
            b.Year == DateTime.UtcNow.Year &&
            b.LeaveType == leaveRequest.LeaveType);

        if (balance != null)
        {
            balance.Pending -= leaveRequest.TotalDays;
            balance.Used += leaveRequest.TotalDays;
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id, string? rejectionReason)
    {
        var leaveRequest = await _context.LeaveRequests.FindAsync(id);
        if (leaveRequest == null)
            return NotFound();

        leaveRequest.Status = LeaveStatus.Rejected;
        leaveRequest.RejectionReason = rejectionReason;

        // Update leave balance
        var balance = await _context.LeaveBalances.FirstOrDefaultAsync(b =>
            b.EmployeeId == leaveRequest.EmployeeId &&
            b.Year == DateTime.UtcNow.Year &&
            b.LeaveType == leaveRequest.LeaveType);

        if (balance != null)
        {
            balance.Pending -= leaveRequest.TotalDays;
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var leaveRequest = await _context.LeaveRequests.FindAsync(id);
        if (leaveRequest == null)
            return NotFound();

        _context.LeaveRequests.Remove(leaveRequest);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }
}

public class LeaveTableViewModel
{
    public List<LeaveRequest> LeaveRequests { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}

public class LeaveFormViewModel
{
    public bool IsEdit { get; set; }
    public LeaveRequest? LeaveRequest { get; set; }
    public List<Employee> Employees { get; set; } = new();
}

public class LeaveFormInput
{
    public Guid? Id { get; set; }
    public Guid EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsHalfDay { get; set; }
    public HalfDayType? HalfDayType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? EmergencyContact { get; set; }
    public string? HandoverNotes { get; set; }
}
