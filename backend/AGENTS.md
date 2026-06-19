# backend/AGENTS.md — .NET 10 Clean Architecture conventions

Read with [`../CLAUDE.md`](../CLAUDE.md) and [`../docs/ARCHITECTURE.md`](../docs/ARCHITECTURE.md). These are the rules to follow when writing backend code.

## Projects & dependency rule
- `Domain` → no project references. `Application` → Domain only. `Infrastructure` → Application + Domain. `Api` → Application + Infrastructure (DI) + Domain.
- Need infra from Application? Define an **interface** in `Application/Abstractions` and implement it in `Infrastructure`. Never reference Infrastructure types from Application/Domain.

## Conventions
- C# latest, **nullable enabled**, **implicit usings** on. File-scoped namespaces. One public type per file.
- Naming: PascalCase types/methods, camelCase locals, `_camelCase` private fields, `I`-prefixed interfaces. Async methods end in `Async`.
- Prefer records for DTOs and immutable value objects; classes for entities with behavior.
- No business logic in endpoints/controllers — delegate to an Application handler.
- Use case = `XxxHandler` with `HandleAsync(XxxCommand/Query, CancellationToken)`. Commands under `Application/Features/<area>/Commands`, queries under `.../Queries`.
- Validation via FluentValidation (`AbstractValidator<T>`), invoked before the handler. Domain invariants live in the entity (throw domain exceptions).
- EF Core: configurations via `IEntityTypeConfiguration<T>` in Infrastructure; no data annotations on Domain entities. Async EF calls + `CancellationToken`. No lazy loading. Use `AsNoTracking()` for queries.
- Endpoints: Minimal APIs grouped by feature (`app.MapGroup("/tweets")`), or thin controllers — be consistent. Return ProblemDetails on error via the shared middleware.
- DI: register per project with a `DependencyInjection.cs` `AddXxx(this IServiceCollection)` extension; compose in `Program.cs`.
- Config/secrets only through `IConfiguration`/options + env vars. Never hardcode connection strings or JWT keys.

## Testing
- xUnit + FluentAssertions. One test class per unit; `Method_State_Expected` naming.
- Domain/Application: fast, isolated, mock interfaces (NSubstitute).
- Integration: `WebApplicationFactory` + **Testcontainers** Postgres (real DB, fresh per run). Cover critical endpoints + the E2E auth flow.
- Architecture: NetArchTest rules in `Architecture.Tests` (the dependency rule above).
- Coverage **≥ 85%** (coverlet). Don't write empty/assert-nothing tests to game it.

## Commands
```powershell
dotnet build backend/TwitterClone.sln -warnaserror
dotnet test  backend/TwitterClone.sln
dotnet format backend/TwitterClone.sln                 # style/lint
dotnet ef migrations add <Name> -p backend/src/TwitterClone.Infrastructure -s backend/src/TwitterClone.Api
dotnet ef database update    -p backend/src/TwitterClone.Infrastructure -s backend/src/TwitterClone.Api
dotnet run --project backend/src/TwitterClone.Api
```
Coverage + full gate: `pwsh scripts/check.ps1 -Backend`.

## Key packages
Npgsql.EntityFrameworkCore.PostgreSQL · Microsoft.EntityFrameworkCore.Design · FluentValidation · BCrypt.Net-Next · Microsoft.AspNetCore.Authentication.JwtBearer · Microsoft.AspNetCore.SignalR · xUnit · FluentAssertions · NSubstitute · Testcontainers.PostgreSql · coverlet.collector · NetArchTest.Rules.

## TDD Workflow — seguir en orden estricto

### 1 — Red (tests primero, antes de implementar)
```powershell
# Crear test file en tests/TwitterClone.<Layer>.Tests/<Feature>/
dotnet test backend/TwitterClone.sln --nologo
# Tests nuevos DEBEN FALLAR. Si pasan → el test no cubre la feature. Arreglarlo.
```

### 2 — Green (implementar hasta que los tests pasen)
```powershell
dotnet build backend/TwitterClone.sln -warnaserror --nologo
dotnet test  backend/TwitterClone.sln --nologo
# Todos deben pasar.
```

### 3 — Gate (antes de commitear)
```powershell
pwsh scripts/check.ps1 -Backend
# MUST print: CHECK PASSED — si no, arreglar. NO commitear con rojo.
```

### 4 — Commit (feature + tests + ROADMAP en un solo commit)
```powershell
git add backend/src/... backend/tests/... docs/ROADMAP.md
git commit -m "feat(<scope>): descripción

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>"
```
