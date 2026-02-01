using System.Text.Json.Serialization;

namespace StrmBridge.Api.Debrid.RealDebrid;

/// <summary>
/// JSON response DTOs for Real-Debrid API
/// API Documentation: https://api.real-debrid.com/
/// </summary>
/// 
/// <summary>
/// Torrent item from GET /torrents
/// </summary>
public record RealDebridTorrentDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("filename")]
    public string Filename { get; init; } = string.Empty;

    [JsonPropertyName("hash")]
    public string? Hash { get; init; }

    [JsonPropertyName("bytes")]
    public long Bytes { get; init; }

    [JsonPropertyName("host")]
    public string? Host { get; init; }

    [JsonPropertyName("split")]
    public int Split { get; init; }

    [JsonPropertyName("progress")]
    public double Progress { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("added")]
    public DateTime? Added { get; init; }

    [JsonPropertyName("links")]
    public List<string> Links { get; init; } = [];

    [JsonPropertyName("ended")]
    public DateTime? Ended { get; init; }

    [JsonPropertyName("speed")]
    public long? Speed { get; init; }

    [JsonPropertyName("seeders")]
    public int? Seeders { get; init; }
}

/// <summary>
/// Detailed torrent info from GET /torrents/info/{id}
/// </summary>
public record RealDebridTorrentInfoDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("filename")]
    public string Filename { get; init; } = string.Empty;

    [JsonPropertyName("original_filename")]
    public string? OriginalFilename { get; init; }

    [JsonPropertyName("hash")]
    public string? Hash { get; init; }

    [JsonPropertyName("bytes")]
    public long Bytes { get; init; }

    [JsonPropertyName("original_bytes")]
    public long? OriginalBytes { get; init; }

    [JsonPropertyName("host")]
    public string? Host { get; init; }

    [JsonPropertyName("split")]
    public int Split { get; init; }

    [JsonPropertyName("progress")]
    public double Progress { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("added")]
    public DateTime? Added { get; init; }

    [JsonPropertyName("files")]
    public List<RealDebridFileDto> Files { get; init; } = [];

    [JsonPropertyName("links")]
    public List<string> Links { get; init; } = [];

    [JsonPropertyName("ended")]
    public DateTime? Ended { get; init; }

    [JsonPropertyName("speed")]
    public long? Speed { get; init; }

    [JsonPropertyName("seeders")]
    public int? Seeders { get; init; }
}

/// <summary>
/// File within a torrent from GET /torrents/info/{id}
/// </summary>
public record RealDebridFileDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("bytes")]
    public long Bytes { get; init; }

    [JsonPropertyName("selected")]
    public int Selected { get; init; }
}

/// <summary>
/// Response from POST /unrestrict/link
/// </summary>
public record RealDebridUnrestrictDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("filename")]
    public string Filename { get; init; } = string.Empty;

    [JsonPropertyName("mimeType")]
    public string? MimeType { get; init; }

    [JsonPropertyName("filesize")]
    public long Filesize { get; init; }

    [JsonPropertyName("link")]
    public string Link { get; init; } = string.Empty;

    [JsonPropertyName("host")]
    public string? Host { get; init; }

    [JsonPropertyName("chunks")]
    public int Chunks { get; init; }

    [JsonPropertyName("download")]
    public string Download { get; init; } = string.Empty;

    [JsonPropertyName("streamable")]
    public int Streamable { get; init; }
}

/// <summary>
/// Error response from Real-Debrid API
/// </summary>
public record RealDebridErrorDto
{
    [JsonPropertyName("error")]
    public string Error { get; init; } = string.Empty;

    [JsonPropertyName("error_code")]
    public int? ErrorCode { get; init; }
}
