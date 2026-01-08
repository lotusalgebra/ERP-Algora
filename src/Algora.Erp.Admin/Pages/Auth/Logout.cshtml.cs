using Algora.Erp.Admin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Admin.Pages.Auth;

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
        return await LogoutAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        return await LogoutAsync();
    }

    private async Task<IActionResult> LogoutAsync()
    {
        // Revoke refresh token
        if (Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _authService.RevokeTokenAsync(refreshToken, ipAddress);
        }

        // Clear cookies
        Response.Cookies.Delete("RefreshToken");

        // Sign out
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogInformation("User logged out");

        return RedirectToPage("/Auth/Login", new { message = "You have been logged out successfully" });
    }
}
