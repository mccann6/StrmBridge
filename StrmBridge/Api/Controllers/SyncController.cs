using Microsoft.AspNetCore.Mvc;
using StrmBridge.Sync;

namespace StrmBridge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly ISyncEngine _syncEngine;
    private readonly ILogger<SyncController> _logger;

    public SyncController(
        ISyncEngine syncEngine,
        ILogger<SyncController> logger)
    {
        _syncEngine = syncEngine;
        _logger = logger;
    }

    /// <summary>
    /// Triggers a manual sync for all enabled providers
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<IReadOnlyList<SyncResult>>> SyncAll(CancellationToken ct)
    {
        _logger.LogInformation("Manual sync triggered via API");
        var results = await _syncEngine.SyncAllAsync(ct);
        return Ok(results);
    }

    /// <summary>
    /// Triggers a manual sync for a specific provider
    /// </summary>
    [HttpPost("{providerName}")]
    public async Task<ActionResult<SyncResult>> SyncProvider(
        string providerName, 
        CancellationToken ct)
    {
        _logger.LogInformation("Manual sync triggered for provider: {Provider}", providerName);
        var result = await _syncEngine.SyncProviderAsync(providerName, ct);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}
