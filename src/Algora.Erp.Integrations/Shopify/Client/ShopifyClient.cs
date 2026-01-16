using System.Net;
using System.Net.Http.Json;
using Algora.Erp.Integrations.Common.Exceptions;
using Algora.Erp.Integrations.Shopify.Auth;
using Algora.Erp.Integrations.Shopify.Models;

namespace Algora.Erp.Integrations.Shopify.Client;

public interface IShopifyClient
{
    string CrmType { get; }
    Task<bool> TestConnectionAsync(CancellationToken ct = default);

    // Customers
    Task<List<ShopifyCustomer>> GetCustomersAsync(int limit = 250, string? sinceId = null, CancellationToken ct = default);
    Task<ShopifyCustomer?> GetCustomerByIdAsync(long id, CancellationToken ct = default);
    Task<List<ShopifyCustomer>> GetCustomersModifiedSinceAsync(DateTime since, CancellationToken ct = default);

    // Orders
    Task<List<ShopifyOrder>> GetOrdersAsync(int limit = 250, string? sinceId = null, string? status = "any", CancellationToken ct = default);
    Task<ShopifyOrder?> GetOrderByIdAsync(long id, CancellationToken ct = default);
    Task<List<ShopifyOrder>> GetOrdersModifiedSinceAsync(DateTime since, CancellationToken ct = default);

    // Products
    Task<List<ShopifyProduct>> GetProductsAsync(int limit = 250, string? sinceId = null, CancellationToken ct = default);
    Task<ShopifyProduct?> GetProductByIdAsync(long id, CancellationToken ct = default);
    Task<List<ShopifyProduct>> GetProductsModifiedSinceAsync(DateTime since, CancellationToken ct = default);

    // Inventory
    Task<List<ShopifyLocation>> GetLocationsAsync(CancellationToken ct = default);
    Task<List<ShopifyInventoryLevel>> GetInventoryLevelsAsync(long locationId, CancellationToken ct = default);

    // Shop Info
    Task<ShopifyShop?> GetShopInfoAsync(CancellationToken ct = default);
}

public class ShopifyClient : IShopifyClient
{
    private readonly HttpClient _httpClient;
    private readonly IShopifyAuthHandler _authHandler;

    public string CrmType => "Shopify";

    public ShopifyClient(HttpClient httpClient, IShopifyAuthHandler authHandler)
    {
        _httpClient = httpClient;
        _authHandler = authHandler;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            var shop = await GetShopInfoAsync(ct);
            return shop != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ShopifyShop?> GetShopInfoAsync(CancellationToken ct = default)
    {
        var url = await BuildUrlAsync("shop.json", ct);
        var response = await SendRequestAsync(HttpMethod.Get, url, null, ct);
        await EnsureSuccessAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<ShopifyShopResponse>(ct);
        return result?.Shop;
    }

    #region Customers

    public async Task<List<ShopifyCustomer>> GetCustomersAsync(int limit = 250, string? sinceId = null, CancellationToken ct = default)
    {
        var customers = new List<ShopifyCustomer>();
        string? pageInfo = null;

        do
        {
            string url;
            if (pageInfo != null)
            {
                url = await BuildUrlAsync($"customers.json?limit={limit}&page_info={pageInfo}", ct);
            }
            else
            {
                url = await BuildUrlAsync($"customers.json?limit={limit}" + (sinceId != null ? $"&since_id={sinceId}" : ""), ct);
            }

            var response = await SendRequestAsync(HttpMethod.Get, url, null, ct);
            await EnsureSuccessAsync(response, ct);

            var result = await response.Content.ReadFromJsonAsync<ShopifyCustomersResponse>(ct);
            if (result?.Customers != null)
            {
                customers.AddRange(result.Customers);
            }

            pageInfo = GetNextPageInfo(response);

        } while (pageInfo != null);

        return customers;
    }

    public async Task<ShopifyCustomer?> GetCustomerByIdAsync(long id, CancellationToken ct = default)
    {
        var url = await BuildUrlAsync($"customers/{id}.json", ct);
        var response = await SendRequestAsync(HttpMethod.Get, url, null, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response, ct);
        var result = await response.Content.ReadFromJsonAsync<ShopifyCustomerResponse>(ct);
        return result?.Customer;
    }

    public async Task<List<ShopifyCustomer>> GetCustomersModifiedSinceAsync(DateTime since, CancellationToken ct = default)
    {
        var customers = new List<ShopifyCustomer>();
        var sinceStr = since.ToString("yyyy-MM-ddTHH:mm:sszzz");
        string? pageInfo = null;

        do
        {
            string url;
            if (pageInfo != null)
            {
                url = await BuildUrlAsync($"customers.json?limit=250&page_info={pageInfo}", ct);
            }
            else
            {
                url = await BuildUrlAsync($"customers.json?limit=250&updated_at_min={Uri.EscapeDataString(sinceStr)}", ct);
            }

            var response = await SendRequestAsync(HttpMethod.Get, url, null, ct);
            await EnsureSuccessAsync(response, ct);

            var result = await response.Content.ReadFromJsonAsync<ShopifyCustomersResponse>(ct);
            if (result?.Customers != null)
            {
                customers.AddRange(result.Customers);
            }

            pageInfo = GetNextPageInfo(response);

        } while (pageInfo != null);

        return customers;
    }

    #endregion

    #region Orders

    public async Task<List<ShopifyOrder>> GetOrdersAsync(int limit = 250, string? sinceId = null, string? status = "any", CancellationToken ct = default)
    {
        var orders = new List<ShopifyOrder>();
        string? pageInfo = null;

        do
        {
            string url;
            if (pageInfo != null)
            {
                url = await BuildUrlAsync($"orders.json?limit={limit}&page_info={pageInfo}", ct);
            }
            else
            {
                url = await BuildUrlAsync($"orders.json?limit={limit}&status={status}" + (sinceId != null ? $"&since_id={sinceId}" : ""), ct);
            }

            var response = await SendRequestAsync(HttpMethod.Get, url, null, ct);
            await EnsureSuccessAsync(response, ct);

            var result = await response.Content.ReadFromJsonAsync<ShopifyOrdersResponse>(ct);
            if (result?.Orders != null)
            {
                orders.AddRange(result.Orders);
            }

            pageInfo = GetNextPageInfo(response);

        } while (pageInfo != null);

        return orders;
    }

    public async Task<ShopifyOrder?> GetOrderByIdAsync(long id, CancellationToken ct = default)
    {
        var url = await BuildUrlAsync($"orders/{id}.json", ct);
        var response = await SendRequestAsync(HttpMethod.Get, url, null, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response, ct);
        var result = await response.Content.ReadFromJsonAsync<ShopifyOrderResponse>(ct);
        return result?.Order;
    }

    public async Task<List<ShopifyOrder>> GetOrdersModifiedSinceAsync(DateTime since, CancellationToken ct = default)
    {
        var orders = new List<ShopifyOrder>();
        var sinceStr = since.ToString("yyyy-MM-ddTHH:mm:sszzz");
        string? pageInfo = null;

        do
        {
            string url;
            if (pageInfo != null)
            {
                url = await BuildUrlAsync($"orders.json?limit=250&page_info={pageInfo}", ct);
            }
            else
            {
                url = await BuildUrlAsync($"orders.json?limit=250&status=any&updated_at_min={Uri.EscapeDataString(sinceStr)}", ct);
            }

            var response = await SendRequestAsync(HttpMethod.Get, url, null, ct);
            await EnsureSuccessAsync(response, ct);

            var result = await response.Content.ReadFromJsonAsync<ShopifyOrdersResponse>(ct);
            if (result?.Orders != null)
            {
                orders.AddRange(result.Orders);
            }

            pageInfo = GetNextPageInfo(response);

        } while (pageInfo != null);

        return orders;
    }

    #endregion

    #region Products

    public async Task<List<ShopifyProduct>> GetProductsAsync(int limit = 250, string? sinceId = null, CancellationToken ct = default)
    {
        var products = new List<ShopifyProduct>();
        string? pageInfo = null;

        do
        {
            string url;
            if (pageInfo != null)
            {
                url = await BuildUrlAsync($"products.json?limit={limit}&page_info={pageInfo}", ct);
            }
            else
            {
                url = await BuildUrlAsync($"products.json?limit={limit}" + (sinceId != null ? $"&since_id={sinceId}" : ""), ct);
            }

            var response = await SendRequestAsync(HttpMethod.Get, url, null, ct);
            await EnsureSuccessAsync(response, ct);

            var result = await response.Content.ReadFromJsonAsync<ShopifyProductsResponse>(ct);
            if (result?.Products != null)
            {
                products.AddRange(result.Products);
            }

            pageInfo = GetNextPageInfo(response);

        } while (pageInfo != null);

        return products;
    }

    public async Task<ShopifyProduct?> GetProductByIdAsync(long id, CancellationToken ct = default)
    {
        var url = await BuildUrlAsync($"products/{id}.json", ct);
        var response = await SendRequestAsync(HttpMethod.Get, url, null, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response, ct);
        var result = await response.Content.ReadFromJsonAsync<ShopifyProductResponse>(ct);
        return result?.Product;
    }

    public async Task<List<ShopifyProduct>> GetProductsModifiedSinceAsync(DateTime since, CancellationToken ct = default)
    {
        var products = new List<ShopifyProduct>();
        var sinceStr = since.ToString("yyyy-MM-ddTHH:mm:sszzz");
        string? pageInfo = null;

        do
        {
            string url;
            if (pageInfo != null)
            {
                url = await BuildUrlAsync($"products.json?limit=250&page_info={pageInfo}", ct);
            }
            else
            {
                url = await BuildUrlAsync($"products.json?limit=250&updated_at_min={Uri.EscapeDataString(sinceStr)}", ct);
            }

            var response = await SendRequestAsync(HttpMethod.Get, url, null, ct);
            await EnsureSuccessAsync(response, ct);

            var result = await response.Content.ReadFromJsonAsync<ShopifyProductsResponse>(ct);
            if (result?.Products != null)
            {
                products.AddRange(result.Products);
            }

            pageInfo = GetNextPageInfo(response);

        } while (pageInfo != null);

        return products;
    }

    #endregion

    #region Inventory

    public async Task<List<ShopifyLocation>> GetLocationsAsync(CancellationToken ct = default)
    {
        var url = await BuildUrlAsync("locations.json", ct);
        var response = await SendRequestAsync(HttpMethod.Get, url, null, ct);
        await EnsureSuccessAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<ShopifyLocationsResponse>(ct);
        return result?.Locations ?? new List<ShopifyLocation>();
    }

    public async Task<List<ShopifyInventoryLevel>> GetInventoryLevelsAsync(long locationId, CancellationToken ct = default)
    {
        var levels = new List<ShopifyInventoryLevel>();
        string? pageInfo = null;

        do
        {
            string url;
            if (pageInfo != null)
            {
                url = await BuildUrlAsync($"inventory_levels.json?limit=250&page_info={pageInfo}", ct);
            }
            else
            {
                url = await BuildUrlAsync($"inventory_levels.json?limit=250&location_ids={locationId}", ct);
            }

            var response = await SendRequestAsync(HttpMethod.Get, url, null, ct);
            await EnsureSuccessAsync(response, ct);

            var result = await response.Content.ReadFromJsonAsync<ShopifyInventoryLevelsResponse>(ct);
            if (result?.InventoryLevels != null)
            {
                levels.AddRange(result.InventoryLevels);
            }

            pageInfo = GetNextPageInfo(response);

        } while (pageInfo != null);

        return levels;
    }

    #endregion

    #region Private Methods

    private async Task<string> BuildUrlAsync(string endpoint, CancellationToken ct)
    {
        var domain = await _authHandler.GetShopDomainAsync(ct);
        var version = await _authHandler.GetApiVersionAsync(ct);
        return $"https://{domain}/admin/api/{version}/{endpoint}";
    }

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpMethod method, string url, HttpContent? content, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        var token = await _authHandler.GetAccessTokenAsync(ct);
        request.Headers.Add("X-Shopify-Access-Token", token);

        return await _httpClient.SendAsync(request, ct);
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync(ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new CrmAuthenticationException(CrmType, $"Authentication failed: {body}");
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryAfter = response.Headers.RetryAfter?.Delta?.Seconds;
            throw new CrmRateLimitException(CrmType, "Rate limit exceeded", (int?)retryAfter);
        }

        throw new CrmApiException(CrmType, $"API error: {body}", (int)response.StatusCode, body);
    }

    private static string? GetNextPageInfo(HttpResponseMessage response)
    {
        // Shopify uses cursor-based pagination via Link header
        if (!response.Headers.TryGetValues("Link", out var linkValues))
            return null;

        var linkHeader = linkValues.FirstOrDefault();
        if (string.IsNullOrEmpty(linkHeader))
            return null;

        // Parse Link header for "next" relation
        // Format: <url>; rel="next", <url>; rel="previous"
        var links = linkHeader.Split(',');
        foreach (var link in links)
        {
            if (link.Contains("rel=\"next\""))
            {
                var urlPart = link.Split(';')[0].Trim();
                var url = urlPart.Trim('<', '>');

                // Extract page_info parameter
                var uri = new Uri(url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                return query["page_info"];
            }
        }

        return null;
    }

    #endregion
}
