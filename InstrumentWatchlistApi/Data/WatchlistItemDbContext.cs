// Data/WatchlistDbContext.cs
using Data.Entities;
using Microsoft.EntityFrameworkCore;

public class WatchlistItemDbContext : DbContext
{
    public WatchlistItemDbContext(DbContextOptions<WatchlistItemDbContext> options)
        : base(options)
    {
    }

    public DbSet<WatchlistItem> Watchlists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WatchlistItem>(entity =>
        {
            entity.HasKey(watchlist => watchlist.Id);

            entity.Property(watchlist => watchlist.Symbol)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(watchlist => watchlist.TargetPrice)
                .HasPrecision(18, 2);

            entity.Property(watchlist => watchlist.Note)
                .HasMaxLength(250);
        });
    }
}