using Microsoft.EntityFrameworkCore;
using StrmBridge.Data.Entities;

namespace StrmBridge.Data;

/// <summary>
/// EF Core implementation of media item repository
/// </summary>
public class MediaItemRepository : IMediaItemRepository
{
    private readonly LinkerDbContext _context;

    public MediaItemRepository(LinkerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TrackedMediaItem>> GetByProviderAsync(
        string providerName, 
        CancellationToken ct = default)
    {
        return await _context.TrackedMediaItems
            .Where(x => x.ProviderName == providerName)
            .ToListAsync(ct);
    }

    public async Task<TrackedMediaItem?> GetByProviderIdAsync(
        string providerId, 
        CancellationToken ct = default)
    {
        return await _context.TrackedMediaItems
            .FirstOrDefaultAsync(x => x.ProviderId == providerId, ct);
    }

    public async Task<TrackedMediaItem> AddAsync(
        TrackedMediaItem item, 
        CancellationToken ct = default)
    {
        _context.TrackedMediaItems.Add(item);
        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task UpdateAsync(
        TrackedMediaItem item, 
        CancellationToken ct = default)
    {
        _context.TrackedMediaItems.Update(item);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TrackedMediaItem>> MarkMissingAsync(
        string providerName, 
        IEnumerable<string> seenProviderIds, 
        CancellationToken ct = default)
    {
        var seenSet = seenProviderIds.ToHashSet();

        var itemsToMark = await _context.TrackedMediaItems
            .Where(x => x.ProviderName == providerName && 
                        x.Status == MediaItemStatus.Active &&
                        !seenSet.Contains(x.ProviderId))
            .ToListAsync(ct);

        foreach (var item in itemsToMark)
        {
            item.Status = MediaItemStatus.Missing;
        }

        await _context.SaveChangesAsync(ct);
        return itemsToMark;
    }

    public async Task<Dictionary<string, TrackedMediaItem>> GetProviderItemsAsDictionaryAsync(
        string providerName,
        CancellationToken ct = default)
    {
        return await _context.TrackedMediaItems
            .Where(x => x.ProviderName == providerName)
            .ToDictionaryAsync(x => x.ProviderId, ct);
    }

    public async Task AddRangeAsync(
        IEnumerable<TrackedMediaItem> items,
        CancellationToken ct = default)
    {
        _context.TrackedMediaItems.AddRange(items);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
