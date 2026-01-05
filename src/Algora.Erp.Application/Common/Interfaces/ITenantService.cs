using Algora.Erp.Domain.Entities.Administration;

namespace Algora.Erp.Application.Common.Interfaces;

/// <summary>
/// Service for tenant resolution and management
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets the current tenant ID from the context
    /// </summary>
    Guid? GetCurrentTenantId();

    /// <summary>
    /// Gets the current tenant from the context
    /// </summary>
    Task<Tenant?> GetCurrentTenantAsync();

    /// <summary>
    /// Sets the current tenant context
    /// </summary>
    void SetCurrentTenant(Guid tenantId);

    /// <summary>
    /// Gets the connection string for a specific tenant
    /// </summary>
    Task<string?> GetTenantConnectionStringAsync(Guid tenantId);

    /// <summary>
    /// Gets a tenant by subdomain
    /// </summary>
    Task<Tenant?> GetTenantBySubdomainAsync(string subdomain);
}
