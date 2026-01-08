using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Algora.Erp.Admin.Services;

/// <summary>
/// Background service that periodically cleans up expired database backups
/// </summary>
public class BackupCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackupCleanupService> _logger;
    private readonly TimeSpan _interval;
    private readonly TimeSpan _runTime;

    public BackupCleanupService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<BackupCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;

        // Get cleanup interval from config (default: 24 hours)
        var intervalHours = _configuration.GetValue<int>("Backup:CleanupIntervalHours", 24);
        _interval = TimeSpan.FromHours(intervalHours);

        // Get preferred run time (default: 02:00 AM)
        var runTimeStr = _configuration.GetValue<string>("Backup:CleanupTime", "02:00");
        if (TimeSpan.TryParse(runTimeStr, out var runTime))
        {
            _runTime = runTime;
        }
        else
        {
            _runTime = TimeSpan.FromHours(2); // Default 2 AM
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Backup Cleanup Service started. Will run daily at {RunTime}", _runTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Calculate delay until next run time
                var delay = CalculateDelayUntilNextRun();
                _logger.LogDebug("Next backup cleanup scheduled in {Delay}", delay);

                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await RunCleanupAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Service is stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in backup cleanup service. Will retry in 1 hour.");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("Backup Cleanup Service stopped");
    }

    private TimeSpan CalculateDelayUntilNextRun()
    {
        var now = DateTime.Now;
        var nextRun = now.Date.Add(_runTime);

        // If the time has already passed today, schedule for tomorrow
        if (nextRun <= now)
        {
            nextRun = nextRun.AddDays(1);
        }

        return nextRun - now;
    }

    private async Task RunCleanupAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting scheduled backup cleanup at {Time}", DateTime.UtcNow);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();

            await backupService.CleanupExpiredBackupsAsync();

            _logger.LogInformation("Scheduled backup cleanup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run backup cleanup");
            throw;
        }
    }
}
