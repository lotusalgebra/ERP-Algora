using Algora.Erp.Domain.Entities.Settings;

namespace Algora.Erp.Application.Common.Interfaces;

public interface ITenantSettingsService
{
    /// <summary>
    /// Get current tenant settings. Creates default if not exists.
    /// </summary>
    Task<TenantSettings> GetSettingsAsync();

    /// <summary>
    /// Update tenant settings
    /// </summary>
    Task<TenantSettings> UpdateSettingsAsync(TenantSettings settings);

    /// <summary>
    /// Get formatted currency string
    /// </summary>
    string FormatCurrency(decimal amount);

    /// <summary>
    /// Get currency symbol
    /// </summary>
    Task<string> GetCurrencySymbolAsync();

    /// <summary>
    /// Get currency code (INR, USD, etc.)
    /// </summary>
    Task<string> GetCurrencyCodeAsync();

    /// <summary>
    /// Get company name
    /// </summary>
    Task<string> GetCompanyNameAsync();

    /// <summary>
    /// Get date format
    /// </summary>
    Task<string> GetDateFormatAsync();

    /// <summary>
    /// Format date according to tenant settings
    /// </summary>
    string FormatDate(DateTime date);

    /// <summary>
    /// Format date/time according to tenant settings
    /// </summary>
    string FormatDateTime(DateTime dateTime);
}
