namespace StrmBridge.Sync;

/// <summary>
/// Result of a sync operation
/// </summary>
public record SyncResult
{
    public required string ProviderName { get; init; }
    public int ItemsScanned { get; init; }
    public int ItemsAdded { get; init; }
    public int ItemsUpdated { get; init; }
    public int ItemsMarkedMissing { get; init; }
    public int StrmFilesCreated { get; init; }
    public int StrmFilesRemoved { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Orchestrates the sync process
/// </summary>
public interface ISyncEngine
{
    /// <summary>
    /// Runs a full sync for all enabled providers
    /// </summary>
    Task<IReadOnlyList<SyncResult>> SyncAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Runs a sync for a specific provider
    /// </summary>
    Task<SyncResult> SyncProviderAsync(string providerName, CancellationToken ct = default);
}
