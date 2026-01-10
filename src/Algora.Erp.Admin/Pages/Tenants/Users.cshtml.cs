using Algora.Erp.Admin.Data;
using Algora.Erp.Admin.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Algora.Erp.Admin.Pages.Tenants;

[Authorize]
public class UsersModel : PageModel
{
    private readonly AdminDbContext _context;
    private readonly ILogger<UsersModel> _logger;

    public UsersModel(AdminDbContext context, ILogger<UsersModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Tenant Tenant { get; set; } = null!;
    public List<TenantUser> Users { get; set; } = new();
    public TenantUserStats Stats { get; set; } = new();

    [BindProperty]
    public CreateTenantUserInput CreateInput { get; set; } = new();

    [BindProperty]
    public EditTenantUserInput EditInput { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid tenantId)
    {
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
            return NotFound();

        Tenant = tenant;

        Users = await _context.TenantUsers
            .Where(u => u.TenantId == tenantId)
            .OrderByDescending(u => u.IsOwner)
            .ThenByDescending(u => u.CreatedAt)
            .ToListAsync();

        Stats = new TenantUserStats
        {
            TotalUsers = Users.Count,
            ActiveUsers = Users.Count(u => u.IsActive),
            InactiveUsers = Users.Count(u => !u.IsActive),
            OwnerCount = Users.Count(u => u.IsOwner)
        };

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(Guid tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
            return NotFound();

        // Check user limit
        if (tenant.MaxUsers > 0)
        {
            var currentCount = await _context.TenantUsers.CountAsync(u => u.TenantId == tenantId);
            if (currentCount >= tenant.MaxUsers)
            {
                TempData["ErrorMessage"] = $"User limit reached. This tenant can have a maximum of {tenant.MaxUsers} users.";
                return RedirectToPage(new { tenantId });
            }
        }

        // Check if email already exists for this tenant
        if (await _context.TenantUsers.AnyAsync(u => u.TenantId == tenantId && u.Email.ToLower() == CreateInput.Email.ToLower()))
        {
            TempData["ErrorMessage"] = "A user with this email already exists for this tenant.";
            return RedirectToPage(new { tenantId });
        }

        var user = new TenantUser
        {
            TenantId = tenantId,
            Email = CreateInput.Email,
            FullName = CreateInput.FullName,
            Role = CreateInput.Role,
            IsOwner = CreateInput.IsOwner,
            IsActive = true
        };

        _context.TenantUsers.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tenant user {Email} created for tenant {TenantId} by admin {UserId}",
            user.Email, tenantId, GetCurrentUserId());
        TempData["SuccessMessage"] = $"User '{user.FullName}' created successfully.";

        return RedirectToPage(new { tenantId });
    }

    public async Task<IActionResult> OnPostEditAsync(Guid tenantId)
    {
        var user = await _context.TenantUsers
            .FirstOrDefaultAsync(u => u.Id == EditInput.Id && u.TenantId == tenantId);

        if (user == null)
            return NotFound();

        // Check if email changed and already exists
        if (user.Email.ToLower() != EditInput.Email.ToLower() &&
            await _context.TenantUsers.AnyAsync(u => u.TenantId == tenantId && u.Email.ToLower() == EditInput.Email.ToLower() && u.Id != EditInput.Id))
        {
            TempData["ErrorMessage"] = "A user with this email already exists for this tenant.";
            return RedirectToPage(new { tenantId });
        }

        user.Email = EditInput.Email;
        user.FullName = EditInput.FullName;
        user.Role = EditInput.Role;
        user.IsOwner = EditInput.IsOwner;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Tenant user {Email} updated for tenant {TenantId} by admin {UserId}",
            user.Email, tenantId, GetCurrentUserId());
        TempData["SuccessMessage"] = $"User '{user.FullName}' updated successfully.";

        return RedirectToPage(new { tenantId });
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(Guid tenantId, Guid userId)
    {
        var user = await _context.TenantUsers
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            return NotFound();

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tenant user {Email} {Status} for tenant {TenantId} by admin {UserId}",
            user.Email, user.IsActive ? "activated" : "deactivated", tenantId, GetCurrentUserId());
        TempData["SuccessMessage"] = $"User '{user.FullName}' {(user.IsActive ? "activated" : "deactivated")}.";

        return RedirectToPage(new { tenantId });
    }

    public async Task<IActionResult> OnPostSetOwnerAsync(Guid tenantId, Guid userId)
    {
        var user = await _context.TenantUsers
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            return NotFound();

        user.IsOwner = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tenant user {Email} set as owner for tenant {TenantId} by admin {UserId}",
            user.Email, tenantId, GetCurrentUserId());
        TempData["SuccessMessage"] = $"User '{user.FullName}' has been set as account owner.";

        return RedirectToPage(new { tenantId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid tenantId, Guid userId)
    {
        var user = await _context.TenantUsers
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            return NotFound();

        // Prevent deleting account owner
        if (user.IsOwner)
        {
            TempData["ErrorMessage"] = "Cannot delete the account owner. Please transfer ownership first.";
            return RedirectToPage(new { tenantId });
        }

        _context.TenantUsers.Remove(user);
        await _context.SaveChangesAsync();

        _logger.LogWarning("Tenant user {Email} deleted from tenant {TenantId} by admin {UserId}",
            user.Email, tenantId, GetCurrentUserId());
        TempData["SuccessMessage"] = $"User '{user.FullName}' has been deleted.";

        return RedirectToPage(new { tenantId });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

public class CreateTenantUserInput
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Role { get; set; }

    public bool IsOwner { get; set; }
}

public class EditTenantUserInput
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Role { get; set; }

    public bool IsOwner { get; set; }
}

public class TenantUserStats
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int OwnerCount { get; set; }
}
