using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class WatchlistRepository : IWatchlistRepository
{
    private readonly WatchlistItemDbContext _context;

    public WatchlistRepository(WatchlistItemDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WatchlistItem>> GetAllAsync()
    {
        return await _context.Watchlists.ToListAsync();
    }

    public async Task<WatchlistItem?> GetAsync(string symbol)
    {
        return await _context.Watchlists
            .FirstOrDefaultAsync(watchlist => watchlist.Symbol == symbol.ToUpperInvariant());
    }

    public Task<bool> SymbolExistsAsync(string symbol)
    {
        return _context.Watchlists.AnyAsync(watchlist =>
            watchlist.Symbol == symbol.ToUpperInvariant());
    }

    public async Task AddAsync(WatchlistItem watchlist)
    {
        await _context.Watchlists.AddAsync(watchlist);
        await _context.SaveChangesAsync();
    }
}