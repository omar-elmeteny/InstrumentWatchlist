namespace Data.DTOs;

public class GetWatchlistItems
{
    public string Symbol { get; set; } = string.Empty;

    public decimal TargetPrice { get; set; }

    public string? Note { get; set; }
}