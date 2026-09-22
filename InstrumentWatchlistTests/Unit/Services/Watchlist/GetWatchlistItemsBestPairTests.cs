namespace InstrumentWatchlistTests.Unit.Services.Watchlist;

using Data.Entities;
using global::Services;
using Moq;
using Repositories;
using Services;

public class GetWatchlistItemsBestPairTests
{
    [Fact]
    public async Task GetWatchlistItemsBestPair_ExactMatch_ReturnsBestPair()
    {
        // Arrange
        var repository = new Mock<IWatchlistRepository>();
        decimal totalTarget = 400.00m;
        List<WatchlistItem> items = new() {
            new WatchlistItem { Symbol = "AAPL", TargetPrice = 200.50m, Note = "Long-term position" },
            new WatchlistItem { Symbol = "MSFT", TargetPrice = 150.00m, Note = "Short-term position" },
            new WatchlistItem { Symbol = "GOOGL", TargetPrice = 250.00m, Note = "Long-term growth" },
            new WatchlistItem { Symbol = "AMZN", TargetPrice = 330.00m, Note = "E-commerce giant" }
        };
        repository
            .Setup(mock => mock.GetAllAsync())
            .ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        // Act
        var result = await service.GetWatchlistItemsBestPairAsync(totalTarget);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(400.00m, result.CombinedTargetPrice);
        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal("GOOGL", result.Items[0].Symbol);
        Assert.Equal(250.00m, result.Items[0].TargetPrice);
        Assert.Equal("Long-term growth", result.Items[0].Note);
        Assert.Equal("MSFT", result.Items[1].Symbol);
        Assert.Equal(150.00m, result.Items[1].TargetPrice);
        Assert.Equal("Short-term position", result.Items[1].Note);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_ClosestQualifyingPairing_ReturnsBestPair()
    {
        // Arrange
        var repository = new Mock<IWatchlistRepository>();
        decimal totalTarget = 500.00m;
        List<WatchlistItem> items = new() {
            new WatchlistItem { Symbol = "AAPL", TargetPrice = 200.50m, Note = "Long-term position" },
            new WatchlistItem { Symbol = "MSFT", TargetPrice = 150.00m, Note = "Short-term position" },
            new WatchlistItem { Symbol = "GOOGL", TargetPrice = 250.00m, Note = "Long-term growth" },
            new WatchlistItem { Symbol = "AMZN", TargetPrice = 330.00m, Note = "E-commerce giant" }
        };
        repository
            .Setup(mock => mock.GetAllAsync())
            .ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        // Act
        var result = await service.GetWatchlistItemsBestPairAsync(totalTarget);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(480.00m, result.CombinedTargetPrice);
        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal("AMZN", result.Items[0].Symbol);
        Assert.Equal(330.00m, result.Items[0].TargetPrice);
        Assert.Equal("E-commerce giant", result.Items[0].Note);
        Assert.Equal("MSFT", result.Items[1].Symbol);
        Assert.Equal(150.00m, result.Items[1].TargetPrice);
        Assert.Equal("Short-term position", result.Items[1].Note);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_NoQualifyingPair_ReturnsNull()
    {
        // Arrange
        var repository = new Mock<IWatchlistRepository>();
        decimal totalTarget = 200.00m;
        List<WatchlistItem> items = new() {
            new WatchlistItem { Symbol = "AAPL", TargetPrice = 200.50m, Note = "Long-term position" },
            new WatchlistItem { Symbol = "MSFT", TargetPrice = 150.00m, Note = "Short-term position" },
            new WatchlistItem { Symbol = "GOOGL", TargetPrice = 250.00m, Note = "Long-term growth" },
            new WatchlistItem { Symbol = "AMZN", TargetPrice = 330.00m, Note = "E-commerce giant" }
        };
        repository
            .Setup(mock => mock.GetAllAsync())
            .ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        // Act
        var result = await service.GetWatchlistItemsBestPairAsync(totalTarget);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Null(result.CombinedTargetPrice);
        Assert.Equal("No matching pair", result.Message);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_TargetTotalExceedsAllPairs_ReturnsBestPair()
    {
        // Arrange
        var repository = new Mock<IWatchlistRepository>();
        decimal totalTarget = 4000.00m;
        List<WatchlistItem> items = new() {
            new WatchlistItem { Symbol = "AAPL", TargetPrice = 200.50m, Note = "Long-term position" },
            new WatchlistItem { Symbol = "MSFT", TargetPrice = 150.00m, Note = "Short-term position" },
            new WatchlistItem { Symbol = "GOOGL", TargetPrice = 250.00m, Note = "Long-term growth" },
            new WatchlistItem { Symbol = "AMZN", TargetPrice = 330.00m, Note = "E-commerce giant" }
        };
        repository
            .Setup(mock => mock.GetAllAsync())
            .ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        // Act
        var result = await service.GetWatchlistItemsBestPairAsync(totalTarget);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(580.00m, result.CombinedTargetPrice);
        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal("AMZN", result.Items[0].Symbol);
        Assert.Equal(330.00m, result.Items[0].TargetPrice);
        Assert.Equal("E-commerce giant", result.Items[0].Note);
        Assert.Equal("GOOGL", result.Items[1].Symbol);
        Assert.Equal(250.00m, result.Items[1].TargetPrice);
        Assert.Equal("Long-term growth", result.Items[1].Note);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_AlphabeticalTieBreak_ReturnsBestPair()
    {
        // Arrange
        var repository = new Mock<IWatchlistRepository>();
        decimal totalTarget = 350.00m;
        List<WatchlistItem> items = new() {
            new WatchlistItem { Symbol = "AAPL", TargetPrice = 200.00m, Note = "Long-term position" },
            new WatchlistItem { Symbol = "MSFT", TargetPrice = 200.00m, Note = "Short-term position" },
            new WatchlistItem { Symbol = "GOOGL", TargetPrice = 150.00m, Note = "Long-term growth" },
            new WatchlistItem { Symbol = "AMZN", TargetPrice = 150.00m, Note = "E-commerce giant" }
        };
        repository
            .Setup(mock => mock.GetAllAsync())
            .ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        // Act
        var result = await service.GetWatchlistItemsBestPairAsync(totalTarget);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(350.00m, result.CombinedTargetPrice);
        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal("AAPL", result.Items[0].Symbol);
        Assert.Equal(200.00m, result.Items[0].TargetPrice);
        Assert.Equal("Long-term position", result.Items[0].Note);
        Assert.Equal("AMZN", result.Items[1].Symbol);
        Assert.Equal(150.00m, result.Items[1].TargetPrice);
        Assert.Equal("E-commerce giant", result.Items[1].Note);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithFourItemsAt150_ReturnsTwoItemsAt150()
    {
        const decimal targetTotal = 300.00m;
        List<WatchlistItem> items =
        [
            new() { Symbol = "ITEM01", TargetPrice = 150.00m },
            new() { Symbol = "ITEM02", TargetPrice = 150.00m },
            new() { Symbol = "ITEM03", TargetPrice = 150.00m },
            new() { Symbol = "ITEM04", TargetPrice = 150.00m }
        ];
        var repository = new Mock<IWatchlistRepository>();
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        var result = await service.GetWatchlistItemsBestPairAsync(targetTotal);

        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(targetTotal, result.CombinedTargetPrice);
        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal("ITEM01", result.Items[0].Symbol);
        Assert.Equal(150.00m, result.Items[0].TargetPrice);
        Assert.Equal("ITEM02", result.Items[1].Symbol);
        Assert.Equal(150.00m, result.Items[1].TargetPrice);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithRepeatedPrices_Returns150And200ForTarget350()
    {
        const decimal targetTotal = 350.00m;
        List<WatchlistItem> items =
        [
            new() { Symbol = "ITEM01", TargetPrice = 150.00m },
            new() { Symbol = "ITEM02", TargetPrice = 150.00m },
            new() { Symbol = "ITEM03", TargetPrice = 150.00m },
            new() { Symbol = "ITEM04", TargetPrice = 180.00m },
            new() { Symbol = "ITEM05", TargetPrice = 190.00m },
            new() { Symbol = "ITEM06", TargetPrice = 200.00m },
            new() { Symbol = "ITEM07", TargetPrice = 200.00m }
        ];
        var repository = new Mock<IWatchlistRepository>();
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        var result = await service.GetWatchlistItemsBestPairAsync(targetTotal);

        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal(targetTotal, result.CombinedTargetPrice);
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("ITEM01", result.Items[0].Symbol);
        Assert.Equal(150.00m, result.Items[0].TargetPrice);
        Assert.Equal("ITEM06", result.Items[1].Symbol);
        Assert.Equal(200.00m, result.Items[1].TargetPrice);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithRepeatedPrices_Returns160And170ForTarget330()
    {
        const decimal targetTotal = 330.00m;
        List<WatchlistItem> items =
        [
            new() { Symbol = "ITEM01", TargetPrice = 150.00m },
            new() { Symbol = "ITEM02", TargetPrice = 150.00m },
            new() { Symbol = "ITEM03", TargetPrice = 150.00m },
            new() { Symbol = "ITEM04", TargetPrice = 160.00m },
            new() { Symbol = "ITEM05", TargetPrice = 160.00m },
            new() { Symbol = "ITEM06", TargetPrice = 170.00m },
            new() { Symbol = "ITEM07", TargetPrice = 170.00m },
            new() { Symbol = "ITEM08", TargetPrice = 200.00m },
            new() { Symbol = "ITEM09", TargetPrice = 200.00m },
            new() { Symbol = "ITEM10", TargetPrice = 200.00m }
        ];
        var repository = new Mock<IWatchlistRepository>();
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        var result = await service.GetWatchlistItemsBestPairAsync(targetTotal);

        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("ITEM04", result.Items[0].Symbol);
        Assert.Equal(160.00m, result.Items[0].TargetPrice); 
        Assert.Equal("ITEM06", result.Items[1].Symbol);
        Assert.Equal(170.00m, result.Items[1].TargetPrice);
        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal(targetTotal, result.CombinedTargetPrice);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithNoItems_ReturnsNoMatchingPair()
    {
        IReadOnlyList<WatchlistItem> items = [];
        var repository = new Mock<IWatchlistRepository>();
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        var result = await service.GetWatchlistItemsBestPairAsync(100m);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Null(result.CombinedTargetPrice);
        Assert.Equal("No matching pair", result.Message);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithOneItem_ReturnsNoMatchingPair()
    {
        IReadOnlyList<WatchlistItem> items =
        [
            new() { Symbol = "AAPL", TargetPrice = 100m }
        ];
        var repository = new Mock<IWatchlistRepository>();
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        var result = await service.GetWatchlistItemsBestPairAsync(200m);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Null(result.CombinedTargetPrice);
        Assert.Equal("No matching pair", result.Message);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithSameFirstSymbolTie_ReturnsAlphabeticallyFirstPair()
    {
        const decimal targetTotal = 100m;
        IReadOnlyList<WatchlistItem> items =
        [
            new() { Symbol = "AAA", TargetPrice = 30m },
            new() { Symbol = "BBB", TargetPrice = 70m },
            new() { Symbol = "CCC", TargetPrice = 70m }
        ];
        var repository = new Mock<IWatchlistRepository>();
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        var result = await service.GetWatchlistItemsBestPairAsync(targetTotal);

        Assert.NotNull(result);
        Assert.Equal(targetTotal, result.CombinedTargetPrice);
        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal(["AAA", "BBB"], result.Items.Select(item => item.Symbol));
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithDuplicateHighestPrices_ReturnsThePairOfHighestPrices()
    {
        const decimal targetTotal = 1000.00m;
        IReadOnlyList<WatchlistItem> items =
        [
            new() { Symbol = "AAA", TargetPrice = 100.00m },
            new() { Symbol = "BBB", TargetPrice = 300.00m },
            new() { Symbol = "CCC", TargetPrice = 300.00m }
        ];
        var repository = new Mock<IWatchlistRepository>();
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        var result = await service.GetWatchlistItemsBestPairAsync(targetTotal);

        Assert.NotNull(result);
        Assert.Equal(600.00m, result.CombinedTargetPrice);
        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal(["BBB", "CCC"], result.Items.Select(item => item.Symbol));
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithTiedTotalsBelowTarget_ReturnsAlphabeticallyFirstPair()
    {
        const decimal targetTotal = 650.00m;
        IReadOnlyList<WatchlistItem> items =
        [
            new() { Symbol = "ZZZ", TargetPrice = 100.00m },
            new() { Symbol = "YYY", TargetPrice = 500.00m },
            new() { Symbol = "AAA", TargetPrice = 200.00m },
            new() { Symbol = "BBB", TargetPrice = 400.00m }
        ];
        var repository = new Mock<IWatchlistRepository>();
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        var result = await service.GetWatchlistItemsBestPairAsync(targetTotal);

        Assert.NotNull(result);
        Assert.Equal(600.00m, result.CombinedTargetPrice);
        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal(["AAA", "BBB"], result.Items.Select(item => item.Symbol));
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithCheapestItemAboveTarget_ReturnsEmptyNoMatchingPair()
    {
        const decimal targetTotal = 50.00m;
        IReadOnlyList<WatchlistItem> items =
        [
            new() { Symbol = "AAA", TargetPrice = 100.00m },
            new() { Symbol = "BBB", TargetPrice = 200.00m },
            new() { Symbol = "CCC", TargetPrice = 300.00m }
        ];
        var repository = new Mock<IWatchlistRepository>();
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        var result = await service.GetWatchlistItemsBestPairAsync(targetTotal);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Null(result.CombinedTargetPrice);
        Assert.Equal("No matching pair", result.Message);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithThreeItemsAtSamePriceBelowTarget_ReturnsAlphabeticallyFirstPair()
    {
        const decimal targetTotal = 170.00m;
        IReadOnlyList<WatchlistItem> items =
        [
            new() { Symbol = "AAA", TargetPrice = 80.00m },
            new() { Symbol = "BBB", TargetPrice = 80.00m },
            new() { Symbol = "CCC", TargetPrice = 80.00m }
        ];
        var repository = new Mock<IWatchlistRepository>();
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(items);
        var service = new WatchlistService(repository.Object);

        var result = await service.GetWatchlistItemsBestPairAsync(targetTotal);

        Assert.NotNull(result);
        Assert.Equal(160.00m, result.CombinedTargetPrice);
        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal(["AAA", "BBB"], result.Items.Select(item => item.Symbol));
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }
}