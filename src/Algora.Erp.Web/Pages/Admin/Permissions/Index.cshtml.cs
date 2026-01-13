using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Admin.Permissions;

[Authorize(Policy = "CanManageAdmin")]
[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalPermissions { get; set; }
    public List<string> Modules { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalPermissions = await _context.Permissions.CountAsync();
        Modules = await _context.Permissions.Select(p => p.Module).Distinct().OrderBy(m => m).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? moduleFilter, int pageNumber = 1, int pageSize = 15)
    {
        var query = _context.Permissions
            .Include(p => p.RolePermissions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Code.ToLower().Contains(search) ||
                (p.Description != null && p.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(moduleFilter))
        {
            query = query.Where(p => p.Module == moduleFilter);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var permissions = await query
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_PermissionsTableRows", new PermissionsTableViewModel
        {
            Permissions = permissions,
            Page = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public IActionResult OnGetCreateForm()
    {
        return Partial("_PermissionForm", new PermissionFormViewModel { IsEdit = false });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null) return NotFound();

        return Partial("_PermissionForm", new PermissionFormViewModel
        {
            IsEdit = true,
            Permission = permission
        });
    }

    public async Task<IActionResult> OnPostAsync(PermissionFormInput input)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        Permission? permission;

        if (input.Id.HasValue)
        {
            permission = await _context.Permissions.FindAsync(input.Id.Value);
            if (permission == null) return NotFound();
        }
        else
        {
            // Check if code already exists
            if (await _context.Permissions.AnyAsync(p => p.Code == input.Code))
            {
                return BadRequest("Permission code already exists");
            }

            permission = new Permission { Id = Guid.NewGuid() };
            _context.Permissions.Add(permission);
        }

        permission.Code = input.Code;
        permission.Name = input.Name;
        permission.Description = input.Description;
        permission.Module = input.Module;

        await _context.SaveChangesAsync();
        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var permission = await _context.Permissions
            .Include(p => p.RolePermissions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (permission == null) return NotFound();

        if (permission.RolePermissions.Any())
        {
            return BadRequest("Cannot delete permission assigned to roles");
        }

        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }
}

public class PermissionsTableViewModel
{
    public List<Permission> Permissions { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/Admin/Permissions",
        Handler = "Table",
        HxTarget = "#permissionsTableBody",
        HxInclude = "#searchInput"
    };
}

public class PermissionFormViewModel
{
    public bool IsEdit { get; set; }
    public Permission? Permission { get; set; }
}

public class PermissionFormInput
{
    public Guid? Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
}
