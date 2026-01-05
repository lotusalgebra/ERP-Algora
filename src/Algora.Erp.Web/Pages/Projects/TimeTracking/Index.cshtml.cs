using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.HR;
using Algora.Erp.Domain.Entities.Projects;
using Algora.Erp.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectTaskStatus = Algora.Erp.Domain.Entities.Projects.TaskStatus;

namespace Algora.Erp.Web.Pages.Projects.TimeTracking;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalEntries { get; set; }
    public decimal TotalHours { get; set; }
    public decimal BillableHours { get; set; }
    public int PendingApproval { get; set; }

    public List<Project> Projects { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalEntries = await _context.TimeEntries.CountAsync();
        TotalHours = await _context.TimeEntries.SumAsync(t => t.Hours);
        BillableHours = await _context.TimeEntries.Where(t => t.IsBillable).SumAsync(t => t.Hours);
        PendingApproval = await _context.TimeEntries.CountAsync(t => t.Status == TimeEntryStatus.Submitted);

        Projects = await _context.Projects.Where(p => p.IsActive).ToListAsync();
        Employees = await _context.Employees.Where(e => e.EmploymentStatus == EmploymentStatus.Active).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, Guid? projectId, string? statusFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.TimeEntries
            .Include(t => t.Project)
            .Include(t => t.Task)
            .Include(t => t.Employee)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(t =>
                t.Description != null && t.Description.ToLower().Contains(search) ||
                t.Project!.Name.ToLower().Contains(search) ||
                (t.Employee != null && (t.Employee.FirstName.ToLower().Contains(search) || t.Employee.LastName.ToLower().Contains(search))));
        }

        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<TimeEntryStatus>(statusFilter, out var status))
        {
            query = query.Where(t => t.Status == status);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var entries = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_TimeEntriesTableRows", new TimeEntryTableViewModel
        {
            TimeEntries = entries,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var projects = await _context.Projects.Where(p => p.IsActive).ToListAsync();
        var employees = await _context.Employees.Where(e => e.EmploymentStatus == EmploymentStatus.Active).ToListAsync();

        return Partial("_TimeEntryForm", new TimeEntryFormViewModel
        {
            IsEdit = false,
            Projects = projects,
            Employees = employees
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var entry = await _context.TimeEntries.FirstOrDefaultAsync(t => t.Id == id);

        if (entry == null)
            return NotFound();

        var projects = await _context.Projects.Where(p => p.IsActive).ToListAsync();
        var employees = await _context.Employees.Where(e => e.EmploymentStatus == EmploymentStatus.Active).ToListAsync();
        var tasks = await _context.ProjectTasks.Where(t => t.ProjectId == entry.ProjectId).ToListAsync();

        return Partial("_TimeEntryForm", new TimeEntryFormViewModel
        {
            IsEdit = true,
            TimeEntry = entry,
            Projects = projects,
            Employees = employees,
            Tasks = tasks
        });
    }

    public async Task<IActionResult> OnGetProjectTasksAsync(Guid projectId)
    {
        var tasks = await _context.ProjectTasks
            .Where(t => t.ProjectId == projectId && t.Status != ProjectTaskStatus.Completed && t.Status != ProjectTaskStatus.Cancelled)
            .Select(t => new { t.Id, t.TaskNumber, t.Title })
            .ToListAsync();

        return new JsonResult(tasks);
    }

    public async Task<IActionResult> OnPostAsync(TimeEntryFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        TimeEntry? entry;

        if (input.Id.HasValue)
        {
            entry = await _context.TimeEntries.FirstOrDefaultAsync(t => t.Id == input.Id.Value);
            if (entry == null)
                return NotFound();
        }
        else
        {
            entry = new TimeEntry
            {
                Id = Guid.NewGuid()
            };
            _context.TimeEntries.Add(entry);
        }

        entry.ProjectId = input.ProjectId;
        entry.TaskId = input.TaskId;
        entry.EmployeeId = input.EmployeeId;
        entry.Date = input.Date;
        entry.StartTime = input.StartTime;
        entry.EndTime = input.EndTime;
        entry.Hours = input.Hours;
        entry.Description = input.Description;
        entry.IsBillable = input.IsBillable;
        entry.Status = input.Status;
        entry.Notes = input.Notes;

        // Update task actual hours
        if (input.TaskId.HasValue)
        {
            var task = await _context.ProjectTasks.FindAsync(input.TaskId.Value);
            if (task != null)
            {
                task.ActualHours = await _context.TimeEntries
                    .Where(t => t.TaskId == input.TaskId.Value)
                    .SumAsync(t => t.Hours);
            }
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, TimeEntryStatus status)
    {
        var entry = await _context.TimeEntries.FindAsync(id);
        if (entry == null)
            return NotFound();

        entry.Status = status;
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var entry = await _context.TimeEntries.FirstOrDefaultAsync(t => t.Id == id);

        if (entry == null)
            return NotFound();

        if (entry.Status != TimeEntryStatus.Draft)
        {
            return BadRequest("Only draft time entries can be deleted.");
        }

        _context.TimeEntries.Remove(entry);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }
}

public class TimeEntryTableViewModel
{
    public List<TimeEntry> TimeEntries { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}

public class TimeEntryFormViewModel
{
    public bool IsEdit { get; set; }
    public TimeEntry? TimeEntry { get; set; }
    public List<Project> Projects { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();
    public List<ProjectTask> Tasks { get; set; } = new();
}

public class TimeEntryFormInput
{
    public Guid? Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public bool IsBillable { get; set; } = true;
    public TimeEntryStatus Status { get; set; } = TimeEntryStatus.Draft;
    public string? Notes { get; set; }
}
