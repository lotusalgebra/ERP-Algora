using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Algora.Erp.Integrations.Common.Exceptions;
using Algora.Erp.Integrations.Common.Settings;
using Microsoft.Extensions.Options;

namespace Algora.Erp.Integrations.Salesforce.Auth;

public interface ISalesforceAuthHandler
{
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
    Task<string> GetInstanceUrlAsync(CancellationToken ct = default);
    void InvalidateToken();
}

public class SalesforceAuthHandler : ISalesforceAuthHandler
{
    private readonly HttpClient _httpClient;
    private readonly SalesforceSettings _settings;
    private string? _accessToken;
    private string? _instanceUrl;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public SalesforceAuthHandler(HttpClient httpClient, IOptions<CrmIntegrationsSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value.Salesforce;
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

    public void InvalidateToken()
    {
        _accessToken = null;
        _instanceUrl = null;
        _tokenExpiry = DateTime.MinValue;
    }

    private async Task AuthenticateAsync(CancellationToken ct)
    {
        var tokenUrl = $"{_settings.InstanceUrl}/services/oauth2/token";

        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["username"] = _settings.Username,
            ["password"] = $"{_settings.Password}{_settings.SecurityToken}"
        };

        var content = new FormUrlEncodedContent(parameters);

        try
        {
            var response = await _httpClient.PostAsync(tokenUrl, content, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new CrmAuthenticationException("Salesforce",
                    $"Authentication failed: {responseBody}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<SalesforceTokenResponse>(ct);

            if (tokenResponse == null)
            {
                throw new CrmAuthenticationException("Salesforce", "Failed to parse token response");
            }

            _accessToken = tokenResponse.AccessToken;
            _instanceUrl = tokenResponse.InstanceUrl;
            _tokenExpiry = DateTime.UtcNow.AddHours(1); // Salesforce tokens typically last 2 hours
        }
        catch (HttpRequestException ex)
        {
            throw new CrmAuthenticationException("Salesforce",
                $"Failed to connect to Salesforce: {ex.Message}", ex);
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
