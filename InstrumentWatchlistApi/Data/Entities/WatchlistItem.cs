namespace Data.Entities;

public class WatchlistItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    private string _symbol = string.Empty;

    public string Symbol
    {
        get => _symbol;
        set => _symbol = value.ToUpperInvariant();
    }

    public decimal TargetPrice { get; set; }

    public string? Note { get; set; }
}