using StrmBridge.Configuration.Interfaces;

namespace StrmBridge.Sync;

/// <summary>
/// Background service that periodically syncs media from all enabled providers.
/// Interval is configurable via App__SyncIntervalSeconds (default: 300 seconds / 5 minutes).
/// Set to 0 to disable automatic sync.
/// </summary>
public class SyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAppSettings _appSettings;
    private readonly ILogger<SyncBackgroundService> _logger;

    public SyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        IAppSettings appSettings,
        ILogger<SyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _appSettings = appSettings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = _appSettings.SyncIntervalSeconds;

        if (intervalSeconds <= 0)
        {
            _logger.LogInformation("Background sync is disabled (SyncIntervalSeconds = {Interval})", intervalSeconds);
            return;
        }

        var interval = TimeSpan.FromSeconds(intervalSeconds);
        _logger.LogInformation("Background sync service started. Interval: {Interval}", interval);

        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunSyncAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during background sync");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Background sync service stopped");
    }

    private async Task RunSyncAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting scheduled sync...");

        using var scope = _scopeFactory.CreateScope();
        var syncEngine = scope.ServiceProvider.GetRequiredService<ISyncEngine>();

        var results = await syncEngine.SyncAllAsync(ct);

        foreach (var result in results)
        {
            if (result.Success)
            {
                _logger.LogInformation(
                    "Sync completed for {Provider}: {Added} added, {Updated} updated, {Missing} missing, {Strm} .strm files created in {Duration:N1}s",
                    result.ProviderName,
                    result.ItemsAdded,
                    result.ItemsUpdated,
                    result.ItemsMarkedMissing,
                    result.StrmFilesCreated,
                    result.Duration.TotalSeconds);
            }
            else
            {
                _logger.LogWarning(
                    "Sync failed for {Provider}: {Error}",
                    result.ProviderName,
                    result.Error);
            }
        }
    }
}
