using Data.DTOs;

namespace Services;

public interface IWatchlistService
{
    Task<IReadOnlyList<GetWatchlistItems>> GetAllWatchlistsAsync();

    Task<GetWatchlistItemsBestPair> GetWatchlistBestPairAsync(decimal targetTotal);

    Task<CreateWatchlistItemResponse?> AddWatchlistAsync(
        CreateWatchlistItem watchlist);
}