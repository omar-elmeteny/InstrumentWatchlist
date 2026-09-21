using Data.Entities;

namespace Repositories;

public interface IWatchlistRepository
{
    Task<IReadOnlyList<WatchlistItem>> GetAllAsync();

    Task<WatchlistItem?> GetAsync(string symbol);  

    Task<bool> SymbolExistsAsync(string symbol);

    Task AddAsync(WatchlistItem watchlist);
}