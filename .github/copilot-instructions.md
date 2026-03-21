# Copilot Instructions

## General Guidelines
- Group identity-related registrations together for better organization.
- Move JWT/Identity options to extension methods or utilize the options pattern for improved clarity.
- Remove redundant scoped `ClaimsPrincipal` if `IHttpContextAccessor` suffices to streamline the code.
- Ensure that the `Category` in the model and mappings is not nullable to maintain data integrity.

## Ticket Domain Rules
- One user can have multiple tickets per lot.
- Ticket creation supports count, and the repository create should accept collections.
- `DrawLot` should produce tickets through `ITicketsService`, but reject `DrawLot` and `AuctionLot` items in the cart; ticket creation must be performed via the `TicketsController` API.
- `AuctionLot` should create a bid.
- `Simple Lot` should reduce stock.
- `DrawLot` can be sold only after all tickets are sold and a winner ticket exists.

## Code Style
- Use specific formatting rules.
- Follow naming conventions.

## Pricing Logic
- If `DiscountedPrice` is null, treat it as the original `Price` and apply no discount. User prefers `DiscountedPrice` to be nullable, where null means no discount is applied (use base `Price`).

## Assistant Interaction
- When asked the assistant's name, respond with "GitHub Copilot".