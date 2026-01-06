using Algora.Erp.Integrations.BackgroundServices;
using Algora.Erp.Integrations.Common.Interfaces;
using Algora.Erp.Integrations.Common.Settings;
using Algora.Erp.Integrations.Dynamics365.Auth;
using Algora.Erp.Integrations.Dynamics365.Client;
using Algora.Erp.Integrations.Dynamics365.Services;
using Algora.Erp.Integrations.Salesforce.Auth;
using Algora.Erp.Integrations.Salesforce.Client;
using Algora.Erp.Integrations.Salesforce.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Algora.Erp.Integrations;

public static class DependencyInjection
{
    public static IServiceCollection AddCrmIntegrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind settings
        services.Configure<CrmIntegrationsSettings>(
            configuration.GetSection(CrmIntegrationsSettings.SectionName));

        var settings = configuration.GetSection(CrmIntegrationsSettings.SectionName)
            .Get<CrmIntegrationsSettings>() ?? new CrmIntegrationsSettings();

        // Register Salesforce integration
        if (settings.Salesforce.Enabled)
        {
            services.AddSalesforceIntegration();
        }

        // Register Dynamics 365 integration
        if (settings.Dynamics365.Enabled)
        {
            services.AddDynamics365Integration();
        }

        // Register background sync service
        if (settings.Salesforce.Enabled || settings.Dynamics365.Enabled)
        {
            services.AddHostedService<CrmSyncBackgroundService>();
        }

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
        services.AddSingleton<IDynamics365AuthHandler, Dynamics365AuthHandler>();

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
}
