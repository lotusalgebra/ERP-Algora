using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Algora.Erp.Integrations.Common.Exceptions;
using Algora.Erp.Integrations.Common.Interfaces;
using Algora.Erp.Integrations.Common.Models;
using Algora.Erp.Integrations.Dynamics365.Auth;

namespace Algora.Erp.Integrations.Dynamics365.Client;

public class Dynamics365Client : ICrmClient
{
    private readonly HttpClient _httpClient;
    private readonly IDynamics365AuthHandler _authHandler;

    public string CrmType => "Dynamics365";

    public Dynamics365Client(HttpClient httpClient, IDynamics365AuthHandler authHandler)
    {
        _httpClient = httpClient;
        _authHandler = authHandler;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            var token = await _authHandler.GetAccessTokenAsync(ct);
            var instanceUrl = await _authHandler.GetInstanceUrlAsync(ct);
            var apiVersion = await _authHandler.GetApiVersionAsync(ct);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(
                $"{instanceUrl}/api/data/{apiVersion}/WhoAmI", ct);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<T?> GetByIdAsync<T>(string id, CancellationToken ct = default) where T : class
    {
        var entitySet = GetDataverseEntitySet<T>();
        var instanceUrl = await _authHandler.GetInstanceUrlAsync(ct);
        var apiVersion = await _authHandler.GetApiVersionAsync(ct);
        var url = $"{instanceUrl}/api/data/{apiVersion}/{entitySet}({id})";

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
        var entitySet = GetDataverseEntitySet<T>();
        var instanceUrl = await _authHandler.GetInstanceUrlAsync(ct);
        var apiVersion = await _authHandler.GetApiVersionAsync(ct);
        var url = $"{instanceUrl}/api/data/{apiVersion}/{entitySet}?{query}";
        var results = new List<T>();
        string? nextLink = null;

        do
        {
            var requestUrl = nextLink ?? url;
            var response = await SendRequestAsync(HttpMethod.Get, requestUrl, null, ct);
            await EnsureSuccessAsync(response, ct);

            var queryResult = await response.Content.ReadFromJsonAsync<DataverseQueryResult<T>>(ct);
            if (queryResult?.Value != null)
            {
                results.AddRange(queryResult.Value);
            }

            nextLink = queryResult?.ODataNextLink;

        } while (nextLink != null);

        return results;
    }

    public async Task<string> CreateAsync<T>(T entity, CancellationToken ct = default) where T : class
    {
        var entitySet = GetDataverseEntitySet<T>();
        var instanceUrl = await _authHandler.GetInstanceUrlAsync(ct);
        var apiVersion = await _authHandler.GetApiVersionAsync(ct);
        var url = $"{instanceUrl}/api/data/{apiVersion}/{entitySet}";

        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(entity, options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        request.Headers.Add("Prefer", "return=representation");

        var token = await _authHandler.GetAccessTokenAsync(ct);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, ct);
        await EnsureSuccessAsync(response, ct);

        // Get entity ID from OData-EntityId header
        if (response.Headers.TryGetValues("OData-EntityId", out var values))
        {
            var entityIdUrl = values.FirstOrDefault();
            if (entityIdUrl != null)
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    entityIdUrl, @"\(([a-f0-9-]+)\)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
        }

        throw new CrmApiException(CrmType, "Failed to get ID from create response");
    }

    public async Task UpdateAsync<T>(string id, T entity, CancellationToken ct = default) where T : class
    {
        var entitySet = GetDataverseEntitySet<T>();
        var instanceUrl = await _authHandler.GetInstanceUrlAsync(ct);
        var apiVersion = await _authHandler.GetApiVersionAsync(ct);
        var url = $"{instanceUrl}/api/data/{apiVersion}/{entitySet}({id})";

        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(entity, options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };

        var token = await _authHandler.GetAccessTokenAsync(ct);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task DeleteAsync(string id, string entityType, CancellationToken ct = default)
    {
        var entitySet = GetDataverseEntitySetByName(entityType);
        var instanceUrl = await _authHandler.GetInstanceUrlAsync(ct);
        var apiVersion = await _authHandler.GetApiVersionAsync(ct);
        var url = $"{instanceUrl}/api/data/{apiVersion}/{entitySet}({id})";

        var response = await SendRequestAsync(HttpMethod.Delete, url, null, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task<List<T>> GetModifiedSinceAsync<T>(DateTime since, CancellationToken ct = default) where T : class
    {
        var sinceStr = since.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var query = $"$filter=modifiedon gt {sinceStr}&$top=1000";

        return await QueryAsync<T>(query, ct);
    }

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpMethod method, string url, HttpContent? content, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        var token = await _authHandler.GetAccessTokenAsync(ct);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("OData-MaxVersion", "4.0");
        request.Headers.Add("OData-Version", "4.0");

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

    private static string GetDataverseEntitySet<T>()
    {
        var type = typeof(T);
        if (type == typeof(CrmContact)) return "contacts";
        if (type == typeof(CrmLead)) return "leads";
        if (type == typeof(CrmOpportunity)) return "opportunities";
        if (type == typeof(CrmAccount)) return "accounts";
        return type.Name.Replace("Crm", "").ToLowerInvariant() + "s";
    }

    private static string GetDataverseEntitySetByName(string entityType)
    {
        return entityType.ToLowerInvariant() switch
        {
            "contact" => "contacts",
            "lead" => "leads",
            "opportunity" => "opportunities",
            "account" => "accounts",
            _ => entityType.ToLowerInvariant() + "s"
        };
    }

    private class DataverseQueryResult<T>
    {
        [JsonPropertyName("value")]
        public List<T>? Value { get; set; }

        [JsonPropertyName("@odata.nextLink")]
        public string? ODataNextLink { get; set; }

        [JsonPropertyName("@odata.count")]
        public int? ODataCount { get; set; }
    }
}
