using Data.Entities;
using Moq;
using Repositories;
using Services;

namespace InstrumentWatchlistTests.Unit.Services.Watchlist;

public class GetAllWatchlistItemsTests
{
    [Fact]
    public async Task GetAllWatchlistItems_ReturnsAllItems()
    {
        // Arrange
        var repository = new Mock<IWatchlistRepository>();
        List<WatchlistItem> items = new() {
            new WatchlistItem { Symbol = "AAPL", TargetPrice = 200.50m, Note = "Long-term position" },
            new WatchlistItem { Symbol = "MSFT", TargetPrice = 150.00m, Note = "Short-term position" }
        };
        repository
            .Setup(mock => mock.GetAllAsync())
            .ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        // Act
        var result = await service.GetAllWatchlistItemsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("AAPL", result[0].Symbol);
        Assert.Equal(200.50m, result[0].TargetPrice);
        Assert.Equal("Long-term position", result[0].Note);
        Assert.Equal("MSFT", result[1].Symbol);
        Assert.Equal(150.00m, result[1].TargetPrice);
        Assert.Equal("Short-term position", result[1].Note);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllWatchlistItems_WhenNoItems_ReturnsEmptyList()
    {
        // Arrange
        var repository = new Mock<IWatchlistRepository>();
        repository
            .Setup(mock => mock.GetAllAsync())
            .ReturnsAsync(new List<WatchlistItem>());
        var service = new WatchlistService(repository.Object);

        // Act
        var result = await service.GetAllWatchlistItemsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }
}