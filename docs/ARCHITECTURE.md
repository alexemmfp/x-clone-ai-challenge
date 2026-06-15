# ARCHITECTURE — decisions & layer rules

Living document. Records the *why*. The README pulls its "Technical decisions" section from here.

## Clean Architecture layers

| Layer | Project | Depends on | Contains | Must NOT contain |
|---|---|---|---|---|
| Domain | `TwitterClone.Domain` | nothing | Entities, value objects, domain enums/exceptions, invariants | EF Core, ASP.NET, DTOs, I/O |
| Application | `TwitterClone.Application` | Domain | Use cases (handlers), DTOs, validators, **interfaces** (repos, auth, clock, storage) | EF Core, ASP.NET, concrete infra |
| Infrastructure | `TwitterClone.Infrastructure` | Application, Domain | EF Core DbContext + configs, repo impls, BCrypt, JWT, SignalR hub, file storage | HTTP endpoint definitions |
| Api | `TwitterClone.Api` | Application, Infrastructure (DI only), Domain | Endpoints/controllers, middleware, DI composition root, mapping | business logic, EF queries |

**Dependency rule:** arrows point inward. Domain is the center. Enforced by `TwitterClone.Architecture.Tests` (NetArchTest): Domain references no other project; Application references only Domain; Infrastructure/Api never referenced by inner layers.

## CQRS-lite, no MediatR
Use cases are plain handler classes in Application (e.g. `CreateTweetHandler`) with an input DTO and a result, registered in DI and invoked from endpoints. Rationale: avoids MediatR's licensing change, removes a dependency, keeps call paths explicit and trivially testable. We separate commands (writes) from queries (reads) by folder, not by framework.

## Auth
- Register/login issue a **JWT access token** (short-lived, ~15 min, returned in body) plus a **refresh token** stored in an **httpOnly, Secure, SameSite cookie**.
- `POST /auth/refresh` rotates the refresh token and returns a new access token. Logout revokes it.
- Passwords hashed with **BCrypt** (work factor configurable via env). No third-party identity provider — auth logic is ours.
- Refresh tokens persisted (hashed) so they can be revoked; rotation on each refresh.
- Frontend keeps the access token in memory (Pinia), never localStorage; an axios interceptor calls `/auth/refresh` on 401.

## Data model (timeline & follow graph)
- `User (id, username unique, email unique, passwordHash, bio, avatarUrl, createdAt)`
- `Follow (followerId, followeeId, createdAt)` — composite PK, directed edge of the graph.
- `Tweet (id, authorId, text<=280, parentId nullable, imageUrl nullable, createdAt)` — `parentId` enables reply threads.
- `Like (userId, tweetId, createdAt)` — composite PK; like count = aggregate (and/or denormalized counter if needed for perf).
- **Timeline query**: tweets where `authorId IN (followees of current user)` (optionally + self), ordered by `createdAt DESC`, keyset/seek pagination (`createdAt`,`id`) for stable infinite scroll. Document the trade-off (fan-out-on-read now; fan-out-on-write is a future optimization).

## Validation & errors
- FluentValidation in Application; one mapping middleware in Api turns validation/domain errors into RFC7807 ProblemDetails. Endpoints stay thin.

## Testing strategy (coverage target ≥85%)
- **Domain.Tests**: pure unit tests of invariants (e.g. tweet length, follow self-rule).
- **Application.Tests**: handlers with mocked interfaces.
- **Integration.Tests**: `WebApplicationFactory` + **Testcontainers Postgres** (real DB) for critical endpoints + the E2E auth flow.
- **Architecture.Tests**: NetArchTest boundary rules.
- Frontend: Vitest + @vue/test-utils for login/create-tweet/follow flows; Playwright for the browser E2E.

## Trade-offs / known limits (keep updated for the README)
- Fan-out-on-read timeline: simple and correct, fine at seed scale; would need caching/materialization at large scale.
- Single-image upload only; stored on local volume (or S3-compatible) — documented in README.
- _add as they appear._
