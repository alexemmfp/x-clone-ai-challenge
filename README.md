# Twitter Clone — The Flock Challenge

Full-stack Twitter/X clone. .NET 10 Clean Architecture backend + Vue 3 + TypeScript frontend + PostgreSQL.

---

## Prerequisites

| Tool | Version | Check |
|---|---|---|
| .NET SDK | **10.0.x** | `dotnet --version` |
| Node.js | **20.x** | `node --version` |
| Docker Desktop | **25+** (WSL2 backend on Windows) | `docker --version` |
| PowerShell | **7+** | `pwsh --version` |
| Git | any | `git --version` |

---

## Quick Start (Docker — one command)

> Requires Docker Desktop running. Boots Postgres + API + frontend together.

```bash
git clone <repo-url> twitter_clone
cd twitter_clone
docker compose up -d --build
```

- Frontend: **http://localhost** (port 80)
- API: **http://localhost:8080** — health: `GET /health`
- Postgres: **localhost:5433**

The API runs migrations and seeds demo data automatically on first start. Log in with `alice@example.com` / `Seed1234!`.

---

## Quick Start (local dev)

All commands are copy-pasteable. Run them from the repo root unless noted.

### 1 — Clone

```bash
git clone <repo-url> twitter_clone
cd twitter_clone
```

### 2 — Start PostgreSQL

```bash
docker compose up -d postgres
```

Postgres listens on **localhost:5433** (host port) → 5432 inside the container.
Default credentials: user `twitter`, password `change-me-local-dev`, database `twitterclone`.

### 3 — Run database migrations

```powershell
# PowerShell
dotnet ef database update --project backend/src/TwitterClone.Infrastructure --startup-project backend/src/TwitterClone.Api
```

```bash
# bash/macOS/Linux
dotnet ef database update --project backend/src/TwitterClone.Infrastructure --startup-project backend/src/TwitterClone.Api
```

> `appsettings.Development.json` is already committed with the correct connection string (port 5433). No `.env` file is needed for the backend in development.

### 4 — Start the backend

```powershell
# PowerShell
cd backend/src/TwitterClone.Api
dotnet run
```

```bash
# bash
cd backend/src/TwitterClone.Api && dotnet run
```

Backend listens on **http://localhost:5089**. Health check: `GET http://localhost:5089/health`.

### 5 — Configure the frontend environment

The frontend reads `VITE_API_BASE_URL` from a `.env.development` file. This file is **gitignored** — you must create it:

```bash
# from the repo root
echo "VITE_API_BASE_URL=http://localhost:5089" > frontend/.env.development
```

Or create `frontend/.env.development` manually with this content:

```
VITE_API_BASE_URL=http://localhost:5089
```

### 6 — Start the frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend listens on **http://localhost:5173**.

Open **http://localhost:5173** in your browser. You can register a new account or use the seed users below.

---

## Seed users

Ten demo accounts are pre-loaded. Password for all: **`Seed1234!`**

| Username | Email |
|---|---|
| alice | alice@example.com |
| bob | bob@example.com |
| carol | carol@example.com |
| dave | dave@example.com |
| eve | eve@example.com |
| frank | frank@example.com |
| grace | grace@example.com |
| henry | henry@example.com |
| iris | iris@example.com |
| jack | jack@example.com |

---

## Running the test suite

```powershell
# Full suite (build + tests + lint + typecheck)
pwsh scripts/check.ps1

# Backend only
pwsh scripts/check.ps1 -Backend

# Frontend only
pwsh scripts/check.ps1 -Frontend
```

Detailed logs are written to `.logs/`. The console shows a summary only.

**Coverage** (≥85% target) is enforced on Linux/CI via `scripts/check.sh` (used by GitHub Actions). On Windows, Smart App Control can block DLL instrumentation — run `docker compose run --rm backend dotnet test /p:CollectCoverage=true /p:Threshold=85` to measure in a Linux container.

**80 backend tests**: 10 domain · 50 application · 3 architecture · 17 integration.

---

## Implemented features

- **Auth** — register, login, logout, JWT access token + silent refresh via httpOnly cookie
- **Timeline** — chronological feed of tweets from followed users + own tweets
- **Real-time timeline** — SignalR hub pushes new tweets to connected clients instantly
- **Tweets** — create (280 chars), delete own tweets, reply threads (parentId)
- **Follow / Unfollow** — follow or unfollow any user
- **Likes** — like and unlike tweets with live counters
- **Profile** — view any user's profile, follower/following counts; edit own bio
- **User search** — search users by username or email
- **Docker** — `docker compose up -d --build` boots the full stack (Postgres + API + web)

---

## Architecture

Clean Architecture: `Domain → Application → Infrastructure + Api`. Dependencies point inward; Domain has zero external dependencies. Architecture boundaries are enforced at test-time by NetArchTest. See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for full rationale, layer responsibilities, and trade-offs.

---

## Tech decisions

- **Own auth** — JWT access token (Bearer, 15 min) + refresh token in httpOnly cookie (7 days). No third-party auth provider. Passwords hashed with BCrypt (work factor 11).
- **SHA256 for refresh token hashing** — BCrypt is non-deterministic (random salt per call), so stored hashes can't be looked up by value. Refresh tokens are already high-entropy random strings; SHA256 determinism enables exact-match DB lookups without BCrypt's work factor overhead.
- **Clean Architecture** — use-case classes in `Application`, domain rules in `Domain`, EF Core + repos in `Infrastructure`. Enforced by NetArchTest.
- **EF Core + Testcontainers** — integration tests spin up a real PostgreSQL container so tests cover actual SQL and migrations, not mocks.
- **Vue 3 + Pinia + Axios interceptor** — the Axios instance auto-refreshes the access token on 401 before retrying the original request; the user never sees a login redirect on token expiry.
- **SignalR real-time** — `TimelineHub` broadcasts `TweetCreated` events to all connected clients. Interface (`ITimelineNotifier`) defined in Application; SignalR implementation in Infrastructure (dependency rule preserved).
- **Tailwind mobile-first** — all views are responsive; the layout collapses to a single column on small screens.
- **AI-assisted development** — Claude Code (Sonnet 4.6) wrote ~85% of code in pair-programming mode: the human wrote the CLAUDE.md constraints, reviewed diffs, and made architectural decisions; AI implemented handlers, tests, frontend components, and DevOps. All commits include `Co-Authored-By: Claude Sonnet 4.6`.

---

## Project layout

```
backend/
  src/
    TwitterClone.Domain/         # entities, value objects — zero deps
    TwitterClone.Application/    # use cases, DTOs, interfaces
    TwitterClone.Infrastructure/ # EF Core, repos, JWT, BCrypt
    TwitterClone.Api/            # minimal API endpoints, DI wiring
  tests/
    TwitterClone.Domain.Tests/
    TwitterClone.Application.Tests/
    TwitterClone.Integration.Tests/    # WebApplicationFactory + Testcontainers
    TwitterClone.Architecture.Tests/   # NetArchTest boundary rules
frontend/
  src/{components,views,stores,router,api,composables,types}
  tests/      # Vitest unit/integration
  e2e/        # Playwright
docs/   scripts/   docker-compose.yml   .env.example
```

---

## Further reading

- Spec: [`docs/SPEC.md`](docs/SPEC.md)
- Roadmap: [`docs/ROADMAP.md`](docs/ROADMAP.md)
- Architecture deep-dive: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)
- Backend conventions: [`backend/AGENTS.md`](backend/AGENTS.md)
- Frontend conventions: [`frontend/AGENTS.md`](frontend/AGENTS.md)
