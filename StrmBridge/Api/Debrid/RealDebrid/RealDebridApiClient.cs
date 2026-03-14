using Microsoft.Extensions.Options;
using StrmBridge.Configuration.Providers;

namespace StrmBridge.Api.Debrid.RealDebrid;

/// <summary>
/// Real-Debrid API client implementation
/// API Documentation: https://api.real-debrid.com/
/// </summary>
public class RealDebridApiClient : IDebridApiClient
{
    private readonly RealDebridSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<RealDebridApiClient> _logger;

    private const string ApiVersion = "rest/1.0";

    public RealDebridApiClient(
        IOptions<RealDebridSettings> settings,
        HttpClient httpClient,
        ILogger<RealDebridApiClient> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_settings.ApiBaseUrl.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);
    }

    public string ProviderName => _settings.ProviderName;

    public bool IsEnabled => _settings.IsEnabled && !string.IsNullOrEmpty(_settings.ApiKey);

    public async Task<IReadOnlyList<DebridTorrent>> GetTorrentsAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching torrent list from Real-Debrid API");

        try
        {
            var torrents = await _httpClient.GetFromJsonAsync<List<RealDebridTorrentDto>>(
                $"{ApiVersion}/torrents",
                ct) ?? [];

            _logger.LogInformation("Retrieved {Count} torrents from Real-Debrid API", torrents.Count);

            var result = new List<DebridTorrent>();

            foreach (var torrent in torrents)
            {
                if (!string.Equals(torrent.Status, "downloaded", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(MapToDebridTorrent(torrent, []));
                    continue;
                }

                try
                {
                    var info = await _httpClient.GetFromJsonAsync<RealDebridTorrentInfoDto>(
                        $"{ApiVersion}/torrents/info/{torrent.Id}",
                        ct);

                    if (info != null)
                    {
                        result.Add(MapToDebridTorrent(torrent, info.Files, info.Links));
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "Failed to get info for torrent {Id}, skipping", torrent.Id);
                    result.Add(MapToDebridTorrent(torrent, []));
                }
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching torrents from Real-Debrid API");
            throw;
        }
    }

    public async Task<string> GetDownloadLinkAsync(string torrentId, string fileId, CancellationToken ct = default)
    {
        _logger.LogDebug("Requesting download link for torrent {TorrentId}, file {FileId}", torrentId, fileId);

        try
        {
            var info = await _httpClient.GetFromJsonAsync<RealDebridTorrentInfoDto>(
                $"{ApiVersion}/torrents/info/{torrentId}",
                ct);

            if (info == null || info.Links.Count == 0)
            {
                throw new InvalidOperationException($"No links available for torrent {torrentId}");
            }

            var fileIndex = int.Parse(fileId);
            var selectedFiles = info.Files.Where(f => f.Selected == 1).ToList();
            var linkIndex = selectedFiles.FindIndex(f => f.Id == fileIndex);

            if (linkIndex < 0 || linkIndex >= info.Links.Count)
            {
                throw new InvalidOperationException($"File {fileId} not found in torrent {torrentId}");
            }

            var link = info.Links[linkIndex];

            var content = new FormUrlEncodedContent([new KeyValuePair<string, string>("link", link)]);
            var response = await _httpClient.PostAsync($"{ApiVersion}/unrestrict/link", content, ct);
            response.EnsureSuccessStatusCode();

            var unrestricted = await response.Content.ReadFromJsonAsync<RealDebridUnrestrictDto>(ct);

            if (unrestricted == null || string.IsNullOrEmpty(unrestricted.Download))
            {
                throw new InvalidOperationException("Failed to unrestrict link");
            }

            return unrestricted.Download;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error requesting download link from Real-Debrid API");
            throw;
        }
    }

    public string GetPermalinkUrl(string torrentId, string fileId)
    {
        // Real-Debrid doesn't support direct permalinks
        // We use our internal redirect endpoint instead
        throw new NotSupportedException(
            "Real-Debrid doesn't support permalinks. Use the internal stream redirect endpoint.");
    }

    private static DebridTorrent MapToDebridTorrent(
        RealDebridTorrentDto dto,
        List<RealDebridFileDto> files,
        List<string>? links = null)
    {
        return new DebridTorrent
        {
            Id = dto.Id,
            Hash = dto.Hash,
            Name = dto.Filename,
            SizeBytes = dto.Bytes,
            CreatedAt = dto.Added.HasValue ? new DateTimeOffset(dto.Added.Value, TimeSpan.Zero) : null,
            Status = MapStatus(dto.Status),
            Progress = dto.Progress / 100.0, // RD uses 0-100, we use 0-1
            Files = (files ?? [])
                .Where(f => f.Selected == 1) // Only include selected files
                .Select((f, index) => new DebridFile
                {
                    Id = f.Id.ToString(),
                    Name = f.Path,
                    ShortName = Path.GetFileName(f.Path.TrimStart('/')),
                    SizeBytes = f.Bytes
                })
                .ToList()
        };
    }

    private static DebridTorrentStatus MapStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "downloaded" => DebridTorrentStatus.Completed,
            "downloading" => DebridTorrentStatus.Downloading,
            "magnet_conversion" => DebridTorrentStatus.Downloading,
            "waiting_files_selection" => DebridTorrentStatus.Paused,
            "queued" => DebridTorrentStatus.Paused,
            "uploading" => DebridTorrentStatus.Uploading,
            "compressing" => DebridTorrentStatus.Uploading,
            "dead" => DebridTorrentStatus.Error,
            "error" => DebridTorrentStatus.Error,
            "virus" => DebridTorrentStatus.Error,
            _ => DebridTorrentStatus.Unknown
        };
    }
}
