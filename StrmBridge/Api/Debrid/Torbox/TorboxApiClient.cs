using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using StrmBridge.Configuration.Providers;

namespace StrmBridge.Api.Debrid.Torbox;

/// <summary>
/// Torbox API client implementation
/// API Documentation: https://api.torbox.app/v1/api
/// </summary>
public class TorboxApiClient : IDebridApiClient
{
    private readonly TorboxSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TorboxApiClient> _logger;

    private const string ApiVersion = "v1";

    public TorboxApiClient(
        IOptions<TorboxSettings> settings,
        HttpClient httpClient,
        ILogger<TorboxApiClient> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;

        // Configure base address and auth
        _httpClient.BaseAddress = new Uri(_settings.ApiBaseUrl.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);
    }

    public string ProviderName => _settings.ProviderName;

    public bool IsEnabled => _settings.IsEnabled && !string.IsNullOrEmpty(_settings.ApiKey);

    public async Task<IReadOnlyList<DebridTorrent>> GetTorrentsAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching torrent list from Torbox API");

        try
        {
            var response = await _httpClient.GetFromJsonAsync<TorboxApiResponse<List<TorboxTorrentDto>>>(
                $"{ApiVersion}/api/torrents/mylist",
                ct);

            if (response?.Success != true || response.Data == null)
            {
                _logger.LogWarning("Torbox API returned error: {Error}", response?.Error ?? "Unknown error");
                return [];
            }

            _logger.LogInformation("Retrieved {Count} torrents from Torbox API", response.Data.Count);

            return response.Data
                .Select(MapToDebridTorrent)
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching torrents from Torbox API");
            throw;
        }
    }

    public async Task<string> GetDownloadLinkAsync(string torrentId, string fileId, CancellationToken ct = default)
    {
        _logger.LogDebug("Requesting download link for torrent {TorrentId}, file {FileId}", torrentId, fileId);

        try
        {
            var url = $"{ApiVersion}/api/torrents/requestdl?token={_settings.ApiKey}&torrent_id={torrentId}&file_id={fileId}";

            var response = await _httpClient.GetFromJsonAsync<TorboxApiResponse<string>>(url, ct);

            if (response?.Success != true || string.IsNullOrEmpty(response.Data))
            {
                throw new InvalidOperationException(
                    $"Failed to get download link: {response?.Error ?? "No URL returned"}");
            }

            return response.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error requesting download link from Torbox API");
            throw;
        }
    }

    public string GetPermalinkUrl(string torrentId, string fileId)
    {
        // Torbox supports permalinks with redirect=true
        // When accessed, this URL redirects to the CDN link
        return $"{_settings.ApiBaseUrl.TrimEnd('/')}/{ApiVersion}/api/torrents/requestdl" +
               $"?token={_settings.ApiKey}&torrent_id={torrentId}&file_id={fileId}&redirect=true";
    }

    /// <summary>
    /// Maps Torbox API DTO to our provider-agnostic model
    /// </summary>
    private static DebridTorrent MapToDebridTorrent(TorboxTorrentDto dto)
    {
        return new DebridTorrent
        {
            Id = dto.Id.ToString(),
            Hash = dto.Hash,
            Name = dto.Name,
            SizeBytes = dto.Size,
            CreatedAt = dto.CreatedAt,
            Status = MapStatus(dto.DownloadState),
            Progress = dto.Progress,
            Files = dto.Files
                .Select(f => new DebridFile
                {
                    Id = f.Id.ToString(),
                    Name = f.Name,
                    ShortName = !string.IsNullOrEmpty(f.ShortName) ? f.ShortName : Path.GetFileName(f.Name),
                    SizeBytes = f.Size,
                    MimeType = f.MimeType,
                    Md5Hash = f.Md5
                })
                .ToList()
        };
    }

    private static DebridTorrentStatus MapStatus(string? state)
    {
        return state?.ToLowerInvariant() switch
        {
            "downloading" => DebridTorrentStatus.Downloading,
            "uploading" => DebridTorrentStatus.Uploading,
            "paused" => DebridTorrentStatus.Paused,
            "completed" => DebridTorrentStatus.Completed,
            "cached" => DebridTorrentStatus.Cached,
            "stalled (no seeds)" => DebridTorrentStatus.Error,
            _ => DebridTorrentStatus.Unknown
        };
    }
}
