# CLAUDE.md — Twitter Clone (The Flock Challenge)

> Master feedforward guide for AI coding agents. Read this first, every session.
> Companion guides: [`backend/AGENTS.md`](backend/AGENTS.md), [`frontend/AGENTS.md`](frontend/AGENTS.md).
> Specs: [`docs/SPEC.md`](docs/SPEC.md) · Plan: [`docs/ROADMAP.md`](docs/ROADMAP.md) · Arch: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).

## 0. SESSION START — ejecutar en orden, sin saltear pasos

> Protocolo obligatorio. Cada paso es prereq del siguiente. **No empezar a codear antes de completar 1–4.**

1. **Baseline**: `pwsh scripts/check.ps1` → debe estar verde. Si no, arreglar PRIMERO.
2. **Estado**: Leer `docs/ROADMAP.md` → encontrar primer `[ ]`. Marcarlo `[~]` + `git commit -m "chore: mark <task> in progress"`.
3. **Spec**: Leer la sección de `docs/SPEC.md` que corresponde a la tarea `[~]`.
4. **Convenciones**: Leer `backend/AGENTS.md` o `frontend/AGENTS.md` según la capa afectada.
5. **TDD** → ver §TDD abajo. NO implementar antes de que los tests existan y fallen.
6. **Commit**: feature + tests + `[x]` en `ROADMAP.md` en el **mismo commit**.
7. **Push**: `git push origin main`.
8. **Loop**: volver al paso 2.

**NUNCA** commitear sin que `check.ps1` sea verde.  
**NUNCA** marcar `[x]` en ROADMAP en un commit separado del feature.

## 1. Goal & constraints

Build a functional Twitter/X clone, full-stack. This is a graded 72h challenge. We are evaluated on the **process** (commit history) as much as the result.

Rubric weights — optimize for these:

| Criterion | Weight | What moves the needle |
|---|---:|---|
| Testing | 25% | Real tests, backend coverage **≥ 85%**, meaningful frontend tests |
| Functionality | 25% | Every mandatory feature works end-to-end; app boots from the Runbook with zero manual steps |
| Code quality | 20% | Clean, consistent, Clean Architecture boundaries respected |
| Process | 15% | Incremental commits, **no squash**, logical progression, evident good AI usage |
| Documentation | 10% | Complete README + **operative Runbook** (first thing evaluators read) |
| Bonus | 5% | Docker, real-time, reply threads, image upload |

## 2. Stack (decided — do not change without updating this file)

- **Backend**: .NET 10, ASP.NET Core, **Clean Architecture**. EF Core 10 + Npgsql (PostgreSQL).
- **Auth**: own auth (no Firebase/Supabase). JWT **access token** (Bearer, short-lived) + **refresh token in httpOnly cookie**. Passwords hashed with BCrypt.
- **Frontend**: Vue 3 + Vite + **TypeScript** + Pinia + Vue Router + **Tailwind** (mobile-first). Axios with refresh interceptor.
- **DB**: PostgreSQL.
- **Tests**: backend xUnit + FluentAssertions + Testcontainers (real Postgres) + coverlet + NetArchTest; frontend Vitest + @vue/test-utils + Playwright (E2E).
- **Bonus targeted**: Docker compose · SignalR real-time timeline · reply threads · image upload.

## 3. Architecture (the dependency rule is non-negotiable)

```
Api  ──>  Application  ──>  Domain
 │            │
 └─> Infrastructure ──┘   (implements Application interfaces)
```

Dependencies point **inward**. Domain depends on nothing. Application depends only on Domain. Infrastructure and Api depend outward-in only. Enforced by NetArchTest in `tests` (see `docs/ARCHITECTURE.md`). If you need an external thing in Application, define an **interface** in Application and implement it in Infrastructure.

## 4. Repo layout

```
backend/
  src/
    TwitterClone.Domain/          # entities, value objects, domain rules — zero deps
    TwitterClone.Application/     # use cases, DTOs, interfaces, validators
    TwitterClone.Infrastructure/  # EF Core, repos, auth, SignalR, file storage
    TwitterClone.Api/             # endpoints, DI wiring, middleware
  tests/
    TwitterClone.Domain.Tests/
    TwitterClone.Application.Tests/
    TwitterClone.Integration.Tests/   # WebApplicationFactory + Testcontainers
    TwitterClone.Architecture.Tests/  # NetArchTest boundary rules
  TwitterClone.sln
frontend/
  src/{components,views,stores,router,api,composables,types}
  tests/            # Vitest unit/integration
  e2e/              # Playwright
docs/   scripts/   .github/workflows/   docker-compose.yml   .env.example
```

## 5. Commands — the feedback sensors

One command gates everything. Run it before declaring anything done:

```powershell
pwsh scripts/check.ps1          # build + tests + coverage(≥85%) + lint + typecheck
pwsh scripts/check.ps1 -Backend # backend only
pwsh scripts/check.ps1 -Frontend
```

Detailed logs go to `.logs/`; the console shows only a few summary lines (keep agent context clean). Day-to-day commands live in `backend/AGENTS.md` and `frontend/AGENTS.md`.

## TDD — orden obligatorio (red → green → commit)

1. **Red** — Crear archivo(s) de test para la feature. Correr `pwsh scripts/check.ps1 -Backend` (o `-Frontend`). Los tests **DEBEN FALLAR**. Si pasan, el test está incompleto — arreglarlo.
2. **Green** — Implementar mínimo código para que pasen. Correr `check.ps1` completo. **DEBE ser verde**.
3. **Commit** — `feat(<scope>): descripción` + `[x]` en `ROADMAP.md` en el mismo commit.

## 6. Definition of Done (a feature is NOT done until all are true)

1. Code compiles with **zero warnings** (`TreatWarningsAsErrors`).
2. Tests written **with** the feature (not later). Backend coverage stays **≥ 85%**.
3. `scripts/check.ps1` is green.
4. Lint + format clean (`dotnet format`, ESLint, Prettier, `vue-tsc`).
5. Runbook/README updated if setup, env vars, or commands changed.
6. Committed as one focused **Conventional Commit** (see §7).

## 7. Commit discipline (15% of the grade — take it seriously)

- **Conventional Commits**: `feat:`, `fix:`, `test:`, `refactor:`, `docs:`, `chore:`, `build:`.
- **Small and incremental**. One logical change per commit. Tests committed alongside their feature.
- **Never squash.** The history must tell the story: scaffolding → features one by one → polish.
- First commit = scaffolding/harness. Last commits = polish, docs, cleanup.
- Do not commit secrets. `.env` is gitignored; only `.env.example` is tracked.
- Only commit when the user asks. When you do, follow the roadmap order.

## 8. Guardrails — do not

- Do not introduce a feature, abstraction, or dependency that isn't in `docs/SPEC.md` / roadmap without asking.
- Do not use third-party auth providers (Firebase/Supabase/Auth0). Auth is ours.
- Do not weaken or delete a test to make the gate pass. Fix the cause.
- Do not bypass the Clean Architecture dependency rule.
- Do not rewrite whole files for small changes; edit in place.
- Do not put detailed test/build output into your reply — point to `.logs/`.

## 9. Where to look

- What to build → [`docs/SPEC.md`](docs/SPEC.md)
- In what order → [`docs/ROADMAP.md`](docs/ROADMAP.md)
- How the layers/decisions work → [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)
- Backend conventions → [`backend/AGENTS.md`](backend/AGENTS.md)
- Frontend conventions → [`frontend/AGENTS.md`](frontend/AGENTS.md)
- How to run it → [`README.md`](README.md) (Runbook)
