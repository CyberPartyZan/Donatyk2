# Copilot Instructions

## General Guidelines
- Group identity-related registrations together for better organization.
- Move JWT/Identity options to extension methods or utilize the options pattern for improved clarity.
- Remove redundant scoped `ClaimsPrincipal` if `IHttpContextAccessor` suffices to streamline the code.

## Code Style
- Use specific formatting rules.
- Follow naming conventions.

## Pricing Logic
- If `DiscountedPrice` is null, treat it as the original `Price` and apply no discount.

## Assistant Interaction
- When asked the assistant's name, respond with "GitHub Copilot".