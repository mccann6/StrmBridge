namespace StrmBridge.Providers.Abstractions;

/// <summary>
/// Represents a streamable media file from a debrid provider's library.
/// Each MediaItem corresponds to a single video file that will become a .strm file.
/// </summary>
public record MediaItem
{
    /// <summary>
    /// Unique ID for this item combining provider + torrent + file
    /// Format: "{ProviderName}:{TorrentId}:{FileId}"
    /// </summary>
    public required string ProviderId { get; init; }

    /// <summary>
    /// Parent torrent's ID in the provider's system
    /// </summary>
    public required string TorrentId { get; init; }

    /// <summary>
    /// File's ID within the torrent
    /// </summary>
    public required string FileId { get; init; }

    /// <summary>
    /// Torrent name from the provider (often messy torrent names)
    /// </summary>
    public required string TorrentName { get; init; }

    /// <summary>
    /// The video file's name (e.g., "Movie.2024.1080p.mkv")
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Streaming URL for this file (permalink that redirects to CDN)
    /// </summary>
    public required string StreamingUrl { get; init; }

    /// <summary>
    /// Size in bytes
    /// </summary>
    public long SizeBytes { get; init; }

    /// <summary>
    /// When this item was added to the provider's library
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    /// Type of media (if detectable)
    /// </summary>
    public MediaType Type { get; init; } = MediaType.Unknown;
}

public enum MediaType
{
    Unknown,
    Movie,
    TvShow,
    Music,
    Other
}
