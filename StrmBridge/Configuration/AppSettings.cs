using StrmBridge.Configuration.Interfaces;

namespace StrmBridge.Configuration;

/// <summary>
/// Application-level settings (provider-agnostic)
/// </summary>
public class AppSettings : IAppSettings
{
    public const string SectionName = "App";

    /// <inheritdoc />
    public string MediaOutputPath { get; set; } = "/app/media";

    /// <inheritdoc />
    public int SyncIntervalSeconds { get; set; } = 300;

    /// <inheritdoc />
    public string DatabasePath { get; set; } = "/app/data/linker.db";

    /// <inheritdoc />
    public string ServiceBaseUrl { get; set; } = "http://localhost:8080";
}
