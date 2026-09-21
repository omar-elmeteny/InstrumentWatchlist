using Controllers;
using Data.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services;

namespace InstrumentWatchlistTests.Unit.Controllers;

public class GetWatchlistItemsBestPairControllerTests
{
    [Fact]
    public async Task GetWatchlistItemsBestPair_WithValidTargetTotal_ReturnsOk()
    {
        const decimal targetTotal = 300m;
        var bestPair = new GetWatchlistItemsBestPair
        {
            CombinedTargetPrice = targetTotal,
            Message = "Matching pair found"
        };
        var service = new Mock<IWatchlistService>();
        service
            .Setup(mock => mock.GetWatchlistItemsBestPairAsync(targetTotal))
            .ReturnsAsync(bestPair);
        var controller = new WatchlistController(service.Object);

        var result = await controller.GetWatchlistItemsBestPair(targetTotal);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(bestPair, okResult.Value);
        service.Verify(mock => mock.GetWatchlistItemsBestPairAsync(targetTotal), Times.Once);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithoutTargetTotal_ReturnsBadRequest()
    {
        var service = new Mock<IWatchlistService>();
        var controller = new WatchlistController(service.Object);

        var result = await controller.GetWatchlistItemsBestPair(null);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Target total is required.", badRequestResult.Value);
        service.Verify(
            mock => mock.GetWatchlistItemsBestPairAsync(It.IsAny<decimal>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetWatchlistItemsBestPair_WithNonPositiveTargetTotal_ReturnsBadRequest(
        decimal targetTotal)
    {
        var service = new Mock<IWatchlistService>();
        var controller = new WatchlistController(service.Object);

        var result = await controller.GetWatchlistItemsBestPair(targetTotal);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Target total must be greater than zero.", badRequestResult.Value);
        service.Verify(
            mock => mock.GetWatchlistItemsBestPairAsync(It.IsAny<decimal>()),
            Times.Never);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithMoreThanTwoDecimalPlaces_ReturnsBadRequest()
    {
        var service = new Mock<IWatchlistService>();
        var controller = new WatchlistController(service.Object);

        var result = await controller.GetWatchlistItemsBestPair(100.123m);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Target total cannot have more than two decimal places.", badRequestResult.Value);
        service.Verify(
            mock => mock.GetWatchlistItemsBestPairAsync(It.IsAny<decimal>()),
            Times.Never);
    }
}