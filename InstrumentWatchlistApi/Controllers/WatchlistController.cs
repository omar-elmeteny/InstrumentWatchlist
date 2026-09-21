using Microsoft.AspNetCore.Mvc;
using Services;
using Data.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Controllers;


[ApiController]
[Route("watchlist-items")]
public class WatchlistController : ControllerBase
{
    public readonly IWatchlistService _watchlistService;

    public WatchlistController(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    [HttpPost]
    [AllowAnonymous]
    [EndpointName("CreateWatchlistItem")]
    [EndpointSummary("Creates a new watchlist item.")]
    [EndpointDescription("Adds a new instrument to the watchlist. Symbols are stored in uppercase and must be unique.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateWatchlistItemResponse>> CreateWatchlistItem(
        CreateWatchlistItem createWatchlistDTO)
    {
        var createdWatchlist = await _watchlistService.AddWatchlistItemAsync(createWatchlistDTO);

        if (createdWatchlist == null)
        {
            return Conflict("A watchlist with the same symbol already exists.");
        }

        return CreatedAtAction(nameof(GetWatchlistItems), createdWatchlist);
    }

    [HttpGet]
    [AllowAnonymous]
    [EndpointName("GetWatchlistItems")]
    [EndpointSummary("Retrieves all watchlist items.")]
    [EndpointDescription("Fetches the complete list of instruments in the watchlist.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GetWatchlistItems>>> GetWatchlistItems()
    {
        return Ok(await _watchlistService.GetAllWatchlistItemsAsync());
    }

    [HttpGet("best-pair")]
    [AllowAnonymous]
    [EndpointName("GetWatchlistItemsBestPair")]
    [EndpointSummary("Retrieves the best pair of watchlist items for a given target total.")]
    [EndpointDescription("Finds the pair of instruments in the watchlist whose combined target price is closest to the specified target total.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GetWatchlistItemsBestPair>> GetWatchlistItemsBestPair(
        [FromQuery] decimal? targetTotal
    )
    {
        if (!targetTotal.HasValue)
        {
            return BadRequest("Target total is required.");
        }

        if (targetTotal <= 0)
        {
            return BadRequest("Target total must be greater than zero.");
        }

        if (targetTotal != decimal.Round(targetTotal.Value, 2))
        {
            return BadRequest("Target total cannot have more than two decimal places.");
        }

        var bestPair = await _watchlistService.GetWatchlistItemsBestPairAsync(targetTotal.Value);

        return Ok(bestPair);
    }
}