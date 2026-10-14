# Dilovi Condominium Application

Backend for calculating residents' monthly payments for the "Dilovi" condominium (ОСББ):
water meter readings, tariffs, additional charges and a monthly payment statement per apartment.

## Features

| Status | Feature |
|---|---|
| ✅ | JWT authentication with BCrypt password hashing and role-based authorization |
| ✅ | Apartments, water meter readings (including meter replacement) |
| ✅ | Tariffs with validity periods (mid-month tariff changes are prorated by days) |
| ✅ | Additional one-off charges and discounts |
| ✅ | Monthly payment statement calculation |
| 🔜 | User registration |
| 🔜 | Integration with PrivatBank |

## Tech stack

- .NET 10, ASP.NET Core Web API, Swagger
- EF Core 10 + PostgreSQL 16 (Npgsql)
- MediatR (CQRS), FluentValidation, Serilog
- xUnit + Moq

## Architecture

```
src/
  Domain/                 Entities, value objects (BillingPeriod, Money) and billing logic. No dependencies.
  Application/            Commands/queries (MediatR), validators, repository abstractions
  Infrastructure/         EF Core DbContext and configurations, repositories, JWT, BCrypt
  Migrations.PostgreSQL/  EF Core migrations
  Condominium/            Web API: controllers, middleware, composition root
tests/
  Domain.UnitTests/       Billing rules
  Application.UnitTests/  Handlers, validators, pipeline behavior
```

Dependencies point inwards: `Condominium → Infrastructure → Application → Domain`.

### How a payment is calculated

`PaymentCalculator` (Domain) builds a statement for an apartment and a month:

1. Tariffs valid in the month are grouped by service (cold water, hot water, building maintenance, waste collection).
2. `TariffSchedule` returns the day-weighted price. If the tariff changes on the 16th, the first 15 days use the old price and the rest use the new one. Gaps and overlapping tariffs are rejected.
3. The quantity comes from the charge strategy that matches the tariff's `ChargeBasis` (**Strategy pattern**):
   - `MeterConsumptionChargeStrategy`: consumption from `ConsumptionCalculator`
   - `AreaChargeStrategy`: apartment area (m²)
   - `PerResidentChargeStrategy`: number of residents
4. Additional charges of the month are added. Each line is rounded to kopecks (`decimal`, away from zero), and the total is their sum.

`ConsumptionCalculator` takes the last reading before the month as the baseline. The first reading of a new meter (`IsNewMeter`) starts a new sequence, so a meter replacement does not produce a negative consumption. A decreasing reading without replacement is rejected.

## Getting started

### Prerequisites

- .NET 10 SDK
- Docker
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### 1. Database

```bash
echo POSTGRES_PASSWORD=<your-password> > .env
docker compose up -d db
```

PostgreSQL listens on `localhost:5435`, and data is stored in `environment/postgres-data`.

### 2. Configuration

Copy `src/Condominium/appsettings.Development.example.json` to `appsettings.Development.json` (it is git-ignored) and set the connection string.
Keep secrets in user-secrets:

```bash
cd src/Condominium
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "<at-least-32-characters>"
dotnet user-secrets set "DevelopmentSeed:AdminEmail" "admin@dilovi.local"
dotnet user-secrets set "DevelopmentSeed:AdminPassword" "<password>"
```

The application does not start when `JwtSettings` are missing or invalid.

### 3. Run

```bash
dotnet run --project src/Condominium
```

In Development the app applies pending migrations and creates the admin user from `DevelopmentSeed` on start.
Swagger UI is at the root URL. Call `POST /api/users/login`, then use **Authorize** with the returned token.

### Tests

```bash
dotnet test src/Condominium/Condominium.slnx
```

## API

| Method | Route | Role |
|---|---|---|
| POST | `/api/users/login` | anonymous |
| POST | `/api/apartments` | Admin, SuperAdmin |
| POST | `/api/apartments/{id}/meter-readings` | Admin, SuperAdmin |
| POST | `/api/apartments/{id}/additional-charges` | Admin, SuperAdmin |
| GET | `/api/apartments/{id}/payments/{year}/{month}` | any authenticated user |
| POST | `/api/tariffs` | Admin, SuperAdmin |

Errors: `400` validation, `401` authentication, `404` not found, `422` business rule violation.

Example statement (`GET /api/apartments/{id}/payments/2026/8`):

```json
{
  "apartmentId": "5d7556f2-adfb-4454-be2f-7394f55893d9",
  "period": "2026-08",
  "lines": [
    { "description": "ColdWater", "quantity": 12.5, "unit": "m³", "unitPrice": 33.0968, "amount": 413.71 },
    { "description": "HotWater", "quantity": 5.0, "unit": "m³", "unitPrice": 110.0, "amount": 550.0 },
    { "description": "BuildingMaintenance", "quantity": 54.3, "unit": "m²", "unitPrice": 8.5, "amount": 461.55 },
    { "description": "WasteCollection", "quantity": 3, "unit": "person", "unitPrice": 45.0, "amount": 135.0 },
    { "description": "Roof repair fund", "quantity": null, "unit": null, "unitPrice": null, "amount": 200.0 }
  ],
  "total": 1760.26
}
```
