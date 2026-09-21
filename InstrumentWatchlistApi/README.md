# Instrument Watchlist API

An ASP.NET Core Web API for adding instruments to a watchlist, listing saved items, and finding the pair of items with the highest combined target price that does not exceed a target amount.

The API uses EF Core's InMemory provider. Data is retained only while the application is running.

## Prerequisites

- .NET SDK 10.0 or later

## Build and run

From the repository root:

```bash
dotnet restore
dotnet build
```

Run the API using either command:

```bash
dotnet run
```

For hot reload during development:

```bash
DOTNET_USE_POLLING_FILE_WATCHER=1 dotnet watch
```

The development HTTP URL is `http://localhost:5245`.

Swagger UI is available at:

```text
http://localhost:5245/swagger
```

The OpenAPI document is available at:

```text
http://localhost:5245/openapi/v1.json
```

## Endpoints

The API endpoints are:

| Method | URL | Description |
| --- | --- | --- |
| `POST` | `/watchlist-items` | Add an item. |
| `GET` | `/watchlist-items` | List all items. |
| `GET` | `/watchlist-items/best-pair?targetTotal={amount}` | Find the closest target-price pair. |

## Verification requests

I used Swagger UI at `http://localhost:5245/swagger`.

### POST `/watchlist-items`

**Response types:** `201 Created`, `400 Bad Request`, and `409 Conflict`.

Response structures:

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

This endpoint has no request input, so it has no client-input validation cases.

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

## Best-pair logic

### Approach

The service loads all saved items and sorts them by `TargetPrice`. It then uses two pointers: `start` begins at the lowest price and `end` at the highest.

- If the `start` price exceeds `targetTotal`, then there is no qualifying pair because the smallest price already exceeds the `targetTotal`.
- If the pair total or the `end` price exceed `targetTotal` , `end` moves left to reduce the total.
- If the pair total qualifies, it becomes the current best when its total is greater than the best total found so far; then `start` moves right to search for a larger qualifying total.
- If no qualifying pair is found, the response contains an empty `items`, `combinedTargetPrice: null`, and the message `"No matching pair"`.

Sorting costs $O(n \log n)$ and the two-pointer scan costs $O(n)$, so the overall time complexity is $O(n \log n)$. This approach avoids checking every possible pair, which would take $O(n^2)$.

### Tie-breaking

For a qualifying pair, the two symbols are first ordered alphabetically. If another pair has the same combined target price as the current best pair, the service compares the first symbol of each ordered pair and retains the pair whose first symbol comes first alphabetically. Because each pair is already internally ordered, this returns the pair that comes first alphabetically.

## Assumptions and Additional behavior

- The best-pair response includes a `message` indicating whether a matching pair was found.
- Target prices and target totals are limited to two decimal places. Entering more decimal places is not practical in general when entering prices, so the API rejects them during validation.

## Issues encountered

`dotnet watch` initially stopped after startup because the Linux user had exhausted the `inotify` watcher-instance limit. Running `DOTNET_USE_POLLING_FILE_WATCHER=1 dotnet watch` uses polling and avoids that limit. `dotnet run` is also unaffected.

## Improvements with more time

- Add automated unit tests for exact matches, tie-breaking, no-match results, duplicate symbols, and validation.
- Add a relational database, migrations, a unique database index for symbols, and a database check constraint for positive prices for production use.
- Create a client project for this Web API project.

## Tools and resources used

- .NET SDK, ASP.NET Core, EF Core InMemory, and NuGet.
- VS Code and its C# Dev Kit tooling.
- Swagger UI and ASP.NET Core OpenAPI support for manual endpoint exploration.
- Git and GitHub for source control and repository hosting.
- GitHub Copilot, used as a development assistant for implementation guidance and code review.
- Google Search for documentation and troubleshooting.
