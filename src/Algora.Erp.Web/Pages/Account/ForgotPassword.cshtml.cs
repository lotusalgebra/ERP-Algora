using System.ComponentModel.DataAnnotations;
using Algora.Erp.Auth.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(IAuthService authService, ILogger<ForgotPasswordModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [BindProperty]
    public ForgotPasswordInput Input { get; set; } = new();

    public bool EmailSent { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _authService.ForgotPasswordAsync(Input.Email);

        _logger.LogInformation("Password reset requested for {Email}", Input.Email);

        // Always show success to prevent email enumeration
        EmailSent = true;
        return Page();
    }
}

public class ForgotPasswordInput
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}
