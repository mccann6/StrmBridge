namespace StrmBridge.Sync;

/// <summary>
/// Manages creation and removal of .strm files for media streaming.
/// .strm files are simple text files containing a URL that media players can stream from.
/// </summary>
public interface IStrmFileManager
{
    /// <summary>
    /// Creates a .strm file with the given streaming URL
    /// </summary>
    /// <param name="strmPath">Path where the .strm file should be created</param>
    /// <param name="streamingUrl">URL to write into the .strm file</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if created/updated, false if already exists with same URL</returns>
    Task<bool> CreateStrmFileAsync(string strmPath, string streamingUrl, CancellationToken ct = default);

    /// <summary>
    /// Removes a .strm file if it exists
    /// </summary>
    Task<bool> RemoveStrmFileAsync(string strmPath, CancellationToken ct = default);

    /// <summary>
    /// Checks if a .strm file exists
    /// </summary>
    Task<bool> StrmFileExistsAsync(string strmPath, CancellationToken ct = default);

    /// <summary>
    /// Gets the URL from an existing .strm file
    /// </summary>
    Task<string?> GetStrmUrlAsync(string strmPath, CancellationToken ct = default);

    Task<bool> MoveStrmFileAsync(string oldPath, string newPath, CancellationToken ct = default);
}
