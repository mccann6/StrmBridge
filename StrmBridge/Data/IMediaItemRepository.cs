using StrmBridge.Data.Entities;

namespace StrmBridge.Data;

/// <summary>
/// Repository for tracked media items
/// </summary>
public interface IMediaItemRepository
{
    /// <summary>
    /// Gets all tracked items for a provider
    /// </summary>
    Task<IReadOnlyList<TrackedMediaItem>> GetByProviderAsync(string providerName, CancellationToken ct = default);

    /// <summary>
    /// Gets a tracked item by its provider ID
    /// </summary>
    Task<TrackedMediaItem?> GetByProviderIdAsync(string providerId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new tracked item
    /// </summary>
    Task<TrackedMediaItem> AddAsync(TrackedMediaItem item, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing tracked item
    /// </summary>
    Task UpdateAsync(TrackedMediaItem item, CancellationToken ct = default);

    /// <summary>
    /// Marks items as missing if they weren't seen in the current sync.
    /// Returns the items that were marked missing (for cleanup).
    /// </summary>
    Task<IReadOnlyList<TrackedMediaItem>> MarkMissingAsync(string providerName, IEnumerable<string> seenProviderIds, CancellationToken ct = default);

    /// <summary>
    /// Gets all active tracked items for a provider as a dictionary keyed by ProviderId.
    /// Used for efficient batch comparison during sync.
    /// </summary>
    Task<Dictionary<string, TrackedMediaItem>> GetProviderItemsAsDictionaryAsync(string providerName, CancellationToken ct = default);

    /// <summary>
    /// Adds multiple items in a single transaction.
    /// </summary>
    Task AddRangeAsync(IEnumerable<TrackedMediaItem> items, CancellationToken ct = default);

    /// <summary>
    /// Saves all pending changes in a single transaction.
    /// </summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
