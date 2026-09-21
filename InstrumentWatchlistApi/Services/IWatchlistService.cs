using Data.DTOs;

namespace Services;

public interface IWatchlistService
{
    Task<IReadOnlyList<GetWatchlistItems>> GetAllWatchlistItemsAsync();

    Task<GetWatchlistItemsBestPair> GetWatchlistItemsBestPairAsync(decimal targetTotal);

    Task<CreateWatchlistItemResponse?> AddWatchlistItemAsync(
        CreateWatchlistItem watchlist);
}