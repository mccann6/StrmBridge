namespace StrmBridge.Api.Debrid;

/// <summary>
/// Abstract interface for debrid provider API operations.
/// Each provider (Torbox, RealDebrid, AllDebrid, etc.) implements this interface.
/// </summary>
public interface IDebridApiClient
{
    /// <summary>
    /// Unique name for this provider
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Whether this provider is enabled and configured
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets all torrents/downloads from the provider's library
    /// </summary>
    Task<IReadOnlyList<DebridTorrent>> GetTorrentsAsync(CancellationToken ct = default);

    /// <summary>
    /// Requests a direct download/streaming link for a specific file.
    /// Some providers return time-limited CDN URLs.
    /// </summary>
    /// <param name="torrentId">The torrent's ID</param>
    /// <param name="fileId">The specific file's ID within the torrent</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A direct download URL (may be time-limited)</returns>
    Task<string> GetDownloadLinkAsync(string torrentId, string fileId, CancellationToken ct = default);

    /// <summary>
    /// Gets a permalink URL that can be used for .strm files.
    /// This URL should redirect to the actual CDN link when accessed.
    /// </summary>
    /// <param name="torrentId">The torrent's ID</param>
    /// <param name="fileId">The specific file's ID within the torrent</param>
    /// <returns>A stable permalink URL suitable for .strm files</returns>
    string GetPermalinkUrl(string torrentId, string fileId);
}
