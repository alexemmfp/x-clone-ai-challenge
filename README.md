# Twitter Clone — The Flock Challenge

A functional Twitter/X clone. Full-stack, .NET 10 (Clean Architecture) + Vue 3 + PostgreSQL.

> Status: harness/scaffolding. Sections marked _TODO_ are filled as milestones land (see [`docs/ROADMAP.md`](docs/ROADMAP.md)).

## Stack & why
- **Backend**: .NET 10, ASP.NET Core, Clean Architecture (Domain / Application / Infrastructure / Api). EF Core + Npgsql.
- **Frontend**: Vue 3 + Vite + TypeScript + Pinia + Vue Router + Tailwind (mobile-first).
- **DB**: PostgreSQL.
- **Auth**: own — JWT access token + refresh token in httpOnly cookie, BCrypt password hashing.
- Full rationale: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).

---

## Runbook (Setup & Operation)

### Prerequisites
- .NET SDK **10.0.x** — `dotnet --version`
- Node **20.x** + npm — `node --version`
- Docker **25+** (for PostgreSQL + integration tests) — `docker --version`
- PowerShell 7+ (`pwsh`) to run the check script (optional; `scripts/check.sh` works on bash)

### 1. Clone & configure
```bash
git clone <repo-url> twitter_clone && cd twitter_clone
cp .env.example .env        # then edit secrets
```

### 2. Start the database
```bash
docker compose up -d postgres
```

### 3. Backend — _TODO (M0/M1)_
```bash
# dotnet restore backend/TwitterClone.sln
# dotnet ef database update -p backend/src/TwitterClone.Infrastructure -s backend/src/TwitterClone.Api
# dotnet run --project backend/src/TwitterClone.Api      # -> http://localhost:8080
```

### 4. Seed data — _TODO (M6)_
```bash
# dotnet run --project backend/src/TwitterClone.Api -- seed
```

### 5. Frontend — _TODO (M0)_
```bash
# cd frontend && npm install && npm run dev               # -> http://localhost:5173
```

### 6. Run the full test suite
```bash
pwsh scripts/check.ps1        # build + tests + coverage(>=85%) + lint + typecheck
# or: bash scripts/check.sh
```

### Environment variables
Full list with descriptions and example values in [`.env.example`](.env.example). `.env.example` is the canonical reference; `.env` is gitignored.

### Example credentials (after seed) — _TODO (M6)_
- email: `alice@example.com` · password: `Password123!`

---

## Technical decisions
Summarized here from [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md): stack rationale, timeline & follow-graph modeling, auth approach, trade-offs/known limits, and AI tooling used. _TODO: finalize at M8._

## Tests
See [`docs/ARCHITECTURE.md#testing-strategy`](docs/ARCHITECTURE.md). Backend coverage target **≥ 85%**.

## AI tooling
Built with agentic coding (Claude Code). The harness that steers it: [`CLAUDE.md`](CLAUDE.md), [`backend/AGENTS.md`](backend/AGENTS.md), [`frontend/AGENTS.md`](frontend/AGENTS.md). _TODO: expand at M8._
