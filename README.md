# Instrument Watchlist

Instrument Watchlist is a multi-project solution for adding instruments to a watchlist, listing saved items, and finding the pair with the highest combined target price that does not exceed a target amount.

The completed API uses EF Core's InMemory provider, so data is retained only while the application is running.

## Solution Overview

This repository is organized into two projects:

- `InstrumentWatchlistApi`: the completed ASP.NET Core Web API and the required assessment deliverable.
- `InstrumentWatchlistTests`: a completed xUnit test project added beyond the assessment scope to provide unit and integration coverage.

## API Development

### Prerequisites

- .NET SDK 10.0 or later

### Build and Run

From the repository root:

```bash
dotnet restore
dotnet build
```

Run the API using either command:

```bash
dotnet run --project InstrumentWatchlistApi
```

For hot reload during development:

```bash
DOTNET_USE_POLLING_FILE_WATCHER=1 dotnet watch
```

The development HTTP URL is `http://localhost:5245`.

Swagger UI is available at:

<http://localhost:5245/swagger>

The OpenAPI document is available at:

<http://localhost:5245/openapi/v1.json>

## Testing

The test project includes:

- Unit tests for controller responses and service behavior using Moq to isolate dependencies.
- Integration tests that host the API with `WebApplicationFactory` and an isolated EF Core in-memory database for each endpoint test.

From the repository root, enter the test project and run all tests:

```bash
dotnet test
```

The test command restores packages and builds both the test project and its API project reference. The API does not need to be running because integration tests host it in memory through `WebApplicationFactory`.

## API Reference

### Endpoints

The API endpoints are:

| Method | URL | Description |
| --- | --- | --- |
| `POST` | `/watchlist-items` | Add an item. |
| `GET` | `/watchlist-items` | List all items. |
| `GET` | `/watchlist-items/best-pair?targetTotal={amount}` | Find the closest target-price pair. |

### Verification Requests

I used Swagger UI at `http://localhost:5245/swagger`.

### POST `/watchlist-items`

**Response types:** `201 Created`, `400 Bad Request`, and `409 Conflict`.

Response structure:

| Status | Response body | Meaning |
| --- | --- | --- |
| `201 Created` | Created-item object | Contains the created item's `id`, `symbol`, `targetPrice`, and `note`. |
| `400 Bad Request` | Validation-problem-details object | Contains standard problem-details fields and an `errors` object mapping field names to validation messages. |
| `409 Conflict` | JSON string | `"A watchlist with the same symbol already exists."` |

#### Valid: Lowercase symbol is normalized

Valid lowercase symbol. This verifies that the entity normalizes the symbol to uppercase:

```json
{
	"symbol": "msft",
	"targetPrice": 120.50,
	"note": "Microsoft"
}
```

Response: `201 Created`

```json
{
	"id": "<generated-id>",
	"symbol": "MSFT",
	"targetPrice": 120.50,
	"note": "Microsoft"
}
```

#### Valid: Optional note is omitted

Valid request without a note. `note` is optional:

```json
{
	"symbol": "NVDA",
	"targetPrice": 70
}
```

Response: `201 Created`

```json
{
	"id": "<generated-id>",
	"symbol": "NVDA",
	"targetPrice": 70,
	"note": null
}
```

#### Conflict: Duplicate symbol

Duplicate symbol after the first `msft` request:

```json
{
	"symbol": "MSFT",
	"targetPrice": 120.50
}
```

Response: `409 Conflict`

```json
"A watchlist with the same symbol already exists."
```

#### Invalid: Missing symbol

Missing symbol:

```json
{
	"targetPrice": 120.50,
	"note": "Microsoft"
}
```

Response: `400 Bad Request` with `Symbol is required.`

#### Invalid: Symbol exceeds 10 characters

Symbol longer than 10 characters:

```json
{
	"symbol": "TOO-LONG-SYMBOL",
	"targetPrice": 120.50,
	"note": "Microsoft"
}
```

Response: `400 Bad Request` with `Symbol must be between 1 and 10 characters.`

#### Invalid: Target price is zero

Target price of zero:

```json
{
	"symbol": "AAPL",
	"targetPrice": 0,
	"note": "Apple"
}
```

Response: `400 Bad Request` with `Target Price must be greater than 0.`

#### Invalid: Target price is negative

Negative target price:

```json
{
	"symbol": "AAPL",
	"targetPrice": -10,
	"note": "Apple"
}
```

Response: `400 Bad Request` with `Target Price must be greater than 0.`

#### Invalid: Missing target price

Missing target price:

```json
{
	"symbol": "AAPL",
	"note": "Apple"
}
```

Response: `400 Bad Request` with `Target Price must be greater than 0.`

#### Invalid: Target price has more than two decimal places

Target price with more than two decimal places:

```json
{
	"symbol": "AAPL",
	"targetPrice": 12.345,
	"note": "Apple"
}
```

Response: `400 Bad Request` with `Target Price cannot have more than two decimal places.`

#### Invalid: Note exceeds 250 characters

Note longer than 250 characters:

```json
{
	"symbol": "IBM",
	"targetPrice": 30,
	"note": "<251-character note>"
}
```

Response: `400 Bad Request` with `Note cannot exceed 250 characters.`

### GET `/watchlist-items`

**Response type:** `200 OK`.

Response structure:

| Status | Response body | Meaning |
| --- | --- | --- |
| `200 OK` | Array of watchlist-item objects | Each object contains `symbol`, `targetPrice`, and `note`. The array can be empty; `note` can be `null`. |

#### Valid: Empty watchlist

With no saved items, the response is a valid empty list:

```json
[]
```

#### Valid: Saved items are returned

After adding the lowercase `msft` item above, the response confirms uppercase storage:

```json
[
	{
		"symbol": "MSFT",
		"targetPrice": 120.50,
		"note": "Microsoft"
	}
]
```

This endpoint has no request input, so it has no validation cases.

### GET `/watchlist-items/best-pair?targetTotal={amount}`

**Response types:** `200 OK` and `400 Bad Request`.

The `200 OK` response is a best-pair result object:

| Property | Type | Meaning |
| --- | --- | --- |
| `items` | Array | Zero or two saved watchlist items. Each item contains `symbol`, `targetPrice`, and `note`; `note` can be `null`. |
| `combinedTargetPrice` | Decimal or `null` | The sum of the selected pair, or `null` when no pair qualifies. |
| `message` | String | Describes whether a pair was found. |

The concrete response examples below show the actual JSON returned for matching and no-match cases.

Populate the watchlist with `MSFT: 120`, `AAPL: 80`, `NVDA: 70`, and `IBM: 30` before these tests.

#### Valid: Exact match

Exact match, `targetTotal=200`:

```json
{
	"items": [
		{ "symbol": "AAPL", "targetPrice": 80, "note": null },
		{ "symbol": "MSFT", "targetPrice": 120, "note": null }
	],
	"combinedTargetPrice": 200,
	"message": "Matching pair found"
}
```

#### Valid: Closest qualifying pair

Closest qualifying pair, `targetTotal=155`:

```json
{
	"items": [
		{ "symbol": "AAPL", "targetPrice": 80, "note": null },
		{ "symbol": "NVDA", "targetPrice": 70, "note": null }
	],
	"combinedTargetPrice": 150,
	"message": "Matching pair found"
}
```

#### Valid: No qualifying pair

No qualifying pair, `targetTotal=90`, or fewer than two saved items:

```json
{
	"items": [],
	"combinedTargetPrice": null,
	"message": "No matching pair"
}
```

#### Valid: Target total exceeds every pair

With `MSFT: 120`, `AAPL: 80`, `NVDA: 70`, and `IBM: 30`, `targetTotal=5000` still returns the highest available qualifying pair:

```json
{
	"items": [
		{ "symbol": "AAPL", "targetPrice": 80, "note": null },
		{ "symbol": "MSFT", "targetPrice": 120, "note": null }
	],
	"combinedTargetPrice": 200,
	"message": "Matching pair found"
}
```

#### Valid: Alphabetical tie-break

With `AAA: 40`, `BBB: 60`, `CCC: 50`, and `DDD: 50`, both `AAA + BBB` and `CCC + DDD` total `100`. With `targetTotal=100`, the API returns the pair that comes first alphabetically:

```json
{
	"items": [
		{ "symbol": "AAA", "targetPrice": 40, "note": null },
		{ "symbol": "BBB", "targetPrice": 60, "note": null }
	],
	"combinedTargetPrice": 100,
	"message": "Matching pair found"
}
```

#### Invalid: Missing target total

Request: `GET /watchlist-items/best-pair`

Response: `400 Bad Request`

```json
"Target total is required."
```

#### Invalid: Target total is zero

Request: `GET /watchlist-items/best-pair?targetTotal=0`

Response: `400 Bad Request`

```json
"Target total must be greater than zero."
```

#### Invalid: Target total is negative

Request: `GET /watchlist-items/best-pair?targetTotal=-10`

Response: `400 Bad Request`

```json
"Target total must be greater than zero."
```

#### Invalid: Target total has more than two decimal places

Request: `GET /watchlist-items/best-pair?targetTotal=100.123`

Response: `400 Bad Request`

```json
"Target total cannot have more than two decimal places."
```

#### Invalid: Target total is non-numeric

Request: `GET /watchlist-items/best-pair?targetTotal=invalid`

Response: `400 Bad Request` with an ASP.NET Core validation-problem-details object. Its `errors` property contains a model-binding error for `targetTotal`.

## Best-Pair Logic

### Approach

The service uses two phases:

1. It checks whether the saved items are already ordered by `targetPrice`, then by `symbol` alphabetically when prices are equal. If not, it sorts them into that order.
2. It scans the ordered list with two pointers. `start` points to the lowest remaining price and `end` points to the highest remaining price. The pointers always refer to different items because the scan continues only while `start < end`.

The two-pointer scan follows these rules:

- If the `start` price exceeds `targetTotal`, then there is no qualifying pair because the smallest price already exceeds the `targetTotal`.
- If the pair total, or the `end` price alone, exceeds `targetTotal`, `end` moves left to try a smaller price.
- If the pair qualifies and has a higher total than the current best pair, it becomes the new best pair. The service then moves a pointer to continue the search, including when prices are repeated.
- If no qualifying pair is found, the response contains an empty `items`, `combinedTargetPrice: null`, and the message `"No matching pair"`.

For unordered input, the order check costs $O(n)$, sorting costs $O(n \log n)$, and the scan costs $O(n)$, so the overall complexity is $O(n \log n)$. For already ordered input, sorting is skipped; the order check and scan are both $O(n)$, so the total is $O(n)$. A nested-loop approach still takes $O(n^2)$ to evaluate every pair, regardless of input order.

| Input state | Order check | Sort | Pointer scan | Total complexity |
| --- | --- | --- | --- | --- |
| Already ordered by price, then symbol | $O(n)$ | Skipped | $O(n)$ | $O(n)$ |
| Unordered | $O(n)$ | $O(n \log n)$ | $O(n)$ | $O(n \log n)$ |
| Nested-loop comparison | Not needed | Not needed | Checks every pair | $O(n^2)$ |

The table below illustrates how the two approaches grow. The values are approximate operation counts, not measured execution times.

| Watchlist items | Nested-loop approach $O(n^2)$ | Sort-and-scan approach $O(n \log n)$ |
| ---: | ---: | ---: |
| 1,000 | about 500,000 pair checks | about 11,000 sort-and-scan operations |
| 100,000 | about 5 billion pair checks | about 1.8 million sort-and-scan operations |
| 1,000,000 | about 500 billion pair checks | about 21 million sort-and-scan operations |

### Tie-breaking

Before scanning, the service orders saved items by price from lowest to highest. Items with the same price are ordered by symbol. This gives the scan a consistent order when prices are repeated.

When the service considers a pair, it orders the two symbols inside that pair alphabetically. For example, a pair containing `MSFT` and `AAPL` is treated as `AAPL + MSFT`.

If two pairs have the same best combined price, the service keeps the pair that comes first alphabetically. For example, `AAA + BBB` is selected before `CCC + DDD`.

If the first symbol is the same in both pairs, the service compares the second symbol. For example, if `AAA + CCC` was selected first and `AAA + BBB` is considered later, the service selects `AAA + BBB`.

## Assumptions and Additional Behavior

- The best-pair response includes a `message` indicating whether a matching pair was found.
- Target prices and target totals are limited to two decimal places. The API rejects values with more decimal places during validation.

## Issues Encountered

The best-pair logic initially contained several edge-case bugs. Writing unit tests exposed them. After fixing one issue, I would think of another combination of prices or symbols and discover a new failure. I spent several hours iterating between new test cases and service fixes, particularly around repeated prices and alphabetical tie-breaking. This process improved both the implementation and the test coverage.

## Improvements with More Time

- Add a relational database, migrations, a unique database index for symbols, and a database check constraint for positive prices for production use.
- Implement an Angular client project.

## Tools and Resources Used

- .NET SDK, ASP.NET Core, EF Core InMemory, and NuGet.
- VS Code and its C# Dev Kit tooling.
- Swagger UI and ASP.NET Core OpenAPI support for manual endpoint exploration.
- xUnit for unit and integration tests.
- Moq for mocking service and repository dependencies in unit tests.
- Microsoft.AspNetCore.Mvc.Testing and `WebApplicationFactory` for API integration tests.
- Git and GitHub for source control and repository hosting.
- GitHub Copilot, used as a development assistant for implementation guidance and code review.
- Google Search for documentation and troubleshooting.
