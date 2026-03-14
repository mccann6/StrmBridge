namespace StrmBridge.Sync;

/// <summary>
/// File system implementation of .strm file manager.
/// Creates simple text files containing streaming URLs.
/// </summary>
public class StrmFileManager : IStrmFileManager
{
    private readonly ILogger<StrmFileManager> _logger;

    public StrmFileManager(ILogger<StrmFileManager> logger)
    {
        _logger = logger;
    }

    public async Task<bool> CreateStrmFileAsync(string strmPath, string streamingUrl, CancellationToken ct = default)
    {
        try
        {
            if (!strmPath.EndsWith(".strm", StringComparison.OrdinalIgnoreCase))
            {
                strmPath += ".strm";
            }

            if (File.Exists(strmPath))
            {
                var existingUrl = await File.ReadAllTextAsync(strmPath, ct);
                if (existingUrl.Trim() == streamingUrl.Trim())
                {
                    _logger.LogDebug(".strm file already exists with correct URL: {Path}", strmPath);
                    return false;
                }

                _logger.LogDebug("Updating existing .strm file: {Path}", strmPath);
            }

            var strmDir = Path.GetDirectoryName(strmPath);
            if (!string.IsNullOrEmpty(strmDir) && !Directory.Exists(strmDir))
            {
                Directory.CreateDirectory(strmDir);
                _logger.LogDebug("Created directory: {Directory}", strmDir);
            }

            await File.WriteAllTextAsync(strmPath, streamingUrl, ct);

            _logger.LogInformation("Created .strm file: {Path}", strmPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create .strm file: {Path}", strmPath);
            throw;
        }
    }

    public Task<bool> RemoveStrmFileAsync(string strmPath, CancellationToken ct = default)
    {
        try
        {
            if (!File.Exists(strmPath))
            {
                return Task.FromResult(false);
            }

            File.Delete(strmPath);
            _logger.LogInformation("Removed .strm file: {Path}", strmPath);

            CleanupEmptyDirectories(Path.GetDirectoryName(strmPath));

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove .strm file: {Path}", strmPath);
            throw;
        }
    }

    public Task<bool> StrmFileExistsAsync(string strmPath, CancellationToken ct = default)
    {
        return Task.FromResult(File.Exists(strmPath));
    }

    public async Task<string?> GetStrmUrlAsync(string strmPath, CancellationToken ct = default)
    {
        if (!File.Exists(strmPath))
        {
            return null;
        }

        var content = await File.ReadAllTextAsync(strmPath, ct);
        return content.Trim();
    }

    public Task<bool> MoveStrmFileAsync(string oldPath, string newPath, CancellationToken ct = default)
    {
        try
        {
            if (!File.Exists(oldPath))
                return Task.FromResult(false);

            var newDir = Path.GetDirectoryName(newPath);
            if (!string.IsNullOrEmpty(newDir) && !Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);

            File.Move(oldPath, newPath, overwrite: true);
            _logger.LogInformation("Moved .strm file: {OldPath} -> {NewPath}", oldPath, newPath);

            CleanupEmptyDirectories(Path.GetDirectoryName(oldPath));
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to move .strm file: {OldPath} -> {NewPath}", oldPath, newPath);
            return Task.FromResult(false);
        }
    }

    private void CleanupEmptyDirectories(string? directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath))
            return;

        try
        {
            while (Directory.Exists(directoryPath) &&
                   !Directory.EnumerateFileSystemEntries(directoryPath).Any())
            {
                Directory.Delete(directoryPath);
                _logger.LogDebug("Removed empty directory: {Directory}", directoryPath);
                directoryPath = Path.GetDirectoryName(directoryPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not cleanup empty directories from: {Directory}", directoryPath);
        }
    }
}
