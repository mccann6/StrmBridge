namespace StrmBridge.Sync.Naming;

/// <summary>
/// Determines the output path for media items with proper naming conventions
/// </summary>
public interface IMediaNamingStrategy
{
    /// <summary>
    /// Gets the clean, organized path for a media item based on torrent name.
    /// Used for fallback/folder detection.
    /// </summary>
    /// <param name="originalName">The original messy torrent name</param>
    /// <param name="providerName">The provider name (for multi-provider scenarios)</param>
    /// <returns>Relative path like "TV Shows/Show Name/Season 01/..." or "Movies/Movie Name (2024)/..."</returns>
    string GetOutputPath(string originalName, string providerName);

    /// <summary>
    /// Gets the full file path for a specific media file, including the .strm filename.
    /// This analyzes the actual filename for episode info (useful for season packs).
    /// </summary>
    /// <param name="torrentName">The parent torrent name</param>
    /// <param name="fileName">The actual video filename</param>
    /// <returns>Full relative path including filename, e.g., "TV Shows/Show/Season 01/Show - S01E01.strm"</returns>
    string GetFilePath(string torrentName, string fileName);
}
