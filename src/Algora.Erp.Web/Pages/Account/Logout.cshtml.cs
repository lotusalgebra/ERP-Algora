using System.Security.Claims;
using Algora.Erp.Auth.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(IAuthService authService, ILogger<LogoutModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        return await PerformLogout();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        return await PerformLogout();
    }

    private async Task<IActionResult> PerformLogout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Revoke refresh token
        if (Guid.TryParse(userId, out var userGuid))
        {
            await _authService.RevokeTokenAsync(userGuid);
        }

        // Sign out from cookie authentication
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Clear token cookies
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        _logger.LogInformation("User {UserId} logged out", userId);

        return RedirectToPage("/Account/Login", new { message = "You have been logged out successfully." });
    }
}
