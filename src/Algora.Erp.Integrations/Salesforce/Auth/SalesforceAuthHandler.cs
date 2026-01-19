using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Algora.Erp.Integrations.Common.Exceptions;

namespace Algora.Erp.Integrations.Salesforce.Auth;

public interface ISalesforceAuthHandler
{
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
    Task<string> GetInstanceUrlAsync(CancellationToken ct = default);
    Task<string> GetApiVersionAsync(CancellationToken ct = default);
    void InvalidateToken();
}

public class SalesforceAuthHandler : ISalesforceAuthHandler
{
    private readonly HttpClient _httpClient;
    private readonly IIntegrationSettingsService _settingsService;
    private string? _accessToken;
    private string? _instanceUrl;
    private DateTime _tokenExpiry = DateTime.MinValue;

    // Cached settings
    private SalesforceSettingsData? _cachedSettings;
    private SalesforceCredentials? _cachedCredentials;
    private DateTime _settingsCacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan SettingsCacheDuration = TimeSpan.FromMinutes(5);

    public const string IntegrationType = "Salesforce";

    public SalesforceAuthHandler(HttpClient httpClient, IIntegrationSettingsService settingsService)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
    }

    private async Task EnsureSettingsCacheAsync(CancellationToken ct)
    {
        if (_cachedSettings != null && _cachedCredentials != null && DateTime.UtcNow < _settingsCacheExpiry)
            return;

        _cachedSettings = await _settingsService.GetSettingsAsync<SalesforceSettingsData>(IntegrationType, ct);
        _cachedCredentials = await _settingsService.GetCredentialsAsync<SalesforceCredentials>(IntegrationType, ct);
        _settingsCacheExpiry = DateTime.UtcNow.Add(SettingsCacheDuration);
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
        {
            return _accessToken;
        }

        await AuthenticateAsync(ct);
        return _accessToken!;
    }

    public async Task<string> GetInstanceUrlAsync(CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(_instanceUrl))
        {
            return _instanceUrl;
        }

        await AuthenticateAsync(ct);
        return _instanceUrl!;
    }

    public async Task<string> GetApiVersionAsync(CancellationToken ct = default)
    {
        await EnsureSettingsCacheAsync(ct);
        return _cachedSettings?.ApiVersion ?? "v58.0";
    }

    public void InvalidateToken()
    {
        _accessToken = null;
        _instanceUrl = null;
        _tokenExpiry = DateTime.MinValue;
    }

    private async Task AuthenticateAsync(CancellationToken ct)
    {
        await EnsureSettingsCacheAsync(ct);

        if (_cachedSettings == null || _cachedCredentials == null)
        {
            throw new CrmAuthenticationException(IntegrationType, "Salesforce integration is not configured");
        }

        var tokenUrl = $"{_cachedSettings.InstanceUrl}/services/oauth2/token";

        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _cachedCredentials.ClientId,
            ["client_secret"] = _cachedCredentials.ClientSecret,
            ["username"] = _cachedSettings.Username,
            ["password"] = $"{_cachedCredentials.Password}{_cachedCredentials.SecurityToken}"
        };

        var content = new FormUrlEncodedContent(parameters);

        try
        {
            var response = await _httpClient.PostAsync(tokenUrl, content, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                await _settingsService.UpdateTestResultAsync(IntegrationType, false, $"Authentication failed: {responseBody}", ct);
                throw new CrmAuthenticationException(IntegrationType, $"Authentication failed: {responseBody}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<SalesforceTokenResponse>(ct);

            if (tokenResponse == null)
            {
                await _settingsService.UpdateTestResultAsync(IntegrationType, false, "Failed to parse token response", ct);
                throw new CrmAuthenticationException(IntegrationType, "Failed to parse token response");
            }

            _accessToken = tokenResponse.AccessToken;
            _instanceUrl = tokenResponse.InstanceUrl;
            _tokenExpiry = DateTime.UtcNow.AddHours(1); // Salesforce tokens typically last 2 hours

            await _settingsService.UpdateTestResultAsync(IntegrationType, true, null, ct);
        }
        catch (HttpRequestException ex)
        {
            await _settingsService.UpdateTestResultAsync(IntegrationType, false, ex.Message, ct);
            throw new CrmAuthenticationException(IntegrationType, $"Failed to connect to Salesforce: {ex.Message}", ex);
        }
    }

    private class SalesforceTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("instance_url")]
        public string InstanceUrl { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("issued_at")]
        public string IssuedAt { get; set; } = string.Empty;
    }
}
