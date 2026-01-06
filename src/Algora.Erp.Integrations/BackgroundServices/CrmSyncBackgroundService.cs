using Algora.Erp.Integrations.Common.Interfaces;
using Algora.Erp.Integrations.Common.Models;
using Algora.Erp.Integrations.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Algora.Erp.Integrations.BackgroundServices;

public class CrmSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CrmSyncBackgroundService> _logger;
    private readonly CrmIntegrationsSettings _settings;

    public CrmSyncBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<CrmSyncBackgroundService> logger,
        IOptions<CrmIntegrationsSettings> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CRM Sync Background Service starting");

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
            var intervalMinutes = GetMinimumInterval();
            _logger.LogInformation("Next CRM sync in {Minutes} minutes", intervalMinutes);
            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }

        _logger.LogInformation("CRM Sync Background Service stopping");
    }

    private async Task RunSyncCycleAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var syncServices = scope.ServiceProvider.GetServices<ICrmSyncService>();

        foreach (var syncService in syncServices)
        {
            if (!IsCrmEnabled(syncService.CrmType))
            {
                continue;
            }

            _logger.LogInformation("Starting sync for {CrmType}", syncService.CrmType);

            try
            {
                var result = await syncService.FullSyncAsync(ct);
                await LogSyncResultAsync(scope.ServiceProvider, result, ct);

                _logger.LogInformation(
                    "{CrmType} sync completed: {Processed} processed, {Created} created, {Updated} updated, {Failed} failed",
                    syncService.CrmType,
                    result.RecordsProcessed,
                    result.RecordsCreated,
                    result.RecordsUpdated,
                    result.RecordsFailed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing with {CrmType}", syncService.CrmType);
            }
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

    private bool IsCrmEnabled(string crmType)
    {
        return crmType switch
        {
            "Salesforce" => _settings.Salesforce.Enabled,
            "Dynamics365" => _settings.Dynamics365.Enabled,
            _ => false
        };
    }

    private int GetMinimumInterval()
    {
        var intervals = new List<int>();

        if (_settings.Salesforce.Enabled)
            intervals.Add(_settings.Salesforce.SyncIntervalMinutes);

        if (_settings.Dynamics365.Enabled)
            intervals.Add(_settings.Dynamics365.SyncIntervalMinutes);

        return intervals.Any() ? intervals.Min() : 30;
    }
}
