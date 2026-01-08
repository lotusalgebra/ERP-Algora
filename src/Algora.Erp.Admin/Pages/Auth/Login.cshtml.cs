using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Algora.Erp.Admin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Admin.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null, string? message = null)
    {
        ReturnUrl = returnUrl ?? "/";
        SuccessMessage = message;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= "/";

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(Input.Email, Input.Password, ipAddress);

        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage;
            _logger.LogWarning("Failed login attempt for {Email} from {IP}", Input.Email, ipAddress);
            return Page();
        }

        // Create claims for cookie authentication
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.User!.Id.ToString()),
            new(ClaimTypes.Email, result.User.Email),
            new(ClaimTypes.Name, result.User.FullName),
            new(ClaimTypes.Role, result.User.Role?.Name ?? "User")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = Input.RememberMe,
            ExpiresUtc = Input.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(30)
                : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        // Store refresh token in cookie
        Response.Cookies.Append("RefreshToken", result.RefreshToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        _logger.LogInformation("User {Email} logged in successfully", Input.Email);

        return LocalRedirect(returnUrl);
    }
}

public class LoginInput
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
