using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Algora.Erp.Integrations.Common.Exceptions;

namespace Algora.Erp.Integrations.Shopify.Auth;

public interface IShopifyAuthHandler
{
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
    Task<string> GetShopDomainAsync(CancellationToken ct = default);
    Task<string> GetApiVersionAsync(CancellationToken ct = default);
    Task<ShopifySettingsData?> GetSettingsAsync(CancellationToken ct = default);
    Task<bool> ValidateConnectionAsync(CancellationToken ct = default);
}

public class ShopifyAuthHandler : IShopifyAuthHandler
{
    private readonly HttpClient _httpClient;
    private readonly IIntegrationSettingsService _settingsService;
    private bool _isValidated;

    // Cached values for performance
    private ShopifySettingsData? _cachedSettings;
    private ShopifyCredentials? _cachedCredentials;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public const string IntegrationType = "Shopify";

    public ShopifyAuthHandler(HttpClient httpClient, IIntegrationSettingsService settingsService)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
    }

    private async Task EnsureCacheAsync(CancellationToken ct)
    {
        if (_cachedSettings != null && _cachedCredentials != null && DateTime.UtcNow < _cacheExpiry)
            return;

        _cachedSettings = await _settingsService.GetSettingsAsync<ShopifySettingsData>(IntegrationType, ct);
        _cachedCredentials = await _settingsService.GetCredentialsAsync<ShopifyCredentials>(IntegrationType, ct);
        _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        await EnsureCacheAsync(ct);

        if (_cachedCredentials == null || string.IsNullOrEmpty(_cachedCredentials.AccessToken))
        {
            throw new CrmAuthenticationException(IntegrationType, "Access token is not configured");
        }

        return _cachedCredentials.AccessToken;
    }

    public async Task<string> GetShopDomainAsync(CancellationToken ct = default)
    {
        await EnsureCacheAsync(ct);

        if (_cachedSettings == null || string.IsNullOrEmpty(_cachedSettings.ShopDomain))
        {
            throw new CrmAuthenticationException(IntegrationType, "Shop domain is not configured");
        }

        var domain = _cachedSettings.ShopDomain;

        // Ensure domain has proper format
        if (!domain.Contains(".myshopify.com"))
        {
            domain = $"{domain}.myshopify.com";
        }

        return domain;
    }

    public async Task<string> GetApiVersionAsync(CancellationToken ct = default)
    {
        await EnsureCacheAsync(ct);
        return _cachedSettings?.ApiVersion ?? "2024-01";
    }

    public async Task<ShopifySettingsData?> GetSettingsAsync(CancellationToken ct = default)
    {
        await EnsureCacheAsync(ct);
        return _cachedSettings;
    }

    public async Task<bool> ValidateConnectionAsync(CancellationToken ct = default)
    {
        if (_isValidated) return true;

        try
        {
            var domain = await GetShopDomainAsync(ct);
            var token = await GetAccessTokenAsync(ct);
            var apiVersion = await GetApiVersionAsync(ct);
            var url = $"https://{domain}/admin/api/{apiVersion}/shop.json";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-Shopify-Access-Token", token);

            var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                await _settingsService.UpdateTestResultAsync(IntegrationType, false, $"HTTP {(int)response.StatusCode}: {body}", ct);
                throw new CrmAuthenticationException(IntegrationType, $"Failed to validate connection: {body}");
            }

            await _settingsService.UpdateTestResultAsync(IntegrationType, true, null, ct);
            _isValidated = true;
            return true;
        }
        catch (HttpRequestException ex)
        {
            await _settingsService.UpdateTestResultAsync(IntegrationType, false, ex.Message, ct);
            throw new CrmAuthenticationException(IntegrationType, $"Failed to connect to Shopify: {ex.Message}", ex);
        }
    }
}
