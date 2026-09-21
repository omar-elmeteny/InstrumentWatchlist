using Data.DTOs;
using InstrumentWatchlistTests.Integration.Infrastructure;
using System.Net;
using System.Net.Http.Json;

public class GetWatchlistItemsBestPairTests : IDisposable
{
    private readonly WatchlistWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public GetWatchlistItemsBestPairTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithExactMatch_ReturnsMatchingPair()
    {
        await AddItemsAsync("EX");

        var result = await GetBestPairAsync(200);

        Assert.Equal(200m, result.CombinedTargetPrice);
        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal(["EXAAPL", "EXMSFT"], result.Items.Select(item => item.Symbol));
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithClosestQualifyingPair_ReturnsMatchingPair()
    {
        await AddItemsAsync("CL");

        var result = await GetBestPairAsync(155);

        Assert.Equal(150m, result.CombinedTargetPrice);
        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal(["CLAAPL", "CLNVDA"], result.Items.Select(item => item.Symbol));
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithNoQualifyingPair_ReturnsNoMatch()
    {
        await AddItemsAsync("NM");

        var result = await GetBestPairAsync(90);

        Assert.Empty(result.Items);
        Assert.Null(result.CombinedTargetPrice);
        Assert.Equal("No matching pair", result.Message);
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithTargetAboveEveryPair_ReturnsHighestPair()
    {
        await AddItemsAsync("HI");

        var result = await GetBestPairAsync(5000);

        Assert.Equal(200m, result.CombinedTargetPrice);
        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal(["HIAAPL", "HIMSFT"], result.Items.Select(item => item.Symbol));
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithEqualPairs_ReturnsAlphabeticallyFirstPair()
    {
        await AddAsync(
            ("TAAA", 40m),
            ("TBBB", 60m),
            ("TCCC", 50m),
            ("TDDD", 50m));

        var result = await GetBestPairAsync(100);

        Assert.Equal(100m, result.CombinedTargetPrice);
        Assert.Equal("Matching pair found", result.Message);
        Assert.Equal(["TAAA", "TBBB"], result.Items.Select(item => item.Symbol));
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithoutTargetTotal_ReturnsBadRequestWithMessage()
    {
        var response = await _client.GetAsync("/watchlist-items/best-pair");

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "Target total is required.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task GetWatchlistItemsBestPair_WithNonPositiveTargetTotal_ReturnsBadRequestWithMessage(decimal targetTotal)
    {
        var response = await _client.GetAsync($"/watchlist-items/best-pair?targetTotal={targetTotal}");

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "Target total must be greater than zero.");
    }

    [Fact]
    public async Task GetWatchlistItemsBestPair_WithTooManyDecimalPlaces_ReturnsBadRequestWithMessage()
    {
        var response = await _client.GetAsync("/watchlist-items/best-pair?targetTotal=100.123");

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "Target total cannot have more than two decimal places.");
    }

    private async Task AddItemsAsync(string prefix)
    {
        await AddAsync(
            ($"{prefix}MSFT", 120m),
            ($"{prefix}AAPL", 80m),
            ($"{prefix}NVDA", 70m),
            ($"{prefix}IBM", 30m));
    }

    private async Task AddAsync(params (string Symbol, decimal TargetPrice)[] items)
    {
        foreach (var item in items)
        {
            var response = await _client.PostAsJsonAsync("/watchlist-items", new CreateWatchlistItem
            {
                Symbol = item.Symbol,
                TargetPrice = item.TargetPrice
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }

    private async Task<GetWatchlistItemsBestPair> GetBestPairAsync(decimal targetTotal)
    {
        var response = await _client.GetAsync($"/watchlist-items/best-pair?targetTotal={targetTotal}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetWatchlistItemsBestPair>();
        return Assert.IsType<GetWatchlistItemsBestPair>(result);
    }

    private static async Task AssertErrorAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatusCode,
        string expectedMessage)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal(expectedMessage, await response.Content.ReadAsStringAsync());
    }
}