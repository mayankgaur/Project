# Order Discount Service

.NET 10 Web API and Razor UI for calculating customer order discounts.

## Prerequisites

- .NET 10 SDK

## How to Run

1. Restore dependencies and run unit tests:
   dotnet restore
   dotnet test

2. Start the web application:
   dotnet run --project src/OrderDiscount.Web

3. Access the application:
   - Razor UI: http://localhost:5000/ (append ?culture=de-DE for German or ?culture=en-US for English)
   - Calculate API: POST http://localhost:5000/api/v1/discounts/calculate
   - Supported Tiers API: GET http://localhost:5000/api/v1/discounts/customer-types

4. Postman:
   Import Postman/OrderDiscount.postman_collection.json to test valid discount tiers and validation error scenarios.

## Design Approach

The solution is split into four layers (`Domain`, `Application`, `Infrastructure`, and `Web`) to keep business rules isolated from HTTP, UI, and storage concerns:

- **Configurable Rules**: Discount rates (`Regular` 5%, `Premium` 10%, `VIP` 15%), rounding precision, and rounding modes are read from `appsettings.json` via `IOptionsMonitor<DiscountSettings>`, allowing rate updates without code changes.
- **Pluggable Persistence**: `DatabaseSettings:Enabled` defaults to `false`, routing writes to a no-op repository. Running `Database/init_schema.sql` and enabling the setting switches persistence to SQL Server.
- **Pluggable Authentication**: `AuthSettings:Mode` defaults to `Bypass` for local development. Switching to `OAuth` enables JWT Bearer validation against the configured provider (`Keycloak`, `EntraId`, or `Auth0`).
- **Localization & Logging**: Error messages and UI labels use `.resx` resource files (`en-US` and `de-DE`). Serilog writes structured, readable logs to the console and daily rolling files under `logs/`.

## Design Patterns Used

- **Strategy Pattern (`IDiscountStrategy`)**: Encapsulates discount calculation per customer tier. Adding a new tier requires only a new strategy class, DI registration, and a rate entry in `appsettings.json`—no changes to `OrderDiscountService`.
- **Resolver / Factory (`DiscountStrategyResolver`)**: Maps incoming customer types (case-insensitively) to their registered strategy implementation.
- **Null Object Pattern (`NoOpOrderRepository`)**: Safely satisfies the `IOrderRepository` dependency when database persistence is turned off.
- **Options Pattern (`IOptionsMonitor<T>`)**: Binds strongly typed configuration sections for discounts, rounding, database, and authentication.

## Assumptions

- Order amounts must be strictly greater than zero; zero or negative values return a `400 Bad Request` validation error.
- Customer type matching is case-insensitive (`vip`, `VIP`, and `Vip` resolve to the same strategy).
- All monetary calculations use `decimal` and round according to the configured `DecimalPlaces` and `MidpointRounding` mode.
