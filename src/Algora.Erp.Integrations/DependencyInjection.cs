using Algora.Erp.Integrations.BackgroundServices;
using Algora.Erp.Integrations.Common.Interfaces;
using Algora.Erp.Integrations.Dynamics365.Auth;
using Algora.Erp.Integrations.Dynamics365.Client;
using Algora.Erp.Integrations.Dynamics365.Services;
using Algora.Erp.Integrations.Salesforce.Auth;
using Algora.Erp.Integrations.Salesforce.Client;
using Algora.Erp.Integrations.Salesforce.Services;
using Algora.Erp.Integrations.Shopify.Auth;
using Algora.Erp.Integrations.Shopify.Client;
using Algora.Erp.Integrations.Shopify.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Algora.Erp.Integrations;

public static class DependencyInjection
{
    /// <summary>
    /// Adds CRM integration services to the service collection.
    /// All integrations are registered unconditionally; they check their enabled state at runtime from the database.
    /// Note: IIntegrationSettingsService must be registered by the Infrastructure layer before calling this.
    /// </summary>
    public static IServiceCollection AddCrmIntegrations(this IServiceCollection services)
    {
        // Register all integrations unconditionally
        // They will check their enabled state at runtime from the database
        services.AddSalesforceIntegration();
        services.AddDynamics365Integration();
        services.AddShopifyIntegration();

        // Register background sync service
        services.AddHostedService<CrmSyncBackgroundService>();

        return services;
    }

    private static IServiceCollection AddSalesforceIntegration(this IServiceCollection services)
    {
        // Auth handler
        services.AddHttpClient<ISalesforceAuthHandler, SalesforceAuthHandler>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Salesforce client
        services.AddHttpClient<SalesforceClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Sync service
        services.AddScoped<ICrmSyncService, SalesforceSyncService>();

        return services;
    }

    private static IServiceCollection AddDynamics365Integration(this IServiceCollection services)
    {
        // Auth handler
        services.AddScoped<IDynamics365AuthHandler, Dynamics365AuthHandler>();

        // Dynamics 365 client
        services.AddHttpClient<Dynamics365Client>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Sync service
        services.AddScoped<ICrmSyncService, Dynamics365SyncService>();

        return services;
    }

    private static IServiceCollection AddShopifyIntegration(this IServiceCollection services)
    {
        // Auth handler
        services.AddHttpClient<IShopifyAuthHandler, ShopifyAuthHandler>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Shopify client
        services.AddHttpClient<IShopifyClient, ShopifyClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Sync service
        services.AddScoped<IShopifySyncService, ShopifySyncService>();

        return services;
    }
}
