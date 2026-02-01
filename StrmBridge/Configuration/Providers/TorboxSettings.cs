using StrmBridge.Configuration.Interfaces;

namespace StrmBridge.Configuration.Providers;

/// <summary>
/// Configuration settings for Torbox provider
/// </summary>
public class TorboxSettings : IApiSettings
{
    public const string SectionName = "Providers:Torbox";

    // IProviderSettings
    public string ProviderName => "Torbox";
    public bool IsEnabled { get; set; } = false;

    // IApiSettings
    public string ApiBaseUrl { get; set; } = "https://api.torbox.app";
    public string ApiKey { get; set; } = string.Empty;
}
