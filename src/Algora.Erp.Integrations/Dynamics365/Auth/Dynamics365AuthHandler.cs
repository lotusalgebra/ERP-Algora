using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Algora.Erp.Integrations.Common.Exceptions;
using Microsoft.Identity.Client;

namespace Algora.Erp.Integrations.Dynamics365.Auth;

public interface IDynamics365AuthHandler
{
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
    Task<string> GetInstanceUrlAsync(CancellationToken ct = default);
    Task<string> GetApiVersionAsync(CancellationToken ct = default);
    void InvalidateToken();
}

public class Dynamics365AuthHandler : IDynamics365AuthHandler
{
    private readonly IIntegrationSettingsService _settingsService;
    private IConfidentialClientApplication? _app;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    // Cached settings
    private Dynamics365SettingsData? _cachedSettings;
    private Dynamics365Credentials? _cachedCredentials;
    private DateTime _settingsCacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan SettingsCacheDuration = TimeSpan.FromMinutes(5);

    public const string IntegrationType = "Dynamics365";

    public Dynamics365AuthHandler(IIntegrationSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    private async Task EnsureSettingsCacheAsync(CancellationToken ct)
    {
        if (_cachedSettings != null && _cachedCredentials != null && DateTime.UtcNow < _settingsCacheExpiry)
            return;

        _cachedSettings = await _settingsService.GetSettingsAsync<Dynamics365SettingsData>(IntegrationType, ct);
        _cachedCredentials = await _settingsService.GetCredentialsAsync<Dynamics365Credentials>(IntegrationType, ct);
        _settingsCacheExpiry = DateTime.UtcNow.Add(SettingsCacheDuration);

        // Rebuild the MSAL app when settings change
        _app = null;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
        {
            return _cachedToken;
        }

        await EnsureSettingsCacheAsync(ct);

        if (_cachedSettings == null || _cachedCredentials == null)
        {
            throw new CrmAuthenticationException(IntegrationType, "Dynamics 365 integration is not configured");
        }

        _app ??= ConfidentialClientApplicationBuilder
            .Create(_cachedCredentials.ClientId)
            .WithClientSecret(_cachedCredentials.ClientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{_cachedSettings.TenantId}"))
            .Build();

        var scope = $"{_cachedSettings.InstanceUrl}/.default";

        try
        {
            var result = await _app.AcquireTokenForClient(new[] { scope })
                .ExecuteAsync(ct);

            _cachedToken = result.AccessToken;
            _tokenExpiry = result.ExpiresOn.UtcDateTime;

            await _settingsService.UpdateTestResultAsync(IntegrationType, true, null, ct);
            return _cachedToken;
        }
        catch (MsalServiceException ex)
        {
            await _settingsService.UpdateTestResultAsync(IntegrationType, false, ex.Message, ct);
            throw new CrmAuthenticationException(IntegrationType, $"Authentication failed: {ex.Message}", ex);
        }
        catch (MsalClientException ex)
        {
            await _settingsService.UpdateTestResultAsync(IntegrationType, false, ex.Message, ct);
            throw new CrmAuthenticationException(IntegrationType, $"Client authentication error: {ex.Message}", ex);
        }
    }

    public async Task<string> GetInstanceUrlAsync(CancellationToken ct = default)
    {
        await EnsureSettingsCacheAsync(ct);

        if (_cachedSettings == null || string.IsNullOrEmpty(_cachedSettings.InstanceUrl))
        {
            throw new CrmAuthenticationException(IntegrationType, "Instance URL is not configured");
        }

        return _cachedSettings.InstanceUrl;
    }

    public async Task<string> GetApiVersionAsync(CancellationToken ct = default)
    {
        await EnsureSettingsCacheAsync(ct);
        return _cachedSettings?.ApiVersion ?? "v9.2";
    }

    public void InvalidateToken()
    {
        _cachedToken = null;
        _tokenExpiry = DateTime.MinValue;
    }
}
