using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Algora.Erp.Infrastructure.MultiTenancy;

/// <summary>
/// Middleware to resolve and set the current tenant for each request
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantContext = context.RequestServices.GetRequiredService<TenantContext>();
        var tenantResolver = context.RequestServices.GetRequiredService<ITenantResolver>();

        // Extract subdomain
        var host = context.Request.Host.Host;
        var subdomain = ExtractSubdomain(host);

        // Extract tenant header (for API calls)
        context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader);

        // Extract tenant from claims (for authenticated users)
        Guid? tenantIdFromClaim = null;
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenant_id");
            if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var parsedTenantId))
            {
                tenantIdFromClaim = parsedTenantId;
            }
        }

        // Resolve tenant
        var tenant = await tenantResolver.ResolveAsync(subdomain, tenantHeader.FirstOrDefault(), tenantIdFromClaim);

        if (tenant != null)
        {
            tenantContext.TenantId = tenant.Id;
            tenantContext.Tenant = tenant;
            tenantContext.ConnectionString = tenant.ConnectionString;
        }

        await _next(context);
    }

    private static string? ExtractSubdomain(string host)
    {
        // Remove port if present
        var hostWithoutPort = host.Split(':')[0];

        // Skip localhost and IP addresses
        if (hostWithoutPort == "localhost" || IsIpAddress(hostWithoutPort))
        {
            return null;
        }

        var parts = hostWithoutPort.Split('.');

        // Expect at least: subdomain.domain.tld
        if (parts.Length >= 3)
        {
            return parts[0];
        }

        return null;
    }

    private static bool IsIpAddress(string host)
    {
        return System.Net.IPAddress.TryParse(host, out _);
    }
}
