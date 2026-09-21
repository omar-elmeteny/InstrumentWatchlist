namespace Data.DTOs;

public class GetWatchlistItemsBestPair
{
    public List<GetWatchlistItems> Items { get; init; } = new List<GetWatchlistItems>();

    public decimal? CombinedTargetPrice { get; set; } = null;

    public string Message { get; set; } = string.Empty;
}