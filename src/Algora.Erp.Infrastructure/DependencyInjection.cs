using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Application.Common.Interfaces.Ecommerce;
using Algora.Erp.Infrastructure.Data;
using Algora.Erp.Infrastructure.MultiTenancy;
using Algora.Erp.Infrastructure.Services;
using Algora.Erp.Infrastructure.Services.Ecommerce;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace Algora.Erp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
        // Master database context (for tenant management)
        services.AddDbContext<MasterDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Master"),
                b => b.MigrationsAssembly(typeof(MasterDbContext).Assembly.FullName)));

        // Tenant context (scoped per request)
        services.AddScoped<TenantContext>();

        // Tenant resolver and connection factory
        services.AddScoped<ITenantResolver, TenantResolver>();
        services.AddScoped<ITenantConnectionFactory, TenantConnectionFactory>();

        // Register interfaces
        services.AddScoped<IMasterDbContext>(provider => provider.GetRequiredService<MasterDbContext>());
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTime, DateTimeService>();
        services.AddScoped<IInvoicePdfService, InvoicePdfService>();
        services.AddScoped<IRecurringInvoiceService, RecurringInvoiceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IFinancialReportService, FinancialReportService>();

        // Ecommerce services
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IProductCatalogService, ProductCatalogService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IShoppingCartService, ShoppingCartService>();
        services.AddScoped<IWebOrderService, WebOrderService>();
        services.AddScoped<IWebCustomerService, WebCustomerService>();

        // Email service
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.AddScoped<IEmailService, EmailService>();

        // Recurring invoice background service
        services.Configure<RecurringInvoiceSettings>(configuration.GetSection("RecurringInvoices"));
        var recurringSettings = configuration.GetSection("RecurringInvoices").Get<RecurringInvoiceSettings>() ?? new RecurringInvoiceSettings();
        if (recurringSettings.Enabled)
        {
            services.AddHostedService<RecurringInvoiceBackgroundService>();
        }

        // Tenant-specific DbContext (uses connection factory)
        services.AddScoped<IApplicationDbContext>(provider =>
        {
            var factory = provider.GetRequiredService<ITenantConnectionFactory>();
            var tenantContext = provider.GetRequiredService<TenantContext>();

            // If no tenant is resolved, check for development fallback
            if (string.IsNullOrEmpty(tenantContext.ConnectionString))
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var useDevelopmentTenant = config.GetValue<bool>("UseDevelopmentTenant");
                var defaultConnection = config.GetConnectionString("Default");

                if (useDevelopmentTenant && !string.IsNullOrEmpty(defaultConnection))
                {
                    // Use default connection for development
                    tenantContext.ConnectionString = defaultConnection;
                    tenantContext.TenantId = Guid.Empty;
                }
                else
                {
                    throw new InvalidOperationException(
                        "No tenant context available. Ensure the request has a valid tenant.");
                }
            }

            return factory.CreateContext();
        });

        // Memory cache for tenant resolution caching
        services.AddMemoryCache();

        return services;
    }
}
