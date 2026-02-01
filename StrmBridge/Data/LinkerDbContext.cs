using Microsoft.EntityFrameworkCore;
using StrmBridge.Data.Entities;

namespace StrmBridge.Data;

/// <summary>
/// Database context for the StrmBridge application
/// </summary>
public class LinkerDbContext : DbContext
{
    public LinkerDbContext(DbContextOptions<LinkerDbContext> options) 
        : base(options)
    {
    }

    public DbSet<TrackedMediaItem> TrackedMediaItems => Set<TrackedMediaItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TrackedMediaItem>(entity =>
        {
            // Index on ProviderId for fast lookups during sync
            entity.HasIndex(e => e.ProviderId)
                  .IsUnique();

            // Index on ProviderName for filtering by provider
            entity.HasIndex(e => e.ProviderName);

            // Index on Status for finding missing/unavailable items
            entity.HasIndex(e => e.Status);

            // Composite index for common query pattern
            entity.HasIndex(e => new { e.ProviderName, e.Status });
        });
    }
}
