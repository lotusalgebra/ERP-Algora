using System.Text.Json;
using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Algora.Erp.Infrastructure.Services;

public class IntegrationSettingsService : IIntegrationSettingsService
{
    private readonly IApplicationDbContext _context;
    private readonly IDataProtector _protector;
    private readonly IMemoryCache _cache;
    private readonly ILogger<IntegrationSettingsService> _logger;
    private const string CacheKeyPrefix = "IntegrationSettings_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public IntegrationSettingsService(
        IApplicationDbContext context,
        IDataProtectionProvider dataProtectionProvider,
        IMemoryCache cache,
        ILogger<IntegrationSettingsService> logger)
    {
        _context = context;
        _protector = dataProtectionProvider.CreateProtector("Algora.Erp.IntegrationSettings");
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetSettingsAsync<T>(string integrationType, CancellationToken ct = default) where T : class
    {
        var integration = await GetIntegrationAsync(integrationType, ct);
        if (integration == null || string.IsNullOrEmpty(integration.SettingsJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(integration.SettingsJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize settings for {IntegrationType}", integrationType);
            return null;
        }
    }

    public async Task<T?> GetCredentialsAsync<T>(string integrationType, CancellationToken ct = default) where T : class
    {
        var integration = await GetIntegrationAsync(integrationType, ct);
        if (integration == null || string.IsNullOrEmpty(integration.EncryptedCredentials))
            return null;

        try
        {
            var decrypted = _protector.Unprotect(integration.EncryptedCredentials);
            return JsonSerializer.Deserialize<T>(decrypted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt credentials for {IntegrationType}", integrationType);
            return null;
        }
    }

    public async Task<IntegrationSettings?> GetIntegrationAsync(string integrationType, CancellationToken ct = default)
    {
        var cacheKey = CacheKeyPrefix + integrationType;

        if (_cache.TryGetValue(cacheKey, out IntegrationSettings? cached) && cached != null)
            return cached;

        try
        {
            var integration = await _context.IntegrationSettings
                .FirstOrDefaultAsync(x => x.IntegrationType == integrationType && !x.IsDeleted, ct);

            if (integration != null)
            {
                _cache.Set(cacheKey, integration, CacheDuration);
            }

            return integration;
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 208)
        {
            // Table doesn't exist yet - return null gracefully
            _logger.LogDebug("IntegrationSettings table not found, returning null");
            return null;
        }
    }

    public async Task<IntegrationSettings> SaveSettingsAsync<TSettings, TCredentials>(
        string integrationType,
        TSettings settings,
        TCredentials credentials,
        bool enabled,
        int syncIntervalMinutes = 30,
        CancellationToken ct = default)
        where TSettings : class
        where TCredentials : class
    {
        var integration = await _context.IntegrationSettings
            .FirstOrDefaultAsync(x => x.IntegrationType == integrationType && !x.IsDeleted, ct);

        var settingsJson = JsonSerializer.Serialize(settings);
        var credentialsJson = JsonSerializer.Serialize(credentials);
        var encryptedCredentials = _protector.Protect(credentialsJson);

        if (integration == null)
        {
            integration = new IntegrationSettings
            {
                IntegrationType = integrationType,
                IsEnabled = enabled,
                SettingsJson = settingsJson,
                EncryptedCredentials = encryptedCredentials,
                SyncIntervalMinutes = syncIntervalMinutes
            };
            _context.IntegrationSettings.Add(integration);
        }
        else
        {
            integration.IsEnabled = enabled;
            integration.SettingsJson = settingsJson;
            integration.EncryptedCredentials = encryptedCredentials;
            integration.SyncIntervalMinutes = syncIntervalMinutes;
        }

        await _context.SaveChangesAsync(ct);

        // Clear cache
        _cache.Remove(CacheKeyPrefix + integrationType);

        _logger.LogInformation("Saved {IntegrationType} integration settings, Enabled: {Enabled}",
            integrationType, enabled);

        return integration;
    }

    public async Task<bool> IsEnabledAsync(string integrationType, CancellationToken ct = default)
    {
        var integration = await GetIntegrationAsync(integrationType, ct);
        return integration?.IsEnabled ?? false;
    }

    public async Task<List<string>> GetEnabledIntegrationsAsync(CancellationToken ct = default)
    {
        try
        {
            return await _context.IntegrationSettings
                .Where(x => x.IsEnabled && !x.IsDeleted)
                .Select(x => x.IntegrationType)
                .ToListAsync(ct);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 208)
        {
            // Table doesn't exist yet - return empty list gracefully
            _logger.LogDebug("IntegrationSettings table not found, returning empty list");
            return new List<string>();
        }
    }

    public async Task UpdateTestResultAsync(string integrationType, bool success, string? error = null, CancellationToken ct = default)
    {
        var integration = await _context.IntegrationSettings
            .FirstOrDefaultAsync(x => x.IntegrationType == integrationType && !x.IsDeleted, ct);

        if (integration == null)
            return;

        integration.LastTestedAt = DateTime.UtcNow;
        integration.LastTestSuccess = success;
        integration.LastTestError = error;

        await _context.SaveChangesAsync(ct);

        // Clear cache
        _cache.Remove(CacheKeyPrefix + integrationType);
    }

    public async Task UpdateSyncResultAsync(string integrationType, bool success, int recordsProcessed, CancellationToken ct = default)
    {
        var integration = await _context.IntegrationSettings
            .FirstOrDefaultAsync(x => x.IntegrationType == integrationType && !x.IsDeleted, ct);

        if (integration == null)
            return;

        integration.LastSyncAt = DateTime.UtcNow;
        integration.LastSyncSuccess = success;
        integration.LastSyncRecordsProcessed = recordsProcessed;

        await _context.SaveChangesAsync(ct);

        // Clear cache
        _cache.Remove(CacheKeyPrefix + integrationType);
    }

    public async Task DeleteAsync(string integrationType, CancellationToken ct = default)
    {
        var integration = await _context.IntegrationSettings
            .FirstOrDefaultAsync(x => x.IntegrationType == integrationType && !x.IsDeleted, ct);

        if (integration == null)
            return;

        integration.IsDeleted = true;
        integration.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        // Clear cache
        _cache.Remove(CacheKeyPrefix + integrationType);

        _logger.LogInformation("Deleted {IntegrationType} integration settings", integrationType);
    }
}
