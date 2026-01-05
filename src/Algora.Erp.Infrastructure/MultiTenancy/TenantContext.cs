using Algora.Erp.Domain.Entities.Administration;

namespace Algora.Erp.Infrastructure.MultiTenancy;

/// <summary>
/// Holds the current tenant context for the request
/// </summary>
public class TenantContext
{
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string? ConnectionString { get; set; }
}
