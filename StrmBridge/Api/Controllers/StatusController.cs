using Microsoft.AspNetCore.Mvc;
using StrmBridge.Api.Models;
using StrmBridge.Configuration.Interfaces;
using StrmBridge.Providers;

namespace StrmBridge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    private readonly IAppSettings _appSettings;
    private readonly IEnumerable<IDebridProvider> _providers;
    private readonly ILogger<StatusController> _logger;

    public StatusController(
        IAppSettings appSettings,
        IEnumerable<IDebridProvider> providers,
        ILogger<StatusController> logger)
    {
        _appSettings = appSettings;
        _providers = providers;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current status of the application and all providers
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<StatusResponse>> GetStatus(CancellationToken ct)
    {
        _logger.LogInformation("Status check requested");

        var providerStatuses = new List<ProviderStatusInfo>();

        foreach (var provider in _providers)
        {
            int? itemCount = null;
            string? error = null;
            bool isHealthy = false;

            if (provider.IsEnabled)
            {
                try
                {
                    var items = await provider.GetLibraryAsync(ct);
                    itemCount = items.Count;
                    isHealthy = true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get library for {Provider}", provider.ProviderName);
                    error = ex.Message;
                }
            }

            providerStatuses.Add(new ProviderStatusInfo
            {
                Name = provider.ProviderName,
                IsEnabled = provider.IsEnabled,
                IsHealthy = isHealthy,
                Error = error,
                ItemCount = itemCount
            });
        }

        var allHealthy = providerStatuses.All(p => !p.IsEnabled || p.IsHealthy);

        var response = new StatusResponse
        {
            Status = allHealthy ? "Healthy" : "Degraded",
            Timestamp = DateTimeOffset.UtcNow,
            App = new AppStatusInfo
            {
                MediaOutputPath = _appSettings.MediaOutputPath,
                SyncIntervalSeconds = _appSettings.SyncIntervalSeconds,
                DatabasePath = _appSettings.DatabasePath
            },
            Providers = providerStatuses
        };

        return Ok(response);
    }
}
