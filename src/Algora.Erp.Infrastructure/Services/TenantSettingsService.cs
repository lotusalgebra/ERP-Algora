using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Algora.Erp.Infrastructure.Services;

public class TenantSettingsService : ITenantSettingsService
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "TenantSettings";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    // Cached settings for synchronous access
    private TenantSettings? _cachedSettings;

    public TenantSettingsService(IApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<TenantSettings> GetSettingsAsync()
    {
        if (_cache.TryGetValue(CacheKey, out TenantSettings? cachedSettings) && cachedSettings != null)
        {
            _cachedSettings = cachedSettings;
            return cachedSettings;
        }

        var settings = await _context.TenantSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            // Create default settings
            settings = new TenantSettings();
            _context.TenantSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        _cache.Set(CacheKey, settings, CacheDuration);
        _cachedSettings = settings;

        return settings;
    }

    public async Task<TenantSettings> UpdateSettingsAsync(TenantSettings settings)
    {
        var existing = await _context.TenantSettings.FirstOrDefaultAsync();

        if (existing == null)
        {
            _context.TenantSettings.Add(settings);
        }
        else
        {
            // Update all properties
            existing.CompanyName = settings.CompanyName;
            existing.CompanyLogo = settings.CompanyLogo;
            existing.CompanyTagline = settings.CompanyTagline;
            existing.CompanyWebsite = settings.CompanyWebsite;
            existing.CompanyEmail = settings.CompanyEmail;
            existing.CompanyPhone = settings.CompanyPhone;

            existing.AddressLine1 = settings.AddressLine1;
            existing.AddressLine2 = settings.AddressLine2;
            existing.City = settings.City;
            existing.State = settings.State;
            existing.PostalCode = settings.PostalCode;
            existing.Country = settings.Country;
            existing.CountryCode = settings.CountryCode;

            existing.Currency = settings.Currency;
            existing.CurrencySymbol = settings.CurrencySymbol;
            existing.CurrencyName = settings.CurrencyName;
            existing.CurrencyDecimalPlaces = settings.CurrencyDecimalPlaces;
            existing.Language = settings.Language;
            existing.TimeZone = settings.TimeZone;
            existing.DateFormat = settings.DateFormat;
            existing.TimeFormat = settings.TimeFormat;
            existing.DateTimeFormat = settings.DateTimeFormat;

            existing.TaxId = settings.TaxId;
            existing.TaxIdLabel = settings.TaxIdLabel;
            existing.PanNumber = settings.PanNumber;
            existing.IsGstRegistered = settings.IsGstRegistered;

            existing.InvoicePrefix = settings.InvoicePrefix;
            existing.QuotationPrefix = settings.QuotationPrefix;
            existing.SalesOrderPrefix = settings.SalesOrderPrefix;
            existing.PurchaseOrderPrefix = settings.PurchaseOrderPrefix;
            existing.DeliveryChallanPrefix = settings.DeliveryChallanPrefix;
            existing.GoodsReceiptPrefix = settings.GoodsReceiptPrefix;
            existing.DefaultPaymentTermDays = settings.DefaultPaymentTermDays;
            existing.DefaultPaymentTerms = settings.DefaultPaymentTerms;

            existing.InvoiceHeaderText = settings.InvoiceHeaderText;
            existing.InvoiceFooterText = settings.InvoiceFooterText;
            existing.InvoiceTermsText = settings.InvoiceTermsText;
            existing.ShowGstBreakdown = settings.ShowGstBreakdown;
            existing.ShowHsnCode = settings.ShowHsnCode;

            existing.SmtpHost = settings.SmtpHost;
            existing.SmtpPort = settings.SmtpPort;
            existing.SmtpUsername = settings.SmtpUsername;
            if (!string.IsNullOrEmpty(settings.SmtpPassword))
            {
                existing.SmtpPassword = settings.SmtpPassword;
            }
            existing.SmtpEnableSsl = settings.SmtpEnableSsl;
            existing.EmailFromAddress = settings.EmailFromAddress;
            existing.EmailFromName = settings.EmailFromName;

            existing.PasswordMinLength = settings.PasswordMinLength;
            existing.PasswordRequireUppercase = settings.PasswordRequireUppercase;
            existing.PasswordRequireLowercase = settings.PasswordRequireLowercase;
            existing.PasswordRequireDigit = settings.PasswordRequireDigit;
            existing.PasswordRequireSpecialChar = settings.PasswordRequireSpecialChar;
            existing.SessionTimeoutMinutes = settings.SessionTimeoutMinutes;
            existing.MaxLoginAttempts = settings.MaxLoginAttempts;
            existing.LockoutDurationMinutes = settings.LockoutDurationMinutes;
            existing.EnableTwoFactor = settings.EnableTwoFactor;

            existing.AutoBackupEnabled = settings.AutoBackupEnabled;
            existing.AutoBackupTime = settings.AutoBackupTime;
            existing.BackupRetentionDays = settings.BackupRetentionDays;

            existing.EnableEcommerce = settings.EnableEcommerce;
            existing.EnableManufacturing = settings.EnableManufacturing;
            existing.EnableProjects = settings.EnableProjects;
            existing.EnablePayroll = settings.EnablePayroll;

            settings = existing;
        }

        await _context.SaveChangesAsync();

        // Clear cache
        _cache.Remove(CacheKey);
        _cachedSettings = settings;

        return settings;
    }

    public string FormatCurrency(decimal amount)
    {
        var settings = _cachedSettings ?? GetSettingsAsync().GetAwaiter().GetResult();
        var symbol = settings.CurrencySymbol ?? "₹";
        var decimals = settings.CurrencyDecimalPlaces;

        return $"{symbol}{amount.ToString($"N{decimals}")}";
    }

    public async Task<string> GetCurrencySymbolAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.CurrencySymbol ?? "₹";
    }

    public async Task<string> GetCurrencyCodeAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.Currency ?? "INR";
    }

    public async Task<string> GetCompanyNameAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.CompanyName ?? "My Company";
    }

    public async Task<string> GetDateFormatAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.DateFormat ?? "dd/MM/yyyy";
    }

    public string FormatDate(DateTime date)
    {
        var settings = _cachedSettings ?? GetSettingsAsync().GetAwaiter().GetResult();
        return date.ToString(settings.DateFormat ?? "dd/MM/yyyy");
    }

    public string FormatDateTime(DateTime dateTime)
    {
        var settings = _cachedSettings ?? GetSettingsAsync().GetAwaiter().GetResult();
        return dateTime.ToString(settings.DateTimeFormat ?? "dd/MM/yyyy HH:mm");
    }
}
