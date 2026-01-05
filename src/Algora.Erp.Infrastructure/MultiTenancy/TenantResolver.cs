using Algora.Erp.Domain.Entities.Administration;
using Algora.Erp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Algora.Erp.Infrastructure.MultiTenancy;

/// <summary>
/// Resolves tenant from subdomain, header, or claim
/// </summary>
public interface ITenantResolver
{
    Task<Tenant?> ResolveAsync(string? subdomain, string? tenantHeader, Guid? tenantIdFromClaim);
}

public class TenantResolver : ITenantResolver
{
    private readonly MasterDbContext _masterDbContext;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public TenantResolver(MasterDbContext masterDbContext, IMemoryCache cache)
    {
        _masterDbContext = masterDbContext;
        _cache = cache;
    }

    public async Task<Tenant?> ResolveAsync(string? subdomain, string? tenantHeader, Guid? tenantIdFromClaim)
    {
        // Priority: 1. Claim, 2. Header, 3. Subdomain
        if (tenantIdFromClaim.HasValue)
        {
            return await GetTenantByIdAsync(tenantIdFromClaim.Value);
        }

        if (!string.IsNullOrEmpty(tenantHeader) && Guid.TryParse(tenantHeader, out var headerTenantId))
        {
            return await GetTenantByIdAsync(headerTenantId);
        }

        if (!string.IsNullOrEmpty(subdomain))
        {
            return await GetTenantBySubdomainAsync(subdomain);
        }

        return null;
    }

    private async Task<Tenant?> GetTenantByIdAsync(Guid tenantId)
    {
        var cacheKey = $"tenant_id_{tenantId}";

        if (!_cache.TryGetValue(cacheKey, out Tenant? tenant))
        {
            tenant = await _masterDbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);

            if (tenant != null)
            {
                _cache.Set(cacheKey, tenant, _cacheExpiration);
            }
        }

        return tenant;
    }

    private async Task<Tenant?> GetTenantBySubdomainAsync(string subdomain)
    {
        var cacheKey = $"tenant_subdomain_{subdomain.ToLowerInvariant()}";

        if (!_cache.TryGetValue(cacheKey, out Tenant? tenant))
        {
            tenant = await _masterDbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.IsActive);

            if (tenant != null)
            {
                _cache.Set(cacheKey, tenant, _cacheExpiration);
            }
        }

        return tenant;
    }
}
