namespace StrmBridge.Api.Debrid;

/// <summary>
/// Represents a single file within a debrid torrent/download
/// </summary>
public record DebridFile
{
    /// <summary>
    /// Provider's unique ID for this file
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Full path/name of the file within the torrent
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Just the filename (without path)
    /// </summary>
    public string ShortName { get; init; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long SizeBytes { get; init; }

    /// <summary>
    /// MIME type (if known)
    /// </summary>
    public string? MimeType { get; init; }

    /// <summary>
    /// MD5 hash (if provided)
    /// </summary>
    public string? Md5Hash { get; init; }

    /// <summary>
    /// Whether this is a streamable video file
    /// </summary>
    public bool IsVideo => IsVideoFile(ShortName);

    /// <summary>
    /// Whether this file should be included in sync
    /// (excludes samples, small files, non-video)
    /// </summary>
    public bool ShouldSync => IsVideo && !IsSample && SizeBytes > MinVideoSizeBytes;

    /// <summary>
    /// Whether this appears to be a sample file
    /// </summary>
    public bool IsSample =>
        ShortName.Contains("sample", StringComparison.OrdinalIgnoreCase) ||
        Name.Contains("/sample/", StringComparison.OrdinalIgnoreCase);

    // Minimum size to consider a video file worth syncing (50MB)
    private const long MinVideoSizeBytes = 50 * 1024 * 1024;

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mkv", ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm",
        ".m4v", ".mpg", ".mpeg", ".ts", ".m2ts", ".vob"
    };

    private static bool IsVideoFile(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return VideoExtensions.Contains(ext);
    }
}
