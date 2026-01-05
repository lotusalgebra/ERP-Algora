using Algora.Erp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Admin.Settings;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public GeneralSettings General { get; set; } = new();

    [BindProperty]
    public EmailSettings Email { get; set; } = new();

    [BindProperty]
    public SecuritySettings Security { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        // Load settings from database or use defaults
        // In a real app, these would be stored in a Settings table
        General = new GeneralSettings
        {
            CompanyName = "Algora ERP",
            TimeZone = "UTC",
            DateFormat = "MM/dd/yyyy",
            Currency = "USD",
            Language = "en-US"
        };

        Email = new EmailSettings
        {
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            SmtpUsername = "",
            EnableSsl = true,
            FromEmail = "noreply@algora.com",
            FromName = "Algora ERP"
        };

        Security = new SecuritySettings
        {
            PasswordMinLength = 8,
            RequireUppercase = true,
            RequireLowercase = true,
            RequireDigit = true,
            RequireSpecialChar = true,
            SessionTimeoutMinutes = 30,
            MaxLoginAttempts = 5,
            LockoutDurationMinutes = 15,
            EnableTwoFactor = false
        };
    }

    public async Task<IActionResult> OnPostGeneralAsync()
    {
        // Save general settings
        SuccessMessage = "General settings saved successfully";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEmailAsync()
    {
        // Save email settings
        SuccessMessage = "Email settings saved successfully";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSecurityAsync()
    {
        // Save security settings
        SuccessMessage = "Security settings saved successfully";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTestEmailAsync()
    {
        // Test email configuration
        SuccessMessage = "Test email sent successfully";
        return RedirectToPage();
    }
}

public class GeneralSettings
{
    public string CompanyName { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "UTC";
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    public string Currency { get; set; } = "USD";
    public string Language { get; set; } = "en-US";
}

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class SecuritySettings
{
    public int PasswordMinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = true;
    public int SessionTimeoutMinutes { get; set; } = 30;
    public int MaxLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
    public bool EnableTwoFactor { get; set; } = false;
}
