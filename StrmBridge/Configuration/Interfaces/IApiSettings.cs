namespace StrmBridge.Configuration.Interfaces;

/// <summary>
/// Settings for providers that expose an API for library listing
/// </summary>
public interface IApiSettings : IProviderSettings
{
    /// <summary>
    /// Base URL for the provider's API (e.g., "https://api.torbox.app")
    /// </summary>
    string ApiBaseUrl { get; }

    /// <summary>
    /// API key or bearer token for authentication
    /// </summary>
    string ApiKey { get; }
}
