namespace StrmBridge.Api.Models;

/// <summary>
/// Status response for the API
/// </summary>
public record StatusResponse
{
    public required string Status { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required AppStatusInfo App { get; init; }
    public required IReadOnlyList<ProviderStatusInfo> Providers { get; init; }
}

public record AppStatusInfo
{
    public required string MediaOutputPath { get; init; }
    public required int SyncIntervalSeconds { get; init; }
    public required string DatabasePath { get; init; }
}

public record ProviderStatusInfo
{
    public required string Name { get; init; }
    public required bool IsEnabled { get; init; }
    public required bool IsHealthy { get; init; }
    public string? Error { get; init; }
    public int? ItemCount { get; init; }
}
