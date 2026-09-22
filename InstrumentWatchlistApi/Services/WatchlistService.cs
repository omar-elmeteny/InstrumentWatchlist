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

    public async Task<IReadOnlyList<GetWatchlistItems>> GetAllWatchlistItemsAsync()
    {
        var watchlists = await _watchlistRepository.GetAllAsync();
        return watchlists.Select(w => new GetWatchlistItems
        {
            Symbol = w.Symbol,
            TargetPrice = w.TargetPrice,
            Note = w.Note
        }).ToList();
    }

    public async Task<GetWatchlistItemsBestPair> GetWatchlistItemsBestPairAsync(decimal targetTotal)
    {
        var watchlists = await _watchlistRepository.GetAllAsync();
        IReadOnlyList<WatchlistItem> sortedWatchlist = IsSortedByTargetPriceThenSymbol(watchlists)
            ? watchlists
            : watchlists
                .OrderBy(w => w.TargetPrice)
                .ThenBy(w => w.Symbol, StringComparer.Ordinal)
                .ToList();

        var start = 0;
        var end = sortedWatchlist.Count - 1;

        GetWatchlistItemsBestPair watchlistBestPair = new GetWatchlistItemsBestPair();

        var maxCombinedTargetPrice = 0m;
        bool foundMatchingPair = false;

        while (start < end)
        {
            decimal combinedTargetPrice = sortedWatchlist[start].TargetPrice + sortedWatchlist[end].TargetPrice;
            if (sortedWatchlist[start].TargetPrice >= targetTotal)
            {
                watchlistBestPair.Message = "No matching pair";
                return watchlistBestPair;
            }
            else if (sortedWatchlist[end].TargetPrice >= targetTotal ||
                combinedTargetPrice > targetTotal
            )
            {
                end--;
                continue;
            }
            else if (combinedTargetPrice <= targetTotal)
            {

                if (combinedTargetPrice < maxCombinedTargetPrice)
                {
                    start++;
                    continue;
                }
                else
                {
                    var pair = new[] { sortedWatchlist[start], sortedWatchlist[end] }
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
                    else
                    {
                        var existingPair = watchlistBestPair.Items;

                        if (string.Compare(existingPair[0].Symbol, pair[0].Symbol, StringComparison.Ordinal) > 0)
                        {
                            watchlistBestPair.Items.Clear();
                            watchlistBestPair.Items.AddRange(pair);
                        }
                        else if (string.Compare(existingPair[0].Symbol, pair[0].Symbol, StringComparison.Ordinal) == 0 &&
                                string.Compare(existingPair[1].Symbol, pair[1].Symbol, StringComparison.Ordinal) > 0)
                        {
                            watchlistBestPair.Items.Clear();
                            watchlistBestPair.Items.AddRange(pair);
                        }
                    }
                    if (combinedTargetPrice == targetTotal)
                    {
                        end--;
                    }
                    else if (combinedTargetPrice < targetTotal)
                    {
                        if (end != 0 && sortedWatchlist[end].TargetPrice == sortedWatchlist[end - 1].TargetPrice)
                        {
                            decimal combinedTargetPriceT = sortedWatchlist[end].TargetPrice + sortedWatchlist[end - 1].TargetPrice;
                            if(combinedTargetPriceT <= targetTotal)
                            {
                                if(combinedTargetPriceT == combinedTargetPrice)
                                {
                                    end--;
                                }
                                else if(combinedTargetPriceT > combinedTargetPrice)
                                {
                                    start++;
                                }
                            }
                            else
                            {
                                end--;
                            }
                        }
                        else
                        {
                            start++;
                        }
                    }
                }

                watchlistBestPair.Message = "Matching pair found";
                foundMatchingPair = true;
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

    private static bool IsSortedByTargetPriceThenSymbol(IReadOnlyList<WatchlistItem> watchlists)
    {
        for (int index = 1; index < watchlists.Count; index++)
        {
            var previous = watchlists[index - 1];
            var current = watchlists[index];
            var targetPriceComparison = previous.TargetPrice.CompareTo(current.TargetPrice);

            if (targetPriceComparison > 0 ||
                targetPriceComparison == 0 &&
                string.Compare(previous.Symbol, current.Symbol, StringComparison.Ordinal) > 0)
            {
                return false;
            }
        }

        return true;
    }

    public async Task<CreateWatchlistItemResponse?> AddWatchlistItemAsync(
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