using Data.DTOs;
using Data.Entities;
using Repositories;

namespace Services;

public class WatchlistService : IWatchlistService
{
    private readonly IWatchlistRepository _watchlistRepository;

    public WatchlistService(IWatchlistRepository watchlistRepository)
    {
        _watchlistRepository = watchlistRepository;
    }

    public async Task<IReadOnlyList<GetWatchlistItems>> GetAllWatchlistsAsync()
    {
        var watchlists = await _watchlistRepository.GetAllAsync();
        return watchlists.Select(w => new GetWatchlistItems
        {
            Symbol = w.Symbol,
            TargetPrice = w.TargetPrice,
            Note = w.Note
        }).ToList();
    }

    public async Task<GetWatchlistItemsBestPair> GetWatchlistBestPairAsync(decimal targetTotal)
    {
        var watchlists = await _watchlistRepository.GetAllAsync();
        var sortedWatchlists = watchlists.OrderBy(w => w.TargetPrice).ToList();

        var start = 0;
        var end = sortedWatchlists.Count - 1;

        GetWatchlistItemsBestPair watchlistBestPair = new GetWatchlistItemsBestPair();

        var maxCombinedTargetPrice = 0m;
        bool foundMatchingPair = false;

        while (start < end)
        {
            decimal combinedTargetPrice = sortedWatchlists[start].TargetPrice + sortedWatchlists[end].TargetPrice;
            if (sortedWatchlists[start].TargetPrice >= targetTotal)
            {
                watchlistBestPair.Message = "No matching pair";
                return watchlistBestPair;
            }
            else if (sortedWatchlists[end].TargetPrice >= targetTotal ||
                combinedTargetPrice > targetTotal
            )
            {
                end--;
                continue;
            }
            else if (combinedTargetPrice <= targetTotal)
            {
                var pair = new[] { sortedWatchlists[start], sortedWatchlists[end] }
                    .OrderBy(watchlist => watchlist.Symbol, StringComparer.Ordinal)
                    .Select(watchlist => new GetWatchlistItems
                    {
                        Symbol = watchlist.Symbol,
                        TargetPrice = watchlist.TargetPrice,
                        Note = watchlist.Note
                    })
                    .ToList();
                if (combinedTargetPrice > maxCombinedTargetPrice)
                {
                    maxCombinedTargetPrice = combinedTargetPrice;

                    watchlistBestPair.Items.Clear();
                    watchlistBestPair.Items.AddRange(pair);
                    watchlistBestPair.CombinedTargetPrice = maxCombinedTargetPrice;
                }
                else if (combinedTargetPrice < maxCombinedTargetPrice)
                {
                    start++;
                    continue;
                }
                else
                {
                    var existingPair = watchlistBestPair.Items;

                    if (string.Compare(existingPair[0].Symbol, pair[0].Symbol, StringComparison.Ordinal) > 0)
                    {
                        watchlistBestPair.Items.Clear();
                        watchlistBestPair.Items.AddRange(pair);
                    }
                }
                   
                watchlistBestPair.Message = "Matching pair found";
                foundMatchingPair = true;
                start++;
            }
        }

        if (!foundMatchingPair)
        {
            watchlistBestPair.Message = "No matching pair";
            watchlistBestPair.Items.Clear();
            watchlistBestPair.CombinedTargetPrice = null;
            return watchlistBestPair;
        }


        return watchlistBestPair;

    }

    public async Task<CreateWatchlistItemResponse?> AddWatchlistAsync(
        CreateWatchlistItem watchlist)
    {
        if (await _watchlistRepository.SymbolExistsAsync(watchlist.Symbol))
        {
            return null;
        }

        var newWatchlist = new WatchlistItem
        {
            Symbol = watchlist.Symbol,
            TargetPrice = watchlist.TargetPrice,
            Note = watchlist.Note
        };

        await _watchlistRepository.AddAsync(newWatchlist);

        return new CreateWatchlistItemResponse
        {
            Id = newWatchlist.Id,
            Symbol = newWatchlist.Symbol,
            TargetPrice = newWatchlist.TargetPrice,
            Note = newWatchlist.Note
        };
    }
}