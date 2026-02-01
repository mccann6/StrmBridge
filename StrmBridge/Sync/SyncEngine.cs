using System.Diagnostics;
using StrmBridge.Configuration.Interfaces;
using StrmBridge.Data;
using StrmBridge.Data.Entities;
using StrmBridge.Providers;
using StrmBridge.Sync.Naming;

namespace StrmBridge.Sync;

/// <summary>
/// Sync engine that creates .strm files for streaming media items
/// </summary>
public class SyncEngine : ISyncEngine
{
    private readonly IEnumerable<IDebridProvider> _providers;
    private readonly IMediaItemRepository _repository;
    private readonly IStrmFileManager _strmFileManager;
    private readonly IMediaNamingStrategy _namingStrategy;
    private readonly IAppSettings _appSettings;
    private readonly ILogger<SyncEngine> _logger;

    public SyncEngine(
        IEnumerable<IDebridProvider> providers,
        IMediaItemRepository repository,
        IStrmFileManager strmFileManager,
        IMediaNamingStrategy namingStrategy,
        IAppSettings appSettings,
        ILogger<SyncEngine> logger)
    {
        _providers = providers;
        _repository = repository;
        _strmFileManager = strmFileManager;
        _namingStrategy = namingStrategy;
        _appSettings = appSettings;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SyncResult>> SyncAllAsync(CancellationToken ct = default)
    {
        var results = new List<SyncResult>();

        foreach (var provider in _providers.Where(p => p.IsEnabled))
        {
            var result = await SyncProviderAsync(provider.ProviderName, ct);
            results.Add(result);
        }

        return results;
    }

    public async Task<SyncResult> SyncProviderAsync(string providerName, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var provider = _providers.FirstOrDefault(p => p.ProviderName == providerName);
        if (provider == null)
        {
            return new SyncResult
            {
                ProviderName = providerName,
                Success = false,
                Error = $"Provider not found: {providerName}",
                Duration = stopwatch.Elapsed
            };
        }

        _logger.LogInformation("Starting sync for provider: {Provider}", providerName);

        try
        {
            // Get items from provider API
            var providerItems = await provider.GetLibraryAsync(ct);
            _logger.LogInformation(
                "Found {Count} streamable items from {Provider}",
                providerItems.Count,
                providerName);

            var added = 0;
            var updated = 0;
            var strmFilesCreated = 0;
            var seenProviderIds = new List<string>();

            foreach (var item in providerItems)
            {
                ct.ThrowIfCancellationRequested();
                seenProviderIds.Add(item.ProviderId);

            var existing = await _repository.GetByProviderIdAsync(item.ProviderId, ct);

            if (existing == null)
            {
                    var strmPath = GetStrmPath(item.TorrentName, item.FileName, providerName);

                    var created = await _strmFileManager.CreateStrmFileAsync(
                        strmPath,
                        item.StreamingUrl,
                        ct);

                    if (created)
                    {
                        strmFilesCreated++;
                    }

                    var trackedItem = new TrackedMediaItem
                    {
                        ProviderId = item.ProviderId,
                        ProviderName = providerName,
                        TorrentId = item.TorrentId,
                        FileId = item.FileId,
                        TorrentName = item.TorrentName,
                        FileName = item.FileName,
                        StrmPath = strmPath,
                        StreamingUrl = item.StreamingUrl,
                        SizeBytes = item.SizeBytes
                    };

                    await _repository.AddAsync(trackedItem, ct);
                    added++;

                    _logger.LogDebug(
                        "Added new item: {Name} -> {StrmPath}",
                        item.FileName,
                        strmPath);
                }
                else
                {
                    // Existing item - update last seen time
                    existing.LastSeenAt = DateTimeOffset.UtcNow;

                    // If it was marked missing, restore it
                    if (existing.Status == MediaItemStatus.Missing)
                    {
                        existing.Status = MediaItemStatus.Active;
                        updated++;
                    }

                    // Update streaming URL if changed (API key rotation, etc.)
                    if (existing.StreamingUrl != item.StreamingUrl && !string.IsNullOrEmpty(existing.StrmPath))
                    {
                        await _strmFileManager.CreateStrmFileAsync(
                            existing.StrmPath,
                            item.StreamingUrl,
                            ct);
                        existing.StreamingUrl = item.StreamingUrl;
                        updated++;
                    }

                    await _repository.UpdateAsync(existing, ct);
                }
            }

            // Mark items that weren't seen as missing and delete their .strm files
            var missingItems = await _repository.MarkMissingAsync(providerName, seenProviderIds, ct);
            var strmFilesRemoved = 0;
            foreach (var missing in missingItems)
            {
                if (!string.IsNullOrEmpty(missing.StrmPath))
                {
                    try
                    {
                        await _strmFileManager.RemoveStrmFileAsync(missing.StrmPath, ct);
                        strmFilesRemoved++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to remove .strm file for missing item: {Path}", missing.StrmPath);
                    }
                }
            }

            if (missingItems.Count > 0)
            {
                _logger.LogInformation(
                    "Marked {Count} items as missing, removed {Removed} .strm files",
                    missingItems.Count,
                    strmFilesRemoved);
            }

            stopwatch.Stop();

            var result = new SyncResult
            {
                ProviderName = providerName,
                ItemsScanned = providerItems.Count,
                ItemsAdded = added,
                ItemsUpdated = updated,
                ItemsMarkedMissing = missingItems.Count,
                StrmFilesCreated = strmFilesCreated,
                StrmFilesRemoved = strmFilesRemoved,
                Success = true,
                Duration = stopwatch.Elapsed
            };

            _logger.LogInformation(
                "Sync completed for {Provider}: Scanned={Scanned}, Added={Added}, StrmFiles={StrmFiles}, Duration={Duration}ms",
                providerName,
                result.ItemsScanned,
                result.ItemsAdded,
                result.StrmFilesCreated,
                result.Duration.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync failed for provider: {Provider}", providerName);
            return new SyncResult
            {
                ProviderName = providerName,
                Success = false,
                Error = ex.Message,
                Duration = stopwatch.Elapsed
            };
        }
    }

    /// <summary>
    /// Generates the .strm file path for a media item
    /// </summary>
    private string GetStrmPath(string torrentName, string fileName, string providerName)
    {
        var relativePath = _namingStrategy.GetFilePath(torrentName, fileName);

        return Path.Combine(_appSettings.MediaOutputPath, relativePath);
    }
}
