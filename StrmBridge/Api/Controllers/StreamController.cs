using Microsoft.AspNetCore.Mvc;
using StrmBridge.Api.Debrid;

namespace StrmBridge.Api.Controllers;

/// <summary>
/// Streaming redirect controller.
/// Provides URLs for .strm files that redirect to the actual CDN links.
/// This keeps the API key out of the .strm files.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StreamController : ControllerBase
{
    private readonly IEnumerable<IDebridApiClient> _apiClients;
    private readonly ILogger<StreamController> _logger;

    public StreamController(
        IEnumerable<IDebridApiClient> apiClients,
        ILogger<StreamController> logger)
    {
        _apiClients = apiClients;
        _logger = logger;
    }

    /// <summary>
    /// Redirects to the actual CDN streaming URL for a file.
    /// Called by media players when opening .strm files.
    /// API key stays server-side - only the CDN URL is exposed.
    /// </summary>
    /// <param name="provider">Provider name (e.g., "torbox")</param>
    /// <param name="torrentId">Torrent ID</param>
    /// <param name="fileId">File ID within the torrent</param>
    [HttpGet("{provider}/{torrentId}/{fileId}")]
    public async Task<IActionResult> GetStreamRedirect(
        string provider,
        string torrentId,
        string fileId,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Stream redirect requested: {Provider}/{TorrentId}/{FileId}",
            provider, torrentId, fileId);

        // Find the matching API client
        var client = _apiClients.FirstOrDefault(c =>
            c.ProviderName.Equals(provider, StringComparison.OrdinalIgnoreCase));

        if (client == null)
        {
            _logger.LogWarning("Provider not found: {Provider}", provider);
            return NotFound($"Provider not found: {provider}");
        }

        if (!client.IsEnabled)
        {
            _logger.LogWarning("Provider not enabled: {Provider}", provider);
            return BadRequest($"Provider not enabled: {provider}");
        }

        try
        {
            // Call the API to get the actual CDN URL (API key stays server-side)
            var cdnUrl = await client.GetDownloadLinkAsync(torrentId, fileId, ct);

            _logger.LogDebug("Redirecting to CDN: {Url}", cdnUrl);

            // 302 redirect to CDN - API key never leaves the server
            return Redirect(cdnUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stream URL for {Provider}/{TorrentId}/{FileId}",
                provider, torrentId, fileId);
            return StatusCode(500, "Failed to get streaming URL");
        }
    }
}
