using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Algora.Erp.Integrations.Common.Interfaces;
using Algora.Erp.Integrations.Common.Models;
using Algora.Erp.Integrations.Dynamics365.Auth;
using Algora.Erp.Integrations.Salesforce.Auth;
using Algora.Erp.Integrations.Shopify.Auth;
using Algora.Erp.Integrations.Shopify.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Algora.Erp.Integrations.BackgroundServices;

public class CrmSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CrmSyncBackgroundService> _logger;
    private const int DefaultIntervalMinutes = 30;

    public CrmSyncBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<CrmSyncBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CRM Sync Background Service starting");

        // Initial delay to let the application start properly
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunSyncCycleAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during CRM sync cycle");
            }

            // Wait for the minimum interval among enabled CRMs
            var intervalMinutes = await GetMinimumIntervalAsync(stoppingToken);
            _logger.LogInformation("Next CRM sync in {Minutes} minutes", intervalMinutes);
            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }

        _logger.LogInformation("CRM Sync Background Service stopping");
    }

    private async Task RunSyncCycleAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var settingsService = scope.ServiceProvider.GetRequiredService<IIntegrationSettingsService>();

        // Get all enabled integrations
        var enabledIntegrations = await settingsService.GetEnabledIntegrationsAsync(ct);

        if (enabledIntegrations.Count == 0)
        {
            _logger.LogDebug("No CRM integrations are enabled");
            return;
        }

        // Sync Salesforce
        if (enabledIntegrations.Contains(SalesforceAuthHandler.IntegrationType))
        {
            await SyncSalesforceAsync(scope.ServiceProvider, ct);
        }

        // Sync Dynamics 365
        if (enabledIntegrations.Contains(Dynamics365AuthHandler.IntegrationType))
        {
            await SyncDynamics365Async(scope.ServiceProvider, ct);
        }

        // Sync Shopify
        if (enabledIntegrations.Contains(ShopifyAuthHandler.IntegrationType))
        {
            await SyncShopifyAsync(scope.ServiceProvider, ct);
        }
    }

    private async Task SyncSalesforceAsync(IServiceProvider sp, CancellationToken ct)
    {
        _logger.LogInformation("Starting sync for Salesforce");

        try
        {
            var syncServices = sp.GetServices<ICrmSyncService>();
            var salesforceSync = syncServices.FirstOrDefault(s => s.CrmType == SalesforceAuthHandler.IntegrationType);

            if (salesforceSync != null)
            {
                var result = await salesforceSync.FullSyncAsync(ct);
                await LogSyncResultAsync(sp, result, ct);

                _logger.LogInformation(
                    "Salesforce sync completed: {Processed} processed, {Created} created, {Updated} updated, {Failed} failed",
                    result.RecordsProcessed,
                    result.RecordsCreated,
                    result.RecordsUpdated,
                    result.RecordsFailed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing with Salesforce");
        }
    }

    private async Task SyncDynamics365Async(IServiceProvider sp, CancellationToken ct)
    {
        _logger.LogInformation("Starting sync for Dynamics 365");

        try
        {
            var syncServices = sp.GetServices<ICrmSyncService>();
            var dynamicsSync = syncServices.FirstOrDefault(s => s.CrmType == Dynamics365AuthHandler.IntegrationType);

            if (dynamicsSync != null)
            {
                var result = await dynamicsSync.FullSyncAsync(ct);
                await LogSyncResultAsync(sp, result, ct);

                _logger.LogInformation(
                    "Dynamics 365 sync completed: {Processed} processed, {Created} created, {Updated} updated, {Failed} failed",
                    result.RecordsProcessed,
                    result.RecordsCreated,
                    result.RecordsUpdated,
                    result.RecordsFailed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing with Dynamics 365");
        }
    }

    private async Task SyncShopifyAsync(IServiceProvider sp, CancellationToken ct)
    {
        _logger.LogInformation("Starting sync for Shopify");

        try
        {
            var shopifySync = sp.GetService<IShopifySyncService>();

            if (shopifySync != null)
            {
                var summary = await shopifySync.FullSyncAsync(ct);

                _logger.LogInformation(
                    "Shopify sync completed: {Processed} processed, {Failed} failed, Success: {IsSuccess}",
                    summary.TotalRecordsProcessed,
                    summary.TotalRecordsFailed,
                    summary.IsSuccess);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing with Shopify");
        }
    }

    private async Task LogSyncResultAsync(IServiceProvider sp, SyncResult result, CancellationToken ct)
    {
        // Log to database if available
        try
        {
            var log = new CrmSyncLog
            {
                Id = Guid.NewGuid(),
                CrmType = result.CrmType,
                EntityType = result.EntityType,
                Direction = result.Direction.ToString(),
                RecordsProcessed = result.RecordsProcessed,
                RecordsSucceeded = result.RecordsCreated + result.RecordsUpdated,
                RecordsFailed = result.RecordsFailed,
                StartedAt = result.StartedAt,
                CompletedAt = result.CompletedAt,
                ErrorMessage = result.ErrorMessage,
                CreatedAt = DateTime.UtcNow
            };

            // Note: This would need to be saved to the database via repository
            _logger.LogDebug("Sync log created: {LogId}", log.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save sync log to database");
        }
    }

    private async Task<int> GetMinimumIntervalAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var settingsService = scope.ServiceProvider.GetRequiredService<IIntegrationSettingsService>();

            var enabledIntegrations = await settingsService.GetEnabledIntegrationsAsync(ct);
            var intervals = new List<int>();

            foreach (var integrationType in enabledIntegrations)
            {
                var integration = await settingsService.GetIntegrationAsync(integrationType, ct);
                if (integration != null)
                {
                    intervals.Add(integration.SyncIntervalMinutes);
                }
            }

            return intervals.Any() ? intervals.Min() : DefaultIntervalMinutes;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get sync intervals from database, using default");
            return DefaultIntervalMinutes;
        }
    }
}
