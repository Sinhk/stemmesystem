# Copilot Instructions for Stemmesystem

## Project Overview

Stemmesystem is a full-stack **voting/election management system** for Scouting organizations. It integrates with the Norwegian Scout organization system ("Min Speiding") and supports real-time voting with anonymous (secret) and public ballot types.

**Architecture:**
- **Backend:** ASP.NET Core 6.0 Web API + Duende IdentityServer 6.0
- **Frontend:** Blazor WebAssembly (C#)
- **Communication:** gRPC (code-first via ProtoBuf.Net.Grpc) + SignalR for real-time updates
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core 6.0

## Build & Test

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build --no-restore

# Run all tests (requires Docker for Testcontainers/PostgreSQL integration tests)
dotnet test --no-build --verbosity normal
```

Tests use **xUnit** with **FluentAssertions**. Integration tests in `Stemmesystem.Server.Tests` spin up a real PostgreSQL instance via **Testcontainers** — Docker must be running.

## Domain Terminology

The domain uses Norwegian terms throughout the codebase. Do not translate them — keep them as-is:

| Term | Meaning |
|------|---------|
| `Arrangement` | Top-level voting event / assembly |
| `Sak` | Case / matter being voted on |
| `Votering` | Individual ballot / vote round |
| `Valg` | Voting option / choice |
| `Delegat` | Delegate / voter |
| `Stemme` | Vote cast |
| `Gruppe` | Group / classification |
| `Delegatkode` | Unique code issued to each delegate to cast votes |
| `TilStede` | Present / in attendance |
| `Hemmelig` | Secret (as in secret ballot) |
| `Aktiv` | Active (e.g. an active voting round) |

## Project Structure

```
/Server        - ASP.NET Core API, gRPC services, SignalR hubs, IdentityServer
/Client        - Blazor WebAssembly frontend
/Shared        - gRPC interfaces, DTOs, SignalR hub interfaces, shared utilities
/Data          - EF Core DbContext, domain entities, repositories, migrations
/Stemmesystem.Server.Tests   - Integration tests (requires Docker)
/Stemmesystem.Data.Tests     - Data layer / EF Core tests
/Stemmesystem.Client.Tests   - Client AutoMapper profile tests
```

## Coding Conventions

- **Target framework:** .NET 6.0 (C# 10)
- **Nullable reference types** are enabled globally — always handle nullability correctly
- **Implicit usings** are enabled
- Use **file-scoped namespaces** in new files (consistent with newer code in `/Server/Features/`)
- **Record types** are preferred for DTOs, request/response models, and input models
- Domain entities use **internal constructors** to enforce creation through factory methods or EF Core
- Use **AutoMapper** profiles (`ApiAutoMapperProfile`, `WebAutoMapperProfile`) for mapping between entities and DTOs — do not add inline mapping logic
- Use **LanguageExt** `Result<T>` / `Either` for functional error handling where already established
- Prefer **FusionCache** for caching (default: 1 minute cache, 2-hour failsafe; already configured)
- Use **AsyncKeyedLock** to prevent race conditions in concurrent voting operations

## Key Patterns

### gRPC Services
Service interfaces live in `Shared/Interfaces/` and are implemented in `Server/Services/`. Register new services in `Server/ServiceConfiguration.cs`.

### SignalR Hubs
Hub client interfaces live in `Shared/SignalR/`. Admin and delegate hubs are separate (`IAdminHubClient`, `IDelegatHubClient`).

### EF Core / Database
- Default schema: `stemme`
- The database is auto-migrated on startup (`MigrateDatabase()` in `Program.cs`)
- Add new migrations with: `dotnet ef migrations add <MigrationName> --project Data --startup-project Server`
- Connection string: configured via `ConnectionStrings:PostgresConnection` (used by `AddAppDbContext()` and tests); the database provider is currently fixed to PostgreSQL in code and the `"Provider"` config value is not read at runtime

### Authentication
- **Duende IdentityServer** handles OAuth2/OIDC
- A custom **Delegatkode extension grant** allows delegates to authenticate with their voting code
- Google OAuth is also supported
- Admin seeding happens via `AddMissingAdminUsers()` on startup

### Secret Voting
Secret ballots hash the `Delegatkode` (via `KeyHasher`) so no delegate identity is stored alongside the vote. Never store delegate IDs for secret votes.

## Important Notes

- **Do not break nullable reference type safety** — the project compiles with `<Nullable>enable</Nullable>`
- **Do not translate Norwegian domain terms** — keep entity/property/method names in Norwegian
- **Tests require Docker** to run integration tests with Testcontainers
- Production deployment is to **Google Cloud Run** (europe-north1) via GitLab CI
