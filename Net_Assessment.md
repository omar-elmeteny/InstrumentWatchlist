ARQAAM CAPITAL  

# .NET Developer Technical Assessment

## Instrument Watchlist API

Build a small ASP.NET Core Web API that stores instruments a user wants to watch. We are interested in a working, understandable solution and the choices you make. The data may be stored in memory and may be lost when the application restarts.

## Working conditions

| Item | Requirement |
| --- | --- |
| Deadline | Submit your solution anytime before **4 PM on Tuesday the 22nd of September**. |
| Technology | .NET 10 or later and ASP.NET Core Web API. |
| Storage | An in-memory collection is sufficient. A database is not required. |
| Submission method | Push the solution to a **public Git repository** and send us the repository link. Do not submit the solution as a file attachment. The repository must be accessible without authentication. |
| Required contents | Source code and a short README. |
| Tools | Documentation, development assistants, and AI tools may be used. You must be able to explain all submitted work and disclose the tools used in the README. |

## Watchlist item

| Field | Rule |
| --- | --- |
| `symbol` | Required. Store and return it in uppercase. Maximum 10 characters. Example: `MSFT`. |
| `targetPrice` | Required decimal value greater than zero. |
| `note` | Optional text with a maximum of 250 characters. |

## Required behavior

### Add an item

Implement `POST /watchlist-items`.

- Accept `symbol`, `targetPrice`, and `note` in JSON.
- Return a useful validation response when the input is invalid.
- Return `201 Created` when the item is added.
- Do not allow the same symbol twice, even when the casing differs. Return `409 Conflict` for a duplicate.

### List items

Implement `GET /watchlist-items`.

- Return every saved item.
- Return an empty list when no items exist.

### Find the closest target-price pair — critical-thinking requirement

Implement `GET /watchlist-items/best-pair?targetTotal={amount}`.

From the saved watchlist, select **two different items** whose `targetPrice` values produce the best combined total:

- The combined value must not exceed the supplied `targetTotal`.
- The combined value should be as close as possible to `targetTotal`. In other words, select the qualifying pair with the highest combined `targetPrice`.
- The same saved item cannot be selected twice.

For this requirement, each watchlist item represents one symbol and contributes its stored `targetPrice` exactly once. Do not introduce quantities or calculate `quantity × price`.

Additional rules:

- `targetTotal` is required and must be a decimal value greater than zero.
- Return both selected items and a `combinedTargetPrice` containing their total.
- If an exact match exists, return it.
- If more than one pair has the same best combined value, sort the symbols within each pair alphabetically, then return the pair that comes first alphabetically.
- When fewer than two items exist, or when no pair has a combined value less than or equal to `targetTotal`, return `200 OK` with:
  - An empty `items` array.
  - `combinedTargetPrice` set to `null`.
  - The message `"No matching pair"`.
- Do not change the saved watchlist while calculating the result.

#### Examples

Assume the watchlist contains:

| Symbol | `targetPrice` |
| --- | ---: |
| `MSFT` | 120 |
| `AAPL` | 80 |
| `NVDA` | 70 |
| `IBM` | 30 |

##### Exact match

`targetTotal=200` returns `AAPL` and `MSFT` with `combinedTargetPrice=200` because their combined value exactly matches `targetTotal`.

##### Closest qualifying match

`targetTotal=155` returns `AAPL` and `NVDA` with `combinedTargetPrice=150`.

Other combinations either exceed `targetTotal` or have a lower combined value, so 150 is the closest qualifying result.

##### No matching pair

`targetTotal=90` returns `200 OK` with:

```json
{
  "items": [],
  "combinedTargetPrice": null,
  "message": "No matching pair"
}
```

Even the pair with the lowest combined value, `IBM` and `NVDA`, has a combined `targetPrice` of 100, which exceeds `targetTotal`.

##### Target total greater than every available pair

`targetTotal=5000` returns `MSFT` and `AAPL` with `combinedTargetPrice=200`.

Although 200 is much lower than 5000, it is the highest combined value available without exceeding `targetTotal`.

##### Tie between multiple pairs

Assume the watchlist contains:

| Symbol | `targetPrice` |
| --- | ---: |
| `AAA` | 40 |
| `BBB` | 60 |
| `CCC` | 50 |
| `DDD` | 50 |

With `targetTotal=100`, both `AAA + BBB` and `CCC + DDD` have a combined value of 100.

Return `AAA + BBB` because, after sorting the symbols within each pair, it is the pair that comes first alphabetically.

**There is room for improvement beyond the required scope.**

## Validation requirements

The API must validate submitted data and return clear, appropriate responses. At minimum, demonstrate that:

- A missing, empty, or whitespace-only symbol is rejected.
- A symbol longer than 10 characters is rejected.
- A zero or negative target price is rejected.
- A note longer than 250 characters is rejected.
- A duplicate symbol with different casing is rejected with `409 Conflict`.
- A missing, zero, or negative `targetTotal` for the best-pair endpoint is rejected.

## README requirements

Include the following in the README:

- Exact commands to build and run the API.
- Example requests or steps used to verify the required behavior and validations.
- Any assumptions you made.
- One issue you encountered and how you worked through it, if applicable.
- What you would improve if you had more time.
- A brief explanation of how your best-pair logic works, including how you handled ties and cases where no pair qualifies.
- A list of all tools and resources used, including documentation, search engines, IDE features or extensions, development assistants, code generators, and AI tools.


## What we are assessing

- Whether the required behavior works.
- Whether the code is easy to follow.
- How you validate input and handle errors.
- How you translate the best-pair rules into correct, understandable code and handle edge cases.
- How you investigate problems and explain your reasoning.
- How clearly and honestly you describe your use of tools and AI.

## Submission checklist

- [✓] The solution builds from a clean checkout.
- [✓] The build and run commands are included.
- [✓] All tools, resources, and AI usage are disclosed.
- [✓] No credentials, secrets, personal data, or confidential company information are included.
- [✓] Known limitations and unfinished work are stated plainly.
- [✓] The repository is public and accessible without authentication.

## Review discussion

Be prepared to run the API, demonstrate its validation and best-pair behavior, and explain a decision or problem you encountered. You may be asked how you would approach a small new requirement. Asking clarifying questions is encouraged.
