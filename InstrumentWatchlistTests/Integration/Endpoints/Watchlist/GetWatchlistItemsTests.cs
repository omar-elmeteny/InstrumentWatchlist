using System.Net;
using System.Net.Http.Json;
using Data.DTOs;
using InstrumentWatchlistTests.Integration.Infrastructure;

public class GetWatchlistItemsTests : IClassFixture<WatchlistWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetWatchlistItemsTests(WatchlistWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWatchlistItems_ReturnsOk()
    {
        var response = await _client.GetAsync("/watchlist-items");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWatchlistItems_ReturnsListOfItems()
    {
        var response = await _client.GetAsync("/watchlist-items");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<GetWatchlistItems>>();
        Assert.NotNull(items);
    }
}