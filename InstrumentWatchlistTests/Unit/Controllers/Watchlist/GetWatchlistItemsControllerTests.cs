using Controllers;
using Data.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services;

namespace InstrumentWatchlistTests.Unit.Controllers;

public class GetWatchlistItemsControllerTests
{
    [Fact]
    public async Task GetWatchlistItems_WhenServiceReturnsItems_ReturnsOk()
    {
        IReadOnlyList<GetWatchlistItems> items =
        [
            new GetWatchlistItems
            {
                Symbol = "AAPL",
                TargetPrice = 200.50m,
                Note = "Long-term position"
            }
        ];
        var service = new Mock<IWatchlistService>();
        service
            .Setup(mock => mock.GetAllWatchlistItemsAsync())
            .ReturnsAsync(items);
        var controller = new WatchlistController(service.Object);

        var result = await controller.GetWatchlistItems();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(items, okResult.Value);
        service.Verify(mock => mock.GetAllWatchlistItemsAsync(), Times.Once);
    }
}