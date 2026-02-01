using StrmBridge.Configuration.Interfaces;

namespace StrmBridge.Configuration.Providers;

/// <summary>
/// Configuration settings for Real-Debrid provider
/// </summary>
public class RealDebridSettings : IApiSettings
{
    public const string SectionName = "Providers:RealDebrid";

    // IProviderSettings
    public string ProviderName => "RealDebrid";
    public bool IsEnabled { get; set; }

    // IApiSettings
    public string ApiBaseUrl { get; set; } = "https://api.real-debrid.com";
    public string ApiKey { get; set; } = string.Empty;
}
