using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Admin.Settings;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly ITenantSettingsService _settingsService;

    public IndexModel(ITenantSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [BindProperty]
    public TenantSettings Settings { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    // Currency options for dropdown
    public List<CurrencyOption> CurrencyOptions { get; } = new()
    {
        new("INR", "₹", "Indian Rupee"),
        new("USD", "$", "US Dollar"),
        new("EUR", "€", "Euro"),
        new("GBP", "£", "British Pound"),
        new("JPY", "¥", "Japanese Yen"),
        new("CAD", "C$", "Canadian Dollar"),
        new("AUD", "A$", "Australian Dollar"),
        new("CHF", "CHF", "Swiss Franc"),
        new("CNY", "¥", "Chinese Yuan"),
        new("SGD", "S$", "Singapore Dollar"),
        new("AED", "د.إ", "UAE Dirham"),
        new("SAR", "﷼", "Saudi Riyal")
    };

    // Country options
    public List<CountryOption> CountryOptions { get; } = new()
    {
        new("India", "IN"),
        new("United States", "US"),
        new("United Kingdom", "GB"),
        new("Canada", "CA"),
        new("Australia", "AU"),
        new("Germany", "DE"),
        new("France", "FR"),
        new("Japan", "JP"),
        new("Singapore", "SG"),
        new("United Arab Emirates", "AE"),
        new("Saudi Arabia", "SA"),
        new("Netherlands", "NL"),
        new("Switzerland", "CH")
    };

    // Timezone options
    public List<TimezoneOption> TimezoneOptions { get; } = new()
    {
        new("Asia/Kolkata", "India Standard Time (IST)"),
        new("UTC", "Coordinated Universal Time (UTC)"),
        new("America/New_York", "Eastern Time (US)"),
        new("America/Chicago", "Central Time (US)"),
        new("America/Denver", "Mountain Time (US)"),
        new("America/Los_Angeles", "Pacific Time (US)"),
        new("Europe/London", "Greenwich Mean Time (UK)"),
        new("Europe/Paris", "Central European Time"),
        new("Europe/Berlin", "Central European Time (Germany)"),
        new("Asia/Tokyo", "Japan Standard Time"),
        new("Asia/Singapore", "Singapore Time"),
        new("Asia/Dubai", "Gulf Standard Time"),
        new("Australia/Sydney", "Australian Eastern Time")
    };

    // Language options
    public List<LanguageOption> LanguageOptions { get; } = new()
    {
        new("en-IN", "English (India)"),
        new("en-US", "English (US)"),
        new("en-GB", "English (UK)"),
        new("hi-IN", "Hindi"),
        new("es-ES", "Spanish"),
        new("fr-FR", "French"),
        new("de-DE", "German"),
        new("ja-JP", "Japanese"),
        new("zh-CN", "Chinese (Simplified)")
    };

    // Date format options
    public List<DateFormatOption> DateFormatOptions { get; } = new()
    {
        new("dd/MM/yyyy", "31/12/2024 (India/UK)"),
        new("MM/dd/yyyy", "12/31/2024 (US)"),
        new("yyyy-MM-dd", "2024-12-31 (ISO)"),
        new("dd-MM-yyyy", "31-12-2024"),
        new("dd.MM.yyyy", "31.12.2024 (Europe)")
    };

    public async Task OnGetAsync()
    {
        Settings = await _settingsService.GetSettingsAsync();
    }

    public async Task<IActionResult> OnPostGeneralAsync()
    {
        try
        {
            var existing = await _settingsService.GetSettingsAsync();

            // Update only general settings
            existing.CompanyName = Settings.CompanyName;
            existing.CompanyTagline = Settings.CompanyTagline;
            existing.CompanyWebsite = Settings.CompanyWebsite;
            existing.CompanyEmail = Settings.CompanyEmail;
            existing.CompanyPhone = Settings.CompanyPhone;
            existing.AddressLine1 = Settings.AddressLine1;
            existing.AddressLine2 = Settings.AddressLine2;
            existing.City = Settings.City;
            existing.State = Settings.State;
            existing.PostalCode = Settings.PostalCode;
            existing.Country = Settings.Country;
            existing.CountryCode = Settings.CountryCode;
            existing.TimeZone = Settings.TimeZone;
            existing.DateFormat = Settings.DateFormat;
            existing.Language = Settings.Language;

            await _settingsService.UpdateSettingsAsync(existing);
            TempData["SuccessMessage"] = "General settings saved successfully";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to save settings: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCurrencyAsync()
    {
        try
        {
            var existing = await _settingsService.GetSettingsAsync();

            // Update currency settings
            existing.Currency = Settings.Currency;

            // Auto-set currency symbol and name based on currency code
            var selectedCurrency = CurrencyOptions.FirstOrDefault(c => c.Code == Settings.Currency);
            if (selectedCurrency != null)
            {
                existing.CurrencySymbol = selectedCurrency.Symbol;
                existing.CurrencyName = selectedCurrency.Name;
            }
            existing.CurrencyDecimalPlaces = Settings.CurrencyDecimalPlaces;

            await _settingsService.UpdateSettingsAsync(existing);
            TempData["SuccessMessage"] = "Currency settings saved successfully";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to save settings: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTaxAsync()
    {
        try
        {
            var existing = await _settingsService.GetSettingsAsync();

            // Update tax settings
            existing.TaxId = Settings.TaxId;
            existing.TaxIdLabel = Settings.TaxIdLabel;
            existing.PanNumber = Settings.PanNumber;
            existing.IsGstRegistered = Settings.IsGstRegistered;

            await _settingsService.UpdateSettingsAsync(existing);
            TempData["SuccessMessage"] = "Tax settings saved successfully";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to save settings: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostInvoiceAsync()
    {
        try
        {
            var existing = await _settingsService.GetSettingsAsync();

            // Update invoice settings
            existing.InvoicePrefix = Settings.InvoicePrefix;
            existing.QuotationPrefix = Settings.QuotationPrefix;
            existing.SalesOrderPrefix = Settings.SalesOrderPrefix;
            existing.PurchaseOrderPrefix = Settings.PurchaseOrderPrefix;
            existing.DeliveryChallanPrefix = Settings.DeliveryChallanPrefix;
            existing.GoodsReceiptPrefix = Settings.GoodsReceiptPrefix;
            existing.DefaultPaymentTermDays = Settings.DefaultPaymentTermDays;
            existing.DefaultPaymentTerms = Settings.DefaultPaymentTerms;
            existing.InvoiceHeaderText = Settings.InvoiceHeaderText;
            existing.InvoiceFooterText = Settings.InvoiceFooterText;
            existing.InvoiceTermsText = Settings.InvoiceTermsText;
            existing.ShowGstBreakdown = Settings.ShowGstBreakdown;
            existing.ShowHsnCode = Settings.ShowHsnCode;

            await _settingsService.UpdateSettingsAsync(existing);
            TempData["SuccessMessage"] = "Invoice settings saved successfully";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to save settings: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEmailAsync()
    {
        try
        {
            var existing = await _settingsService.GetSettingsAsync();

            // Update email settings
            existing.SmtpHost = Settings.SmtpHost;
            existing.SmtpPort = Settings.SmtpPort;
            existing.SmtpUsername = Settings.SmtpUsername;
            if (!string.IsNullOrEmpty(Settings.SmtpPassword))
            {
                existing.SmtpPassword = Settings.SmtpPassword;
            }
            existing.SmtpEnableSsl = Settings.SmtpEnableSsl;
            existing.EmailFromAddress = Settings.EmailFromAddress;
            existing.EmailFromName = Settings.EmailFromName;

            await _settingsService.UpdateSettingsAsync(existing);
            TempData["SuccessMessage"] = "Email settings saved successfully";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to save settings: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSecurityAsync()
    {
        try
        {
            var existing = await _settingsService.GetSettingsAsync();

            // Update security settings
            existing.PasswordMinLength = Settings.PasswordMinLength;
            existing.PasswordRequireUppercase = Settings.PasswordRequireUppercase;
            existing.PasswordRequireLowercase = Settings.PasswordRequireLowercase;
            existing.PasswordRequireDigit = Settings.PasswordRequireDigit;
            existing.PasswordRequireSpecialChar = Settings.PasswordRequireSpecialChar;
            existing.SessionTimeoutMinutes = Settings.SessionTimeoutMinutes;
            existing.MaxLoginAttempts = Settings.MaxLoginAttempts;
            existing.LockoutDurationMinutes = Settings.LockoutDurationMinutes;
            existing.EnableTwoFactor = Settings.EnableTwoFactor;

            await _settingsService.UpdateSettingsAsync(existing);
            TempData["SuccessMessage"] = "Security settings saved successfully";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to save settings: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTestEmailAsync()
    {
        // TODO: Implement email test
        TempData["SuccessMessage"] = "Test email functionality not yet implemented";
        return RedirectToPage();
    }
}

public record CurrencyOption(string Code, string Symbol, string Name);
public record CountryOption(string Name, string Code);
public record TimezoneOption(string Id, string DisplayName);
public record LanguageOption(string Code, string Name);
public record DateFormatOption(string Format, string Example);
