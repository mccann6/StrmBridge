namespace StrmBridge.Data.Entities;

/// <summary>
/// Represents a media item tracked in the database
/// </summary>
public class TrackedMediaItem
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique identifier: "{ProviderName}:{TorrentId}:{FileId}"
    /// </summary>
    public required string ProviderId { get; set; }

    /// <summary>
    /// Which provider this item came from (e.g., "Torbox", "RealDebrid")
    /// </summary>
    public required string ProviderName { get; set; }

    /// <summary>
    /// Torrent ID in the provider's system
    /// </summary>
    public required string TorrentId { get; set; }

    /// <summary>
    /// File ID within the torrent
    /// </summary>
    public required string FileId { get; set; }

    /// <summary>
    /// Original torrent name from the provider
    /// </summary>
    public required string TorrentName { get; set; }

    /// <summary>
    /// Video filename
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// Path to the .strm file (where Jellyfin sees it)
    /// </summary>
    public string? StrmPath { get; set; }

    /// <summary>
    /// Streaming URL stored in the .strm file
    /// </summary>
    public string? StreamingUrl { get; set; }

    /// <summary>
    /// When this item was first discovered
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Current status of the item
    /// </summary>
    public MediaItemStatus Status { get; set; } = MediaItemStatus.Active;

    /// <summary>
    /// Size in bytes
    /// </summary>
    public long SizeBytes { get; set; }
}

public enum MediaItemStatus
{
    /// <summary>
    /// Item is active and .strm file exists
    /// </summary>
    Active,

    /// <summary>
    /// Item was not found in last sync (may be temporary)
    /// </summary>
    Missing,

    /// <summary>
    /// Item has been unavailable for extended period
    /// </summary>
    Unavailable,

    /// <summary>
    /// Item was manually removed by user
    /// </summary>
    Removed
}
