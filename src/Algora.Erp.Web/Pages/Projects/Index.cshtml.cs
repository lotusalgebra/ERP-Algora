using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.HR;
using Algora.Erp.Domain.Entities.Projects;
using Algora.Erp.Domain.Entities.Sales;
using Algora.Erp.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Projects;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public decimal TotalBudget { get; set; }

    public List<Customer> Customers { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalProjects = await _context.Projects.CountAsync();
        ActiveProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Active);
        CompletedProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Completed);
        TotalBudget = await _context.Projects.Where(p => p.IsActive).SumAsync(p => p.BudgetAmount);

        Customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
        Employees = await _context.Employees.Where(e => e.EmploymentStatus == EmploymentStatus.Active).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.Projects
            .Include(p => p.Customer)
            .Include(p => p.ProjectManager)
            .Include(p => p.Tasks)
            .Include(p => p.Members)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(p =>
                p.ProjectCode.ToLower().Contains(search) ||
                p.Name.ToLower().Contains(search) ||
                (p.Customer != null && p.Customer.Name.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<ProjectStatus>(statusFilter, out var status))
        {
            query = query.Where(p => p.Status == status);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var projects = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_ProjectsTableRows", new ProjectTableViewModel
        {
            Projects = projects,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
        var employees = await _context.Employees.Where(e => e.EmploymentStatus == EmploymentStatus.Active).ToListAsync();

        return Partial("_ProjectForm", new ProjectFormViewModel
        {
            IsEdit = false,
            Customers = customers,
            Employees = employees
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .Include(p => p.Milestones)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            return NotFound();

        var customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
        var employees = await _context.Employees.Where(e => e.EmploymentStatus == EmploymentStatus.Active).ToListAsync();

        return Partial("_ProjectForm", new ProjectFormViewModel
        {
            IsEdit = true,
            Project = project,
            Customers = customers,
            Employees = employees
        });
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var project = await _context.Projects
            .Include(p => p.Customer)
            .Include(p => p.ProjectManager)
            .Include(p => p.Tasks)
                .ThenInclude(t => t.Assignee)
            .Include(p => p.Members)
                .ThenInclude(m => m.Employee)
            .Include(p => p.Milestones)
            .Include(p => p.TimeEntries)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            return NotFound();

        return Partial("_ProjectDetails", project);
    }

    public async Task<IActionResult> OnPostAsync(ProjectFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Project? project;

        if (input.Id.HasValue)
        {
            project = await _context.Projects
                .Include(p => p.Members)
                .Include(p => p.Milestones)
                .FirstOrDefaultAsync(p => p.Id == input.Id.Value);
            if (project == null)
                return NotFound();
        }
        else
        {
            project = new Project
            {
                Id = Guid.NewGuid(),
                ProjectCode = await GenerateProjectCodeAsync()
            };
            _context.Projects.Add(project);
        }

        project.Name = input.Name;
        project.Description = input.Description;
        project.CustomerId = input.CustomerId;
        project.ProjectManagerId = input.ProjectManagerId;
        project.Status = input.Status;
        project.Priority = input.Priority;
        project.StartDate = input.StartDate;
        project.EndDate = input.EndDate;
        project.BudgetAmount = input.BudgetAmount;
        project.IsActive = input.IsActive;
        project.Notes = input.Notes;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, ProjectStatus status)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null)
            return NotFound();

        project.Status = status;

        if (status == ProjectStatus.Active && !project.ActualStartDate.HasValue)
        {
            project.ActualStartDate = DateTime.UtcNow;
        }
        else if (status == ProjectStatus.Completed)
        {
            project.ActualEndDate = DateTime.UtcNow;
            project.Progress = 100;
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var project = await _context.Projects
            .Include(p => p.Tasks)
            .Include(p => p.Members)
            .Include(p => p.TimeEntries)
            .Include(p => p.Milestones)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            return NotFound();

        if (project.Status != ProjectStatus.Planning)
        {
            return BadRequest("Only projects in planning status can be deleted.");
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    private async Task<string> GenerateProjectCodeAsync()
    {
        var lastProject = await _context.Projects
            .IgnoreQueryFilters()
            .OrderByDescending(p => p.ProjectCode)
            .FirstOrDefaultAsync(p => p.ProjectCode.StartsWith("PRJ"));

        if (lastProject == null)
            return "PRJ00001";

        var lastNumber = int.Parse(lastProject.ProjectCode.Substring(3));
        return $"PRJ{(lastNumber + 1):D5}";
    }
}

public class ProjectTableViewModel
{
    public List<Project> Projects { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/Projects",
        Handler = "Table",
        HxTarget = "#projectsTableContainer",
        HxInclude = "#searchInput,#statusFilter"
    };
}

public class ProjectFormViewModel
{
    public bool IsEdit { get; set; }
    public Project? Project { get; set; }
    public List<Customer> Customers { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();
}

public class ProjectFormInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? ProjectManagerId { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public ProjectPriority Priority { get; set; } = ProjectPriority.Normal;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }
    public decimal BudgetAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}
