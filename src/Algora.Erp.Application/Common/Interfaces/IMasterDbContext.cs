using Algora.Erp.Domain.Entities.Administration;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Application.Common.Interfaces;

/// <summary>
/// Interface for the master database context (tenant management)
/// </summary>
public interface IMasterDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<TenantUser> TenantUsers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
