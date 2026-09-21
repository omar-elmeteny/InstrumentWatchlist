using InstrumentWatchlistTests.Integration.Infrastructure;
using Data.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

public class CreateWatchlistItemTests : IClassFixture<WatchlistWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CreateWatchlistItemTests(WatchlistWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateWatchlist_WithValidItem_WithLowercaseSymbol_ReturnsCreatedItem()
    {
        var request = new CreateWatchlistItem
        {
            Symbol = "msft",
            TargetPrice = 200.50m,
            Note = "Long-term position"
        };

        var response = await _client.PostAsJsonAsync("/watchlist-items", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdItem = await response.Content.ReadFromJsonAsync<CreateWatchlistItemResponse>();
        Assert.NotNull(createdItem);
        Assert.False(string.IsNullOrWhiteSpace(createdItem.Id));
        Assert.Equal(request.Symbol.ToUpperInvariant(), createdItem.Symbol);
        Assert.Equal(request.TargetPrice, createdItem.TargetPrice);
        Assert.Equal(request.Note, createdItem.Note);
    }

    [Fact]
    public async Task CreateWatchlist_WithDuplicateSymbol_ReturnsConflict()
    {
        var request = new CreateWatchlistItem
        {
            Symbol = "dupl",
            TargetPrice = 200.50m,
            Note = "Long-term position"
        };

        var response1 = await _client.PostAsJsonAsync("/watchlist-items", request);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

        var response2 = await _client.PostAsJsonAsync("/watchlist-items", request);
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);

        var message = await response2.Content.ReadAsStringAsync();
        Assert.Equal("A watchlist with the same symbol already exists.", message);
    }

    [Fact]
    public async Task CreateWatchlist_WithInvalidItem_WithMissingSymbol_ReturnsBadRequest()
    {
        var request = new CreateWatchlistItem
        {
            TargetPrice = 200.50m,
            Note = "Long-term position"
        };

        var response = await _client.PostAsJsonAsync("/watchlist-items", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationErrorAsync(response, nameof(CreateWatchlistItem.Symbol), "Symbol is required.");
    }

    [Fact]
    public async Task CreateWatchlist_WithInvalidItem_WithSymbolExceedingMaxLength_ReturnsBadRequest()
    {
        var request = new CreateWatchlistItem
        {
            Symbol = "AAABBBCCCDDD",
            TargetPrice = 200.50m,
            Note = "Long-term position"
        };

        var response = await _client.PostAsJsonAsync("/watchlist-items", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationErrorAsync(response, nameof(CreateWatchlistItem.Symbol), "Symbol must be between 1 and 10 characters.");
    }

    [Fact]
    public async Task CreateWatchlist_WithInvalidItem_WithMissingTargetPrice_ReturnsBadRequest()
    {
        var request = new CreateWatchlistItem
        {
            Symbol = "nvda",
            Note = "Long-term position"
        };

        var response = await _client.PostAsJsonAsync("/watchlist-items", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationErrorAsync(response, nameof(CreateWatchlistItem.TargetPrice), "Target Price must be greater than 0.");
    }

    [Fact]
    public async Task CreateWatchlist_WithInvalidItem_WithNegativeTargetPrice_ReturnsBadRequest()
    {
        var request = new CreateWatchlistItem
        {
            Symbol = "goog",
            TargetPrice = -100.00m,
            Note = "Long-term position"
        };

        var response = await _client.PostAsJsonAsync("/watchlist-items", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationErrorAsync(response, nameof(CreateWatchlistItem.TargetPrice), "Target Price must be greater than 0.");
    }

     [Fact]
    public async Task CreateWatchlist_WithInvalidItem_WithZeroTargetPrice_ReturnsBadRequest()
    {
        var request = new CreateWatchlistItem
        {
            Symbol = "tsla",
            TargetPrice = 0.00m,
            Note = "Long-term position"
        };

        var response = await _client.PostAsJsonAsync("/watchlist-items", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationErrorAsync(response, nameof(CreateWatchlistItem.TargetPrice), "Target Price must be greater than 0.");
    }

    [Fact]
    public async Task CreateWatchlist_WithInvalidItem_WithMissingNote_ReturnsCreatedItem()
    {
        var request = new CreateWatchlistItem
        {
            Symbol = "amzn",
            TargetPrice = 200.50m
        };

        var response = await _client.PostAsJsonAsync("/watchlist-items", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdItem = await response.Content.ReadFromJsonAsync<CreateWatchlistItemResponse>();

        Assert.NotNull(createdItem);
        Assert.Equal(request.Symbol.ToUpperInvariant(), createdItem.Symbol);
        Assert.Equal(request.TargetPrice, createdItem.TargetPrice);
        Assert.Null(createdItem.Note);
    }

    [Fact]
    public async Task CreateWatchlist_WithInvalidItem_WithNoteExceedingMaxLength_ReturnsBadRequest()
    {
        var request = new CreateWatchlistItem
        {
            Symbol = "meta",
            TargetPrice = 200.50m,
            Note = new string('a', 300)
        };

        var response = await _client.PostAsJsonAsync("/watchlist-items", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationErrorAsync(response, nameof(CreateWatchlistItem.Note), "Note cannot exceed 250 characters.");
    }

    private static async Task AssertValidationErrorAsync(
        HttpResponseMessage response,
        string propertyName,
        string expectedMessage)
    {
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.True(problem.Errors.TryGetValue(propertyName, out var errors));
        Assert.Contains(expectedMessage, errors);
    }
}