namespace StrmBridge.Api.Debrid;

/// <summary>
/// Represents a torrent/download from a debrid provider's library.
/// This is provider-agnostic - each API client maps their response to this.
/// </summary>
public record DebridTorrent
{
    /// <summary>
    /// Provider's unique ID for this torrent
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Info hash of the torrent
    /// </summary>
    public string? Hash { get; init; }

    /// <summary>
    /// Torrent/download name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Total size in bytes
    /// </summary>
    public long? SizeBytes { get; init; }

    /// <summary>
    /// When this was added to the provider
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    /// Current download state
    /// </summary>
    public DebridTorrentStatus Status { get; init; } = DebridTorrentStatus.Unknown;

    /// <summary>
    /// Download progress (0.0 to 1.0)
    /// </summary>
    public double? Progress { get; init; }

    /// <summary>
    /// Files within this torrent
    /// </summary>
    public IReadOnlyList<DebridFile> Files { get; init; } = [];
}

/// <summary>
/// Status of a torrent in the debrid provider
/// </summary>
public enum DebridTorrentStatus
{
    Unknown,
    Downloading,
    Uploading,
    Paused,
    Completed,
    Cached,
    Error
}
