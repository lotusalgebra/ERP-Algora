using Algora.Erp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Algora.Erp.Infrastructure.MultiTenancy;

/// <summary>
/// Factory to create tenant-specific database connections
/// </summary>
public interface ITenantConnectionFactory
{
    ApplicationDbContext CreateContext();
    ApplicationDbContext CreateContext(string connectionString);
}

public class TenantConnectionFactory : ITenantConnectionFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TenantContext _tenantContext;

    public TenantConnectionFactory(IServiceProvider serviceProvider, TenantContext tenantContext)
    {
        _serviceProvider = serviceProvider;
        _tenantContext = tenantContext;
    }

    public ApplicationDbContext CreateContext()
    {
        if (string.IsNullOrEmpty(_tenantContext.ConnectionString))
        {
            throw new InvalidOperationException("No tenant context available. Ensure tenant middleware has run.");
        }

        return CreateContext(_tenantContext.ConnectionString);
    }

    public ApplicationDbContext CreateContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        var currentUserService = _serviceProvider.GetRequiredService<Algora.Erp.Application.Common.Interfaces.ICurrentUserService>();
        var dateTime = _serviceProvider.GetRequiredService<Algora.Erp.Application.Common.Interfaces.IDateTime>();

        return new ApplicationDbContext(optionsBuilder.Options, currentUserService, dateTime);
    }
}
