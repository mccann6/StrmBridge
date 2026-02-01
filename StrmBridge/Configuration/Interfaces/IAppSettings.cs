namespace StrmBridge.Configuration.Interfaces;

/// <summary>
/// Application-level settings that are provider-agnostic
/// </summary>
public interface IAppSettings
{
    /// <summary>
    /// Output path where .strm files are created (Jellyfin reads this)
    /// </summary>
    string MediaOutputPath { get; }

    /// <summary>
    /// How often (in seconds) to sync with providers
    /// </summary>
    int SyncIntervalSeconds { get; }

    /// <summary>
    /// Path to the SQLite database file
    /// </summary>
    string DatabasePath { get; }

    /// <summary>
    /// Base URL for this service, used in .strm files for streaming redirects.
    /// E.g., "http://strmbridge:8080" or "http://192.168.1.100:8080"
    /// </summary>
    string ServiceBaseUrl { get; }
}
