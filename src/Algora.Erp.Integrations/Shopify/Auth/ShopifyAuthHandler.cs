using Algora.Erp.Integrations.Common.Exceptions;
using Algora.Erp.Integrations.Common.Settings;
using Microsoft.Extensions.Options;

namespace Algora.Erp.Integrations.Shopify.Auth;

public interface IShopifyAuthHandler
{
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
    string GetShopDomain();
    string GetApiVersion();
    Task<bool> ValidateConnectionAsync(CancellationToken ct = default);
}

public class ShopifyAuthHandler : IShopifyAuthHandler
{
    private readonly HttpClient _httpClient;
    private readonly ShopifySettings _settings;
    private bool _isValidated;

    public ShopifyAuthHandler(HttpClient httpClient, IOptions<CrmIntegrationsSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value.Shopify;
    }

    public Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_settings.AccessToken))
        {
            throw new CrmAuthenticationException("Shopify", "Access token is not configured");
        }

        return Task.FromResult(_settings.AccessToken);
    }

    public string GetShopDomain()
    {
        if (string.IsNullOrEmpty(_settings.ShopDomain))
        {
            throw new CrmAuthenticationException("Shopify", "Shop domain is not configured");
        }

        var domain = _settings.ShopDomain;

        // Ensure domain has proper format
        if (!domain.Contains(".myshopify.com"))
        {
            domain = $"{domain}.myshopify.com";
        }

        return domain;
    }

    public string GetApiVersion()
    {
        return _settings.ApiVersion;
    }

    public async Task<bool> ValidateConnectionAsync(CancellationToken ct = default)
    {
        if (_isValidated) return true;

        try
        {
            var domain = GetShopDomain();
            var token = await GetAccessTokenAsync(ct);
            var url = $"https://{domain}/admin/api/{_settings.ApiVersion}/shop.json";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-Shopify-Access-Token", token);

            var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                throw new CrmAuthenticationException("Shopify", $"Failed to validate connection: {body}");
            }

            _isValidated = true;
            return true;
        }
        catch (HttpRequestException ex)
        {
            throw new CrmAuthenticationException("Shopify", $"Failed to connect to Shopify: {ex.Message}", ex);
        }
    }
}
