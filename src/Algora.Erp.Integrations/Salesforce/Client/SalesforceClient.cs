using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Algora.Erp.Integrations.Common.Exceptions;
using Algora.Erp.Integrations.Common.Interfaces;
using Algora.Erp.Integrations.Common.Models;
using Algora.Erp.Integrations.Common.Settings;
using Algora.Erp.Integrations.Salesforce.Auth;
using Microsoft.Extensions.Options;

namespace Algora.Erp.Integrations.Salesforce.Client;

public class SalesforceClient : ICrmClient
{
    private readonly HttpClient _httpClient;
    private readonly ISalesforceAuthHandler _authHandler;
    private readonly SalesforceSettings _settings;

    public string CrmType => "Salesforce";

    public SalesforceClient(
        HttpClient httpClient,
        ISalesforceAuthHandler authHandler,
        IOptions<CrmIntegrationsSettings> options)
    {
        _httpClient = httpClient;
        _authHandler = authHandler;
        _settings = options.Value.Salesforce;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            var token = await _authHandler.GetAccessTokenAsync(ct);
            var instanceUrl = await _authHandler.GetInstanceUrlAsync(ct);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(
                $"{instanceUrl}/services/data/{_settings.ApiVersion}/limits", ct);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<T?> GetByIdAsync<T>(string id, CancellationToken ct = default) where T : class
    {
        var entityType = GetSalesforceEntityType<T>();
        var url = await BuildUrlAsync($"sobjects/{entityType}/{id}", ct);

        var response = await SendRequestAsync(HttpMethod.Get, url, null, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(ct);
    }

    public async Task<List<T>> QueryAsync<T>(string query, CancellationToken ct = default) where T : class
    {
        var url = await BuildUrlAsync($"query?q={Uri.EscapeDataString(query)}", ct);
        var results = new List<T>();
        string? nextRecordsUrl = null;

        do
        {
            var requestUrl = nextRecordsUrl ?? url;
            var response = await SendRequestAsync(HttpMethod.Get, requestUrl, null, ct);
            await EnsureSuccessAsync(response, ct);

            var queryResult = await response.Content.ReadFromJsonAsync<SalesforceQueryResult<T>>(ct);
            if (queryResult?.Records != null)
            {
                results.AddRange(queryResult.Records);
            }

            nextRecordsUrl = queryResult?.NextRecordsUrl != null
                ? await BuildUrlAsync(queryResult.NextRecordsUrl.TrimStart('/'), ct)
                : null;

        } while (nextRecordsUrl != null);

        return results;
    }

    public async Task<string> CreateAsync<T>(T entity, CancellationToken ct = default) where T : class
    {
        var entityType = GetSalesforceEntityType<T>();
        var url = await BuildUrlAsync($"sobjects/{entityType}", ct);

        var json = JsonSerializer.Serialize(entity, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await SendRequestAsync(HttpMethod.Post, url, content, ct);
        await EnsureSuccessAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<SalesforceCreateResult>(ct);
        return result?.Id ?? throw new CrmApiException(CrmType, "Failed to get ID from create response");
    }

    public async Task UpdateAsync<T>(string id, T entity, CancellationToken ct = default) where T : class
    {
        var entityType = GetSalesforceEntityType<T>();
        var url = await BuildUrlAsync($"sobjects/{entityType}/{id}", ct);

        var json = JsonSerializer.Serialize(entity, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };

        var token = await _authHandler.GetAccessTokenAsync(ct);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task DeleteAsync(string id, string entityType, CancellationToken ct = default)
    {
        var url = await BuildUrlAsync($"sobjects/{entityType}/{id}", ct);
        var response = await SendRequestAsync(HttpMethod.Delete, url, null, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task<List<T>> GetModifiedSinceAsync<T>(DateTime since, CancellationToken ct = default) where T : class
    {
        var entityType = GetSalesforceEntityType<T>();
        var sinceStr = since.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var query = $"SELECT FIELDS(ALL) FROM {entityType} WHERE LastModifiedDate > {sinceStr} LIMIT 200";

        return await QueryAsync<T>(query, ct);
    }

    private async Task<string> BuildUrlAsync(string endpoint, CancellationToken ct)
    {
        var instanceUrl = await _authHandler.GetInstanceUrlAsync(ct);
        return $"{instanceUrl}/services/data/{_settings.ApiVersion}/{endpoint}";
    }

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpMethod method, string url, HttpContent? content, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        var token = await _authHandler.GetAccessTokenAsync(ct);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _httpClient.SendAsync(request, ct);
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync(ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _authHandler.InvalidateToken();
            throw new CrmAuthenticationException(CrmType, $"Authentication failed: {body}");
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryAfter = response.Headers.RetryAfter?.Delta?.Seconds;
            throw new CrmRateLimitException(CrmType, "Rate limit exceeded", (int?)retryAfter);
        }

        throw new CrmApiException(CrmType, $"API error: {body}", (int)response.StatusCode, body);
    }

    private static string GetSalesforceEntityType<T>()
    {
        var type = typeof(T);
        if (type == typeof(CrmContact)) return "Contact";
        if (type == typeof(CrmLead)) return "Lead";
        if (type == typeof(CrmOpportunity)) return "Opportunity";
        if (type == typeof(CrmAccount)) return "Account";
        return type.Name.Replace("Crm", "");
    }

    private class SalesforceQueryResult<T>
    {
        public int TotalSize { get; set; }
        public bool Done { get; set; }
        public string? NextRecordsUrl { get; set; }
        public List<T>? Records { get; set; }
    }

    private class SalesforceCreateResult
    {
        public string? Id { get; set; }
        public bool Success { get; set; }
        public List<SalesforceError>? Errors { get; set; }
    }

    private class SalesforceError
    {
        public string? StatusCode { get; set; }
        public string? Message { get; set; }
        public List<string>? Fields { get; set; }
    }
}
