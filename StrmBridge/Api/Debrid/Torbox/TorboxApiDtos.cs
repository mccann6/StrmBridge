using System.Text.Json.Serialization;

namespace StrmBridge.Api.Debrid.Torbox;

/// <summary>
/// JSON response DTOs for Torbox API
/// These map the raw JSON responses to C# objects
/// </summary>
/// 
/// <summary>
/// Base response wrapper for all Torbox API responses
/// </summary>
/// <typeparam name="T">The type of data in the response</typeparam>
public record TorboxApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    [JsonPropertyName("data")]
    public T? Data { get; init; }
}

/// <summary>
/// Torrent item from /api/torrents/mylist
/// </summary>
public record TorboxTorrentDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("hash")]
    public string? Hash { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; init; }

    [JsonPropertyName("download_state")]
    public string? DownloadState { get; init; }

    [JsonPropertyName("progress")]
    public double Progress { get; init; }

    [JsonPropertyName("download_present")]
    public bool DownloadPresent { get; init; }

    [JsonPropertyName("download_finished")]
    public bool DownloadFinished { get; init; }

    [JsonPropertyName("active")]
    public bool Active { get; init; }

    [JsonPropertyName("expires_at")]
    public DateTimeOffset? ExpiresAt { get; init; }

    [JsonPropertyName("files")]
    public List<TorboxFileDto> Files { get; init; } = [];
}

/// <summary>
/// File within a torrent from /api/torrents/mylist
/// </summary>
public record TorboxFileDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("short_name")]
    public string ShortName { get; init; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; init; }

    [JsonPropertyName("mimetype")]
    public string? MimeType { get; init; }

    [JsonPropertyName("md5")]
    public string? Md5 { get; init; }

    [JsonPropertyName("s3_path")]
    public string? S3Path { get; init; }
}
