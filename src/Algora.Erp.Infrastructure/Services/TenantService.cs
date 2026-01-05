using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Administration;
using Algora.Erp.Infrastructure.Data;
using Algora.Erp.Infrastructure.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly TenantContext _tenantContext;
    private readonly MasterDbContext _masterDbContext;

    public TenantService(TenantContext tenantContext, MasterDbContext masterDbContext)
    {
        _tenantContext = tenantContext;
        _masterDbContext = masterDbContext;
    }

    public Guid? GetCurrentTenantId()
    {
        return _tenantContext.TenantId;
    }

    public Task<Tenant?> GetCurrentTenantAsync()
    {
        return Task.FromResult(_tenantContext.Tenant);
    }

    public void SetCurrentTenant(Guid tenantId)
    {
        _tenantContext.TenantId = tenantId;
    }

    public async Task<string?> GetTenantConnectionStringAsync(Guid tenantId)
    {
        var tenant = await _masterDbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);

        return tenant?.ConnectionString;
    }

    public async Task<Tenant?> GetTenantBySubdomainAsync(string subdomain)
    {
        return await _masterDbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.IsActive);
    }
}
