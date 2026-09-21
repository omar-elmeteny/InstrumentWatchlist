using Controllers;
using Data.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services;

namespace InstrumentWatchlistTests.Unit.Controllers;

public class CreateWatchlistItemControllerTests
{
    [Fact]
    public async Task CreateWatchlistItem_WhenServiceCreatesItem_ReturnsCreatedAtAction()
    {
        var request = new CreateWatchlistItem
        {
            Symbol = "AAPL",
            TargetPrice = 200.50m,
            Note = "Long-term position"
        };
        var createdItem = new CreateWatchlistItemResponse
        {
            Id = Guid.NewGuid().ToString(),
            Symbol = request.Symbol,
            TargetPrice = request.TargetPrice,
            Note = request.Note
        };
        var service = new Mock<IWatchlistService>();
        service
            .Setup(mock => mock.AddWatchlistItemAsync(request))
            .ReturnsAsync(createdItem);
        var controller = new WatchlistController(service.Object);

        var result = await controller.CreateWatchlistItem(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(WatchlistController.GetWatchlistItems), createdResult.ActionName);
        Assert.Same(createdItem, createdResult.Value);
        service.Verify(mock => mock.AddWatchlistItemAsync(request), Times.Once);
    }

    [Fact]
    public async Task CreateWatchlistItem_WhenServiceReturnsNull_ReturnsConflict()
    {
        var request = new CreateWatchlistItem
        {
            Symbol = "AAPL",
            TargetPrice = 200.50m
        };
        var service = new Mock<IWatchlistService>();
        service
            .Setup(mock => mock.AddWatchlistItemAsync(request))
            .ReturnsAsync((CreateWatchlistItemResponse?)null);
        var controller = new WatchlistController(service.Object);

        var result = await controller.CreateWatchlistItem(request);

        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal("A watchlist with the same symbol already exists.", conflictResult.Value);
        service.Verify(mock => mock.AddWatchlistItemAsync(request), Times.Once);
    }
}