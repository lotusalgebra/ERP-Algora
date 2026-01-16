using Algora.Erp.Domain.Entities.Settings;

namespace Algora.Erp.Application.Common.Interfaces;

public interface IIntegrationSettingsService
{
    /// <summary>
    /// Get settings for a specific integration type
    /// </summary>
    Task<T?> GetSettingsAsync<T>(string integrationType, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Get credentials for a specific integration type
    /// </summary>
    Task<T?> GetCredentialsAsync<T>(string integrationType, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Get the raw integration settings entity
    /// </summary>
    Task<IntegrationSettings?> GetIntegrationAsync(string integrationType, CancellationToken ct = default);

    /// <summary>
    /// Save settings and credentials for an integration
    /// </summary>
    Task<IntegrationSettings> SaveSettingsAsync<TSettings, TCredentials>(
        string integrationType,
        TSettings settings,
        TCredentials credentials,
        bool enabled,
        int syncIntervalMinutes = 30,
        CancellationToken ct = default)
        where TSettings : class
        where TCredentials : class;

    /// <summary>
    /// Check if an integration is enabled
    /// </summary>
    Task<bool> IsEnabledAsync(string integrationType, CancellationToken ct = default);

    /// <summary>
    /// Get all enabled integration types
    /// </summary>
    Task<List<string>> GetEnabledIntegrationsAsync(CancellationToken ct = default);

    /// <summary>
    /// Update last test result
    /// </summary>
    Task UpdateTestResultAsync(string integrationType, bool success, string? error = null, CancellationToken ct = default);

    /// <summary>
    /// Update last sync result
    /// </summary>
    Task UpdateSyncResultAsync(string integrationType, bool success, int recordsProcessed, CancellationToken ct = default);

    /// <summary>
    /// Delete an integration configuration
    /// </summary>
    Task DeleteAsync(string integrationType, CancellationToken ct = default);
}
