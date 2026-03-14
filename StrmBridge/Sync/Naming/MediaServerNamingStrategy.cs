using System.Globalization;
using System.Text.RegularExpressions;

namespace StrmBridge.Sync.Naming;

/// <summary>
/// Cleans torrent names and organizes into standard Plex/Jellyfin folder structure.
/// 
/// Movies:  Movies/Movie Name (Year)/Movie Name (Year).strm
/// TV:      TV Shows/Show Name/Season 01/Show Name - S01E01.strm
/// </summary>
public partial class MediaServerNamingStrategy : IMediaNamingStrategy
{
    private readonly ILogger<MediaServerNamingStrategy> _logger;

    // Quality/release markers that indicate end of title
    private static readonly string[] QualityMarkers = 
    [
        "1080p", "720p", "2160p", "4k", "480p", "576p",
        "web-dl", "webrip", "web", "bluray", "bdrip", "brrip", "hdtv", "dvdrip", "hdrip",
        "x264", "x265", "h264", "h265", "hevc", "avc", "xvid", "divx",
        "aac", "ac3", "dts", "dd5", "ddp5", "truehd", "atmos",
        "hdr", "hdr10", "dv", "dolby", "vision",
        "remux", "proper", "repack", "extended", "unrated", "directors"
    ];

    public MediaServerNamingStrategy(ILogger<MediaServerNamingStrategy> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generates the output path for a media item.
    /// This is called with the torrent name - we return the folder structure.
    /// The actual filename is handled separately in SyncEngine.
    /// </summary>
    public string GetOutputPath(string originalName, string providerName)
    {
        var cleanedInput = StripPrefixes(originalName);
        
        var tvMatch = TvShowParseRegex().Match(cleanedInput);
        
        if (tvMatch.Success)
        {
            return GetTvShowPath(tvMatch, cleanedInput);
        }

        var movieMatch = MovieParseRegex().Match(cleanedInput);
        if (movieMatch.Success)
        {
            return GetMoviePath(movieMatch);
        }

        var fallbackName = CleanTitle(cleanedInput);
        _logger.LogDebug("Unknown media type for '{Original}', using Other/", originalName);
        return Path.Combine("Other", fallbackName, fallbackName);
    }

    /// <summary>
    /// Gets the path for a specific file within a media item.
    /// This handles the actual .strm filename.
    /// </summary>
    public string GetFilePath(string torrentName, string fileName)
    {
        var cleanedTorrent = StripPrefixes(torrentName);
        var cleanedFile = StripPrefixes(fileName);

        string basePath;

        var tvMatch = TvShowParseRegex().Match(cleanedFile);
        if (tvMatch.Success)
        {
            var showName = tvMatch.Groups["show"].Value.Trim();
            basePath = string.IsNullOrWhiteSpace(showName)
                ? GetTvShowFilePathWithShowName(tvMatch, cleanedTorrent)
                : GetTvShowFilePath(tvMatch, cleanedFile);
        }
        else if ((tvMatch = TvShowParseRegex().Match(cleanedTorrent)).Success)
        {
            basePath = GetTvShowFilePathFromTorrent(tvMatch, cleanedFile);
        }
        else
        {
            var movieMatch = MovieParseRegex().Match(cleanedTorrent);
            if (!movieMatch.Success)
                movieMatch = MovieParseRegex().Match(cleanedFile);

            basePath = movieMatch.Success
                ? GetMovieFilePath(movieMatch, cleanedFile)
                : Path.Combine("Other", CleanTitle(cleanedFile) + ".strm");
        }

        var qualitySuffix = ExtractQualitySuffix(torrentName, fileName);
        if (qualitySuffix.Length > 0)
            basePath = basePath.Replace(".strm", $" - {qualitySuffix}.strm");

        return basePath;
    }

    /// <summary>
    /// Strips common prefixes from torrent/file names
    /// </summary>
    private string StripPrefixes(string name)
    {
        var clean = name;

        clean = WebsitePrefixRegex().Replace(clean, "");
        clean = BracketedTagRegex().Replace(clean, "");
        clean = BracedTagRegex().Replace(clean, "");
        clean = LeadingJunkRegex().Replace(clean, "");
        
        return clean.Trim();
    }

    /// <summary>
    /// Gets the folder path for a TV show
    /// </summary>
    private string GetTvShowPath(Match match, string original)
    {
        var showName = CleanTitle(match.Groups["show"].Value);
        var seasonNum = int.Parse(match.Groups["season"].Value);
        var seasonFolder = $"Season {seasonNum:D2}";

        _logger.LogDebug("TV Show detected: '{Show}' Season {Season}", showName, seasonNum);

        return Path.Combine("TV Shows", showName, seasonFolder, showName);
    }

    /// <summary>
    /// Gets the full file path for a TV show episode
    /// </summary>
    private static string GetTvShowFilePath(Match match, string fileName)
    {
        var showName = CleanTitle(match.Groups["show"].Value);
        var seasonNum = int.Parse(match.Groups["season"].Value);
        var episodeNum = int.Parse(match.Groups["episode"].Value);
        var seasonFolder = $"Season {seasonNum:D2}";

        var strmFileName = $"{showName} - S{seasonNum:D2}E{episodeNum:D2}.strm";

        return Path.Combine("TV Shows", showName, seasonFolder, strmFileName);
    }

    /// <summary>
    /// Gets the full file path when file has episode info but no show name.
    /// Uses the torrent name to derive the show name.
    /// E.g., file "S1Ep1 - Episode.mkv" from torrent "Still.Game.Complete" -> "Still Game"
    /// </summary>
    private static string GetTvShowFilePathWithShowName(Match fileMatch, string torrentName)
    {
        var showName = ExtractShowNameFromTorrent(torrentName);
        var seasonNum = int.Parse(fileMatch.Groups["season"].Value);
        var episodeNum = int.Parse(fileMatch.Groups["episode"].Value);
        var seasonFolder = $"Season {seasonNum:D2}";

        var strmFileName = $"{showName} - S{seasonNum:D2}E{episodeNum:D2}.strm";

        return Path.Combine("TV Shows", showName, seasonFolder, strmFileName);
    }

    /// <summary>
    /// Gets the full file path when torrent has episode info but we're using file's clean name
    /// </summary>
    private static string GetTvShowFilePathFromTorrent(Match torrentMatch, string fileName)
    {
        var showName = CleanTitle(torrentMatch.Groups["show"].Value);
        var seasonNum = int.Parse(torrentMatch.Groups["season"].Value);
        var episodeNum = int.Parse(torrentMatch.Groups["episode"].Value);
        var seasonFolder = $"Season {seasonNum:D2}";

        var strmFileName = $"{showName} - S{seasonNum:D2}E{episodeNum:D2}.strm";

        return Path.Combine("TV Shows", showName, seasonFolder, strmFileName);
    }

    /// <summary>
    /// Extracts show name from a torrent name like "Still.Game.Complete.Collection"
    /// </summary>
    private static string ExtractShowNameFromTorrent(string torrentName)
    {
        string[] endMarkers = ["complete", "collection", "season", "series", "s01", "s02", "s03", 
                               "s04", "s05", "s06", "s07", "s08", "s09", "s10", "dvdrip", "1080p", 
                               "720p", "480p", "x264", "x265", "web-dl", "webrip", "hdtv"];
        
        var clean = torrentName.Replace('.', ' ').Replace('_', ' ');
        
        foreach (var marker in endMarkers)
        {
            var idx = clean.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                clean = clean[..idx];
                break;
            }
        }

        return CleanTitle(clean);
    }

    /// <summary>
    /// Gets the folder path for a movie
    /// </summary>
    private string GetMoviePath(Match match)
    {
        var title = CleanTitle(match.Groups["title"].Value);
        var year = match.Groups["year"].Value;
        var folderName = $"{title} ({year})";

        _logger.LogDebug("Movie detected: '{Title}' ({Year})", title, year);

        return Path.Combine("Movies", folderName, folderName);
    }

    /// <summary>
    /// Gets the full file path for a movie
    /// </summary>
    private string GetMovieFilePath(Match match, string fileName)
    {
        var title = CleanTitle(match.Groups["title"].Value);
        var year = match.Groups["year"].Value;
        var folderName = $"{title} ({year})";
        var strmFileName = $"{folderName}.strm";

        return Path.Combine("Movies", folderName, strmFileName);
    }

    /// <summary>
    /// Cleans a title by replacing dots/underscores with spaces,
    /// removing quality markers, and title-casing
    /// </summary>
    private static string CleanTitle(string input)
    {
        // Replace dots and underscores with spaces
        var clean = input.Replace('.', ' ').Replace('_', ' ');
        
        // Remove anything after quality markers
        foreach (var marker in QualityMarkers)
        {
            var idx = clean.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                clean = clean[..idx];
                break;
            }
        }

        // Remove release group at end (after dash)
        clean = TrailingReleaseGroupRegex().Replace(clean, "");
        
        // Clean up multiple spaces
        clean = MultipleSpacesRegex().Replace(clean, " ");
        
        // Title case
        clean = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(clean.ToLower().Trim());
        
        return clean.Trim(' ', '-');
    }

    private static readonly HashSet<string> NonGroupSuffixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "dl", "rip", "ray", "remux", "hdtv", "bluray", "webrip", "webdl",
        "bdrip", "brrip", "dvdrip", "hdrip",
        "x264", "x265", "h264", "h265", "hevc", "avc", "xvid", "divx",
        "1080p", "720p", "2160p", "480p", "576p", "4k",
        "aac", "ac3", "dts", "flac", "mp3",
        "hdr", "hdr10", "sdr", "dv"
    };

    private static string ExtractQualitySuffix(string torrentName, string fileName)
    {
        var parts = new List<string>();

        var resolution = MatchFirst(ResolutionExtractRegex(), torrentName, fileName);
        if (resolution != null)
            parts.Add(resolution.ToLowerInvariant() == "4k" ? "2160p" : resolution.ToLowerInvariant());

        var source = NormalizeSource(MatchFirst(SourceExtractRegex(), torrentName, fileName));
        if (source != null)
            parts.Add(source);

        var group = MatchReleaseGroup(torrentName) ?? MatchReleaseGroup(fileName);

        var suffix = string.Join(" ", parts);
        if (group != null)
            suffix = suffix.Length > 0 ? $"{suffix}-{group}" : group;

        return suffix;
    }

    private static string? MatchFirst(Regex regex, string primary, string secondary)
    {
        var match = regex.Match(primary);
        if (match.Success) return match.Groups[1].Value;
        match = regex.Match(secondary);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string? NormalizeSource(string? value)
    {
        if (value == null) return null;
        return value.Replace(" ", "-").Replace("_", "-").ToUpperInvariant() switch
        {
            "WEB-DL" or "WEBDL" => "WEB-DL",
            "WEBRIP" => "WEBRip",
            "BLURAY" or "BLU-RAY" => "BluRay",
            "REMUX" => "REMUX",
            "BDRIP" => "BDRip",
            "BRRIP" => "BRRip",
            "HDTV" => "HDTV",
            "DVDRIP" => "DVDRip",
            "HDRIP" => "HDRip",
            _ => value
        };
    }

    private static string? MatchReleaseGroup(string input)
    {
        var match = ReleaseGroupExtractRegex().Match(input);
        if (!match.Success) return null;
        var group = match.Groups[1].Value;
        return NonGroupSuffixes.Contains(group) ? null : group;
    }

    // === REGEX PATTERNS (Source Generated) ===

    // Matches: "www.site.com - " prefixes
    [GeneratedRegex(@"^www\.[a-z0-9.-]+\s*[-–—]\s*", RegexOptions.IgnoreCase)]
    private static partial Regex WebsitePrefixRegex();

    // Matches: [bracketed tags] often found in release names
    [GeneratedRegex(@"\[[^\]]+\]")]
    private static partial Regex BracketedTagRegex();

    // Matches: {tag}
    [GeneratedRegex(@"\{[^}]+\}")]
    private static partial Regex BracedTagRegex();

    // Matches leading dashes/spaces after stripping
    [GeneratedRegex(@"^[\s\-–—]+")]
    private static partial Regex LeadingJunkRegex();

    // Matches: -GROUP at end of string
    [GeneratedRegex(@"\s*-\s*[A-Za-z0-9]+$")]
    private static partial Regex TrailingReleaseGroupRegex();

    // Multiple spaces
    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex MultipleSpacesRegex();

    // TV Show pattern: extracts show name, season, episode
    // Matches: "Show.Name.S01E05", "Show Name S1E5", "S1Ep1 - Episode Name", etc.
    // Also handles "S1Ep1" format (with Ep instead of just E)
    [GeneratedRegex(@"^(?<show>.+?)?[.\s\-_]*[Ss](?<season>\d{1,2})[Ee][Pp]?(?<episode>\d{1,2})", RegexOptions.IgnoreCase)]
    private static partial Regex TvShowParseRegex();

    // Movie pattern: extracts title and year
    // Matches: "Movie.Name.2024.1080p" or "Movie Name (2024)" etc.
    [GeneratedRegex(@"^(?<title>.+?)[.\s\-_\(]+(?<year>(?:19|20)\d{2})[\s.\-_\)]", RegexOptions.IgnoreCase)]
    private static partial Regex MovieParseRegex();

    [GeneratedRegex(@"\b(2160p|1080p|720p|480p|576p|4[Kk])\b", RegexOptions.IgnoreCase)]
    private static partial Regex ResolutionExtractRegex();

    [GeneratedRegex(@"\b(WEB[-\s]?DL|WEBRip|Blu[-\s]?Ray|BDRip|BRRip|HDTV|DVDRip|HDRip|REMUX)\b", RegexOptions.IgnoreCase)]
    private static partial Regex SourceExtractRegex();

    [GeneratedRegex(@"-([A-Za-z0-9]{2,})(?:\.[a-zA-Z0-9]{2,4})?$")]
    private static partial Regex ReleaseGroupExtractRegex();
}

public enum DetectedMediaType
{
    Unknown,
    Movie,
    TvShow
}
