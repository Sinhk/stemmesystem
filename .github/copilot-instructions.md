# Copilot Instructions

## Project Overview

Stemmesystem is a voting/ballot system (Norwegian: "stemming" = voting) for the Speidertinget (Scout Parliament). It is a full-stack .NET 6 application comprising:

- **Server**: ASP.NET Core 6 REST API with gRPC services, Duende IdentityServer for OAuth/OIDC, and SignalR hubs
- **Client**: Blazor WebAssembly frontend
- **Shared**: Shared models, interfaces, gRPC service definitions, and SignalR contracts
- **Data**: Entity Framework Core data layer targeting PostgreSQL

## Solution Structure

```
Stemmesystem.sln
├── Server/              # ASP.NET Core web API + host for Blazor WASM
├── Client/              # Blazor WebAssembly frontend
├── Shared/              # Shared models, gRPC contracts, SignalR definitions
├── Data/                # EF Core DbContext, entities, repositories, migrations
├── Stemmesystem.Server.Tests/   # Server integration tests (xUnit + Testcontainers)
├── Stemmesystem.Data.Tests/     # Data layer unit tests (xUnit)
└── Stemmesystem.Client.Tests/   # Client unit tests (xUnit)
```

## Build

```bash
dotnet restore
dotnet build
```

The solution targets **net6.0** and uses the SDK version pinned in `global.json` (`6.0.0` with `latestFeature` roll-forward).

## Testing

Tests use **xUnit** with **FluentAssertions**. Server integration tests use **Testcontainers** to spin up a real PostgreSQL container — Docker must be running locally to execute them.

```bash
# Run all tests
dotnet test

# Run a specific test project
dotnet test Stemmesystem.Server.Tests/
dotnet test Stemmesystem.Data.Tests/
dotnet test Stemmesystem.Client.Tests/

# Run with verbose output
dotnet test --verbosity normal
```

## Database

The application supports three database providers (controlled by the `"Provider"` key in `appsettings.json`): `Postgres`, `SqlServer`, and `Sqlite`.

For local development, start PostgreSQL via Docker Compose:

```bash
docker-compose up -d
```

Default Postgres credentials (see `docker-compose.yml`):
- Host: `localhost:5432`
- User: `stemme` (`POSTGRES_USER`)
- Password: see `POSTGRES_PASSWORD` in `docker-compose.yml`

### EF Core Migrations

Migrations are in `Data/Migrations/`. To add a new migration, use the PowerShell helper:

```powershell
.\Add-Migration.ps1 <MigrationName>
```

This generates migrations for Postgres, SQL Server, and SQLite providers simultaneously using the `--startup-project Server` and `--project` flags.

To apply migrations at runtime, the app calls `database.MigrateAsync()` on startup.

## Key Conventions

- **Language**: Norwegian is used for domain terminology (e.g., `Arrangement`, `Votering` (vote/ballot), `Delegat` (delegate), `Sak` (agenda item), `Stemme` (vote), `Gruppe` (group)).
- **Nullable reference types**: Enabled in all projects (`<Nullable>enable</Nullable>`).
- **Implicit usings**: Enabled in all projects.
- **Repository pattern**: Data access is abstracted behind interfaces in `Data/Repositories/` and `Server/` (e.g., `IArrangementRepository`).
- **AutoMapper**: Used for mapping between domain entities and API/view models. Profiles live in the respective project (e.g., `Server/ApiAutoMapperProfile.cs`).
- **gRPC**: Service contracts are defined using `protobuf-net.Grpc` (code-first, no `.proto` files). Contracts live in `Shared/`.
- **Authentication**: Duende IdentityServer with Google OAuth. Integration tests use a custom `TestAuthHandler` that bypasses Google auth.
- **Caching**: FusionCache (`ZiggyCreatures.FusionCache`) is used on the server.
- **Error handling**: LanguageExt (`LanguageExt.Core`) is used for functional error handling patterns.

## CI/CD

- **GitHub Actions** (`.github/workflows/dotnet.yml`): Builds and runs tests on push/PR to `master`.
- **GitLab CI** (`.gitlab-ci.yml`): Build, test, and deploy to Google Cloud Run on `master` (production).
