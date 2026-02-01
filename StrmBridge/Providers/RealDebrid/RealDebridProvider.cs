using System.Text.RegularExpressions;
using StrmBridge.Api.Debrid;
using StrmBridge.Api.Debrid.RealDebrid;
using StrmBridge.Configuration.Interfaces;
using StrmBridge.Providers.Abstractions;

namespace StrmBridge.Providers.RealDebrid;

/// <summary>
/// Real-Debrid provider that fetches media library via API
/// </summary>
public partial class RealDebridProvider : IDebridProvider
{
    private readonly RealDebridApiClient _apiClient;
    private readonly IAppSettings _appSettings;
    private readonly ILogger<RealDebridProvider> _logger;

    public RealDebridProvider(
        RealDebridApiClient apiClient,
        IAppSettings appSettings,
        ILogger<RealDebridProvider> logger)
    {
        _apiClient = apiClient;
        _appSettings = appSettings;
        _logger = logger;
    }

    public string ProviderName => _apiClient.ProviderName;

    public bool IsEnabled => _apiClient.IsEnabled;

    public async Task<IReadOnlyList<MediaItem>> GetLibraryAsync(CancellationToken ct = default)
    {
        if (!IsEnabled)
        {
            _logger.LogWarning("Real-Debrid provider is not enabled or configured");
            return [];
        }

        _logger.LogInformation("Fetching library from Real-Debrid API");

        try
        {
            var torrents = await _apiClient.GetTorrentsAsync(ct);
            var items = new List<MediaItem>();

            foreach (var torrent in torrents)
            {
                // Only process completed torrents
                if (torrent.Status != DebridTorrentStatus.Completed)
                {
                    _logger.LogDebug(
                        "Skipping torrent {Name} - status is {Status}",
                        torrent.Name,
                        torrent.Status);
                    continue;
                }

                // Get streamable files from this torrent
                var streamableFiles = torrent.Files.Where(f => f.ShouldSync).ToList();

                if (streamableFiles.Count == 0)
                {
                    _logger.LogDebug("No streamable files in torrent: {Name}", torrent.Name);
                    continue;
                }

                foreach (var file in streamableFiles)
                {
                    var mediaItem = new MediaItem
                    {
                        ProviderId = $"{ProviderName}:{torrent.Id}:{file.Id}",
                        TorrentId = torrent.Id,
                        FileId = file.Id,
                        TorrentName = torrent.Name,
                        FileName = file.ShortName,
                        StreamingUrl = GetInternalStreamUrl(torrent.Id, file.Id),
                        SizeBytes = file.SizeBytes,
                        CreatedAt = torrent.CreatedAt,
                        Type = DetectMediaType(torrent.Name, file.ShortName)
                    };

                    items.Add(mediaItem);
                }
            }

            _logger.LogInformation(
                "Found {Count} streamable files from {TorrentCount} torrents",
                items.Count,
                torrents.Count);

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching library from Real-Debrid API");
            throw;
        }
    }

    private string GetInternalStreamUrl(string torrentId, string fileId)
    {
        var baseUrl = _appSettings.ServiceBaseUrl.TrimEnd('/');
        return $"{baseUrl}/api/stream/{ProviderName.ToLowerInvariant()}/{torrentId}/{fileId}";
    }

    private static MediaType DetectMediaType(string torrentName, string fileName)
    {
        var combinedName = $"{torrentName} {fileName}".ToLowerInvariant();

        if (TvShowPattern().IsMatch(combinedName))
        {
            return MediaType.TvShow;
        }

        if (MoviePattern().IsMatch(combinedName))
        {
            return MediaType.Movie;
        }

        return MediaType.Unknown;
    }

    [GeneratedRegex(@"s\d{1,2}e\d{1,2}|season\s*\d|episode\s*\d", RegexOptions.IgnoreCase)]
    private static partial Regex TvShowPattern();

    [GeneratedRegex(@"[\(\[]?(19|20)\d{2}[\)\]]?")]
    private static partial Regex MoviePattern();
}
