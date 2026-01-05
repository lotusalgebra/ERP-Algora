using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Administration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Admin.Roles;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalRoles { get; set; }
    public int SystemRoles { get; set; }
    public int CustomRoles { get; set; }
    public List<Permission> Permissions { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalRoles = await _context.Roles.CountAsync();
        SystemRoles = await _context.Roles.CountAsync(r => r.IsSystemRole);
        CustomRoles = await _context.Roles.CountAsync(r => !r.IsSystemRole);
        Permissions = await _context.Permissions.OrderBy(p => p.Module).ThenBy(p => p.Name).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, int page = 1, int pageSize = 10)
    {
        var query = _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(r =>
                r.Name.ToLower().Contains(search) ||
                (r.Description != null && r.Description.ToLower().Contains(search)));
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var roles = await query
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_RolesTableRows", new RolesTableViewModel
        {
            Roles = roles,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var permissions = await _context.Permissions.OrderBy(p => p.Module).ThenBy(p => p.Name).ToListAsync();
        return Partial("_RoleForm", new RoleFormViewModel { IsEdit = false, Permissions = permissions });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null) return NotFound();

        var permissions = await _context.Permissions.OrderBy(p => p.Module).ThenBy(p => p.Name).ToListAsync();
        return Partial("_RoleForm", new RoleFormViewModel
        {
            IsEdit = true,
            Role = role,
            Permissions = permissions,
            SelectedPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList()
        });
    }

    public async Task<IActionResult> OnPostAsync(RoleFormInput input)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        Role? role;

        if (input.Id.HasValue)
        {
            role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == input.Id.Value);

            if (role == null) return NotFound();

            // Don't allow editing system role name
            if (!role.IsSystemRole)
            {
                role.Name = input.Name;
            }
            role.Description = input.Description;

            // Remove old permissions
            foreach (var rp in role.RolePermissions.ToList())
            {
                _context.RolePermissions.Remove(rp);
            }
        }
        else
        {
            // Check if name already exists
            if (await _context.Roles.AnyAsync(r => r.Name == input.Name))
            {
                return BadRequest("Role name already exists");
            }

            role = new Role { Id = Guid.NewGuid() };
            role.Name = input.Name;
            role.Description = input.Description;
            role.IsSystemRole = false;
            _context.Roles.Add(role);
        }

        // Add new permissions
        if (input.PermissionIds != null)
        {
            foreach (var permId in input.PermissionIds)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = role.Id,
                    PermissionId = permId
                });
            }
        }

        await _context.SaveChangesAsync();
        return await OnGetTableAsync(null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null) return NotFound();

        if (role.IsSystemRole)
        {
            return BadRequest("Cannot delete system roles");
        }

        if (role.UserRoles.Any())
        {
            return BadRequest("Cannot delete role with assigned users");
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null);
    }
}

public class RolesTableViewModel
{
    public List<Role> Roles { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}

public class RoleFormViewModel
{
    public bool IsEdit { get; set; }
    public Role? Role { get; set; }
    public List<Permission> Permissions { get; set; } = new();
    public List<Guid> SelectedPermissionIds { get; set; } = new();
}

public class RoleFormInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<Guid>? PermissionIds { get; set; }
}
