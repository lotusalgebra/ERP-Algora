using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Administration;
using Algora.Erp.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Algora.Erp.Web.Pages.Admin.Users;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }

    public List<Role> Roles { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalUsers = await _context.Users.CountAsync();
        ActiveUsers = await _context.Users.CountAsync(u => u.Status == UserStatus.Active);
        InactiveUsers = await _context.Users.CountAsync(u => u.Status != UserStatus.Active);

        Roles = await _context.Roles.ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, Guid? roleFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<UserStatus>(statusFilter, out var status))
        {
            query = query.Where(u => u.Status == status);
        }

        if (roleFilter.HasValue)
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == roleFilter.Value));
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_UsersTableRows", new UsersTableViewModel
        {
            Users = users,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var roles = await _context.Roles.ToListAsync();
        return Partial("_UserForm", new UserFormViewModel { IsEdit = false, Roles = roles });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();

        var roles = await _context.Roles.ToListAsync();
        return Partial("_UserForm", new UserFormViewModel
        {
            IsEdit = true,
            User = user,
            Roles = roles,
            SelectedRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList()
        });
    }

    public async Task<IActionResult> OnPostAsync(UserFormInput input)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        User? user;

        if (input.Id.HasValue)
        {
            user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == input.Id.Value);

            if (user == null) return NotFound();

            // Remove old roles
            foreach (var ur in user.UserRoles.ToList())
            {
                _context.UserRoles.Remove(ur);
            }
        }
        else
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == input.Email))
            {
                return BadRequest("Email already exists");
            }

            user = new User { Id = Guid.NewGuid() };
            // Set password hash (simple hash for demo - use proper hashing in production)
            var password = input.Password ?? "Password123!";
            using var sha256 = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            user.PasswordHash = Convert.ToBase64String(hash);
            _context.Users.Add(user);
        }

        user.Email = input.Email;
        user.FirstName = input.FirstName;
        user.LastName = input.LastName;
        user.PhoneNumber = input.PhoneNumber;
        user.Status = input.Status;

        // Add new roles
        if (input.RoleIds != null)
        {
            foreach (var roleId in input.RoleIds)
            {
                _context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = roleId
                });
            }
        }

        await _context.SaveChangesAsync();
        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.Status = user.Status == UserStatus.Active ? UserStatus.Inactive : UserStatus.Active;
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }
}

public class UsersTableViewModel
{
    public List<User> Users { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/Admin/Users",
        Handler = "Table",
        HxTarget = "#usersTableBody",
        HxInclude = "#searchInput,#roleFilter,#statusFilter,#pageSizeSelect"
    };
}

public class UserFormViewModel
{
    public bool IsEdit { get; set; }
    public User? User { get; set; }
    public List<Role> Roles { get; set; } = new();
    public List<Guid> SelectedRoleIds { get; set; } = new();
}

public class UserFormInput
{
    public Guid? Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public List<Guid>? RoleIds { get; set; }
}
