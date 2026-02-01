using StrmBridge.Providers.Abstractions;

namespace StrmBridge.Providers;

/// <summary>
/// Abstraction for any debrid provider (Torbox, RealDebrid, etc.)
/// Providers scan their library via API and return streamable media items.
/// </summary>
public interface IDebridProvider
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
    /// Fetches all streamable media items from the provider's library.
    /// Each item contains a streaming URL ready for .strm file creation.
    /// </summary>
    Task<IReadOnlyList<MediaItem>> GetLibraryAsync(CancellationToken ct = default);
}
