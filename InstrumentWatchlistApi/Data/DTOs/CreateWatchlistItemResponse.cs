namespace Data.DTOs;

public class CreateWatchlistItemResponse
{
    public string Id { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public decimal TargetPrice { get; init; }
    public string? Note { get; init; }
}