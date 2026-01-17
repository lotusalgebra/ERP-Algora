using System.ComponentModel.DataAnnotations;
using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Account;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProfileModel> _logger;

    public ProfileModel(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<ProfileModel> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public User? CurrentUser { get; set; }
    public List<string> UserRoles { get; set; } = new();

    [BindProperty]
    public ProfileInput Input { get; set; } = new();

    [BindProperty]
    public PasswordInput? PasswordChange { get; set; }

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }

        CurrentUser = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        UserRoles = CurrentUser.UserRoles.Select(ur => ur.Role.Name).ToList();

        Input = new ProfileInput
        {
            FirstName = CurrentUser.FirstName,
            LastName = CurrentUser.LastName,
            Email = CurrentUser.Email,
            PhoneNumber = CurrentUser.PhoneNumber
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }

        CurrentUser = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        UserRoles = CurrentUser.UserRoles.Select(ur => ur.Role.Name).ToList();

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please correct the errors below.";
            return Page();
        }

        CurrentUser.FirstName = Input.FirstName;
        CurrentUser.LastName = Input.LastName;
        CurrentUser.PhoneNumber = Input.PhoneNumber;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {Email} updated their profile", CurrentUser.Email);
        SuccessMessage = "Profile updated successfully.";
        return Page();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }

        CurrentUser = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        UserRoles = CurrentUser.UserRoles.Select(ur => ur.Role.Name).ToList();

        Input = new ProfileInput
        {
            FirstName = CurrentUser.FirstName,
            LastName = CurrentUser.LastName,
            Email = CurrentUser.Email,
            PhoneNumber = CurrentUser.PhoneNumber
        };

        if (PasswordChange == null)
        {
            ErrorMessage = "Please fill in the password fields.";
            return Page();
        }

        if (string.IsNullOrEmpty(PasswordChange.CurrentPassword) ||
            string.IsNullOrEmpty(PasswordChange.NewPassword) ||
            string.IsNullOrEmpty(PasswordChange.ConfirmPassword))
        {
            ErrorMessage = "All password fields are required.";
            return Page();
        }

        if (PasswordChange.NewPassword != PasswordChange.ConfirmPassword)
        {
            ErrorMessage = "New password and confirmation do not match.";
            return Page();
        }

        if (PasswordChange.NewPassword.Length < 8)
        {
            ErrorMessage = "New password must be at least 8 characters long.";
            return Page();
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(PasswordChange.CurrentPassword, CurrentUser.PasswordHash))
        {
            ErrorMessage = "Current password is incorrect.";
            return Page();
        }

        // Update password
        CurrentUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordChange.NewPassword);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {Email} changed their password", CurrentUser.Email);
        SuccessMessage = "Password changed successfully.";
        return Page();
    }
}

public class ProfileInput
{
    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
}

public class PasswordInput
{
    [Display(Name = "Current Password")]
    public string? CurrentPassword { get; set; }

    [Display(Name = "New Password")]
    public string? NewPassword { get; set; }

    [Display(Name = "Confirm Password")]
    public string? ConfirmPassword { get; set; }
}
