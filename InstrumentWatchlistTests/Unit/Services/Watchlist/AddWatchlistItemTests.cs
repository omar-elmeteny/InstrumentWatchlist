using Data.DTOs;
using Data.Entities;
using Moq;
using Repositories;
using Services;

namespace InstrumentWatchlistTests.Unit.Services.Watchlist;

public class AddWatchlistItemTests
{
	[Fact]
	public async Task AddWatchlistItem_ValidWatchlistItem_AddsItemAndReturnsResponse()
	{
		var request = new CreateWatchlistItem
		{
			Symbol = "aapl",
			TargetPrice = 200.50m,
			Note = "Long-term position"
		};
		WatchlistItem? addedItem = null;
		var repository = new Mock<IWatchlistRepository>();
		repository
			.Setup(mock => mock.SymbolExistsAsync(request.Symbol))
			.ReturnsAsync(false);
		repository
			.Setup(mock => mock.AddAsync(It.IsAny<WatchlistItem>()))
			.Callback<WatchlistItem>(item => addedItem = item)
			.Returns(Task.CompletedTask);
		var service = new WatchlistService(repository.Object);

		var result = await service.AddWatchlistItemAsync(request);

		Assert.NotNull(result);
		Assert.Equal(request.Symbol.ToUpper(), result.Symbol);
		Assert.Equal(request.TargetPrice, result.TargetPrice);
		Assert.Equal(request.Note, result.Note);
		Assert.NotNull(addedItem);
		Assert.Equal(request.Symbol.ToUpper(), addedItem.Symbol);
		Assert.Equal(request.TargetPrice, addedItem.TargetPrice);
		Assert.Equal(request.Note, addedItem.Note);
		repository.Verify(mock => mock.SymbolExistsAsync(request.Symbol), Times.Once);
		repository.Verify(mock => mock.AddAsync(It.IsAny<WatchlistItem>()), Times.Once);
	}

    [Fact]
    public async Task AddWatchlistItem_WhenSymbolAlreadyExists_ReturnsNull()
    {
        var request = new CreateWatchlistItem
        {
            Symbol = "aapl",
            TargetPrice = 200.50m,
            Note = "Long-term position"
        };
        var repository = new Mock<IWatchlistRepository>();
        repository
            .Setup(mock => mock.SymbolExistsAsync(request.Symbol))
            .ReturnsAsync(true);
        var service = new WatchlistService(repository.Object);

        var result = await service.AddWatchlistItemAsync(request);

        Assert.Null(result);
        repository.Verify(mock => mock.SymbolExistsAsync(request.Symbol), Times.Once);
        repository.Verify(mock => mock.AddAsync(It.IsAny<WatchlistItem>()), Times.Never);
    }

    [Fact]
    public async Task AddWatchlistItem_WhenMixedCaseSymbolAlreadyExists_ReturnsNull()
    {
        var request = new CreateWatchlistItem
        {
            Symbol = "mSfT",
            TargetPrice = 200.50m,
            Note = "Long-term position"
        };
        var repository = new Mock<IWatchlistRepository>();
        repository
            .Setup(mock => mock.SymbolExistsAsync(request.Symbol))
            .ReturnsAsync(true);
        var service = new WatchlistService(repository.Object);

        var result = await service.AddWatchlistItemAsync(request);

        Assert.Null(result);
        repository.Verify(mock => mock.SymbolExistsAsync(request.Symbol), Times.Once);
        repository.Verify(mock => mock.AddAsync(It.IsAny<WatchlistItem>()), Times.Never);
    }

    [Fact]
    public async Task AddWatchlistItem_WhenNoteIsNull_AddsItemAndReturnsResponse()
    {
        var request = new CreateWatchlistItem
        {
            Symbol = "aapl",
            TargetPrice = 200.50m,
            Note = null
        };
        WatchlistItem? addedItem = null;
        var repository = new Mock<IWatchlistRepository>();
        repository
            .Setup(mock => mock.SymbolExistsAsync(request.Symbol))
            .ReturnsAsync(false);
        repository
            .Setup(mock => mock.AddAsync(It.IsAny<WatchlistItem>()))
            .Callback<WatchlistItem>(item => addedItem = item)
            .Returns(Task.CompletedTask);
        var service = new WatchlistService(repository.Object);

        var result = await service.AddWatchlistItemAsync(request);

        Assert.NotNull(result);
        Assert.Equal(request.Symbol.ToUpper(), result.Symbol);
        Assert.Equal(request.TargetPrice, result.TargetPrice);
        Assert.Null(result.Note);
        Assert.NotNull(addedItem);
        Assert.Equal(request.Symbol.ToUpper(), addedItem.Symbol);
        Assert.Equal(request.TargetPrice, addedItem.TargetPrice);
        Assert.Null(addedItem.Note);
        repository.Verify(mock => mock.SymbolExistsAsync(request.Symbol), Times.Once);
        repository.Verify(mock => mock.AddAsync(It.IsAny<WatchlistItem>()), Times.Once);
    }
}
