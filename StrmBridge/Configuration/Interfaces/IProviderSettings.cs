namespace StrmBridge.Configuration.Interfaces;

/// <summary>
/// Base settings for any debrid provider (Torbox, RealDebrid, etc.)
/// </summary>
public interface IProviderSettings
{
    /// <summary>
    /// Unique identifier for this provider (e.g., "Torbox", "RealDebrid")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Whether this provider is enabled for syncing
    /// </summary>
    bool IsEnabled { get; }
}
