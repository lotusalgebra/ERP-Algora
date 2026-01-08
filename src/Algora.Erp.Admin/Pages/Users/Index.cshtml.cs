using Algora.Erp.Admin.Data;
using Algora.Erp.Admin.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Algora.Erp.Admin.Pages.Users;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AdminDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(AdminDbContext context, ILogger<IndexModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<AdminUser> Users { get; set; } = new();
    public List<AdminRole> Roles { get; set; } = new();
    public UserStats Stats { get; set; } = new();

    [BindProperty]
    public CreateUserInput CreateInput { get; set; } = new();

    [BindProperty]
    public EditUserInput EditInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        Users = await _context.AdminUsers
            .Include(u => u.Role)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        Roles = await _context.AdminRoles.OrderBy(r => r.Name).ToListAsync();

        Stats = new UserStats
        {
            TotalUsers = Users.Count,
            ActiveUsers = Users.Count(u => u.IsActive),
            LockedUsers = Users.Count(u => u.LockoutEnd.HasValue && u.LockoutEnd > DateTime.UtcNow)
        };
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        // Check if email already exists
        if (await _context.AdminUsers.AnyAsync(u => u.Email.ToLower() == CreateInput.Email.ToLower()))
        {
            TempData["ErrorMessage"] = "A user with this email already exists.";
            return RedirectToPage();
        }

        var user = new AdminUser
        {
            Email = CreateInput.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(CreateInput.Password),
            FirstName = CreateInput.FirstName,
            LastName = CreateInput.LastName,
            Phone = CreateInput.Phone,
            RoleId = CreateInput.RoleId,
            IsActive = true,
            CreatedBy = GetCurrentUserId()
        };

        _context.AdminUsers.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin user {Email} created by {UserId}", user.Email, GetCurrentUserId());
        TempData["SuccessMessage"] = $"User '{user.FullName}' created successfully.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(Guid id)
    {
        var user = await _context.AdminUsers.FindAsync(id);
        if (user == null) return NotFound();

        // Prevent self-deactivation
        if (user.Id == GetCurrentUserId())
        {
            TempData["ErrorMessage"] = "You cannot deactivate your own account.";
            return RedirectToPage();
        }

        user.IsActive = !user.IsActive;
        user.ModifiedAt = DateTime.UtcNow;
        user.ModifiedBy = GetCurrentUserId();

        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin user {Email} {Status} by {UserId}",
            user.Email, user.IsActive ? "activated" : "deactivated", GetCurrentUserId());
        TempData["SuccessMessage"] = $"User '{user.FullName}' {(user.IsActive ? "activated" : "deactivated")}.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnlockAsync(Guid id)
    {
        var user = await _context.AdminUsers.FindAsync(id);
        if (user == null) return NotFound();

        user.LockoutEnd = null;
        user.AccessFailedCount = 0;
        user.ModifiedAt = DateTime.UtcNow;
        user.ModifiedBy = GetCurrentUserId();

        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin user {Email} unlocked by {UserId}", user.Email, GetCurrentUserId());
        TempData["SuccessMessage"] = $"User '{user.FullName}' has been unlocked.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(Guid id, string newPassword)
    {
        var user = await _context.AdminUsers.FindAsync(id);
        if (user == null) return NotFound();

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ModifiedAt = DateTime.UtcNow;
        user.ModifiedBy = GetCurrentUserId();

        await _context.SaveChangesAsync();

        _logger.LogInformation("Password reset for admin user {Email} by {UserId}", user.Email, GetCurrentUserId());
        TempData["SuccessMessage"] = $"Password reset for '{user.FullName}'.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var user = await _context.AdminUsers.FindAsync(id);
        if (user == null) return NotFound();

        // Prevent self-deletion
        if (user.Id == GetCurrentUserId())
        {
            TempData["ErrorMessage"] = "You cannot delete your own account.";
            return RedirectToPage();
        }

        _context.AdminUsers.Remove(user);
        await _context.SaveChangesAsync();

        _logger.LogWarning("Admin user {Email} deleted by {UserId}", user.Email, GetCurrentUserId());
        TempData["SuccessMessage"] = $"User '{user.FullName}' has been deleted.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        var user = await _context.AdminUsers.FindAsync(EditInput.Id);
        if (user == null) return NotFound();

        // Prevent self role change
        if (user.Id == GetCurrentUserId() && user.RoleId != EditInput.RoleId)
        {
            TempData["ErrorMessage"] = "You cannot change your own role.";
            return RedirectToPage();
        }

        // Check if email changed and already exists
        if (user.Email.ToLower() != EditInput.Email.ToLower() &&
            await _context.AdminUsers.AnyAsync(u => u.Email.ToLower() == EditInput.Email.ToLower() && u.Id != EditInput.Id))
        {
            TempData["ErrorMessage"] = "A user with this email already exists.";
            return RedirectToPage();
        }

        user.Email = EditInput.Email;
        user.FirstName = EditInput.FirstName;
        user.LastName = EditInput.LastName;
        user.Phone = EditInput.Phone;
        user.RoleId = EditInput.RoleId;
        user.ModifiedAt = DateTime.UtcNow;
        user.ModifiedBy = GetCurrentUserId();

        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin user {Email} updated by {UserId}", user.Email, GetCurrentUserId());
        TempData["SuccessMessage"] = $"User '{user.FullName}' updated successfully.";

        return RedirectToPage();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

public class CreateUserInput
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Role is required")]
    public Guid RoleId { get; set; }
}

public class EditUserInput
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Role is required")]
    public Guid RoleId { get; set; }
}

public class UserStats
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int LockedUsers { get; set; }
}
