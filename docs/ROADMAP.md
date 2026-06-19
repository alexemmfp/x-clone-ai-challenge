# ROADMAP — incremental delivery plan

Order of work, mapped to commits. Each milestone = one or a few focused commits, **tests included in the same milestone**. Keep history linear and legible (Process = 15%). Check `scripts/check.ps1` green before each commit.

Legend: `[ ]` todo · `[~]` en progreso (marcar al INICIAR, commitear inmediatamente) · `[x]` done (incluir en el MISMO commit que el feature).

> **Regla de estado**: Marcar `[~]` al iniciar la tarea (commit propio: `chore: mark <task> in progress`). Marcar `[x]` en el MISMO commit que el feature + tests. Nunca en batch al final de la sesión.

## M0 — Harness & scaffolding
- [x] Harness: CLAUDE.md, AGENTS guides, SPEC, ROADMAP, ARCHITECTURE, check scripts, gates, docker, env, CI. — `chore: project harness and conventions`
- [x] Backend solution skeleton (4 src + 4 test projects, Clean Arch refs, Directory.Build.props). — `build: scaffold backend clean-architecture solution`
- [x] Frontend skeleton (Vite + Vue + TS + Tailwind + Pinia + Router). — `build: scaffold vue frontend`
- [x] docker-compose up brings up Postgres; backend connects; healthcheck. — `build: postgres via docker-compose`
- [x] Architecture tests green (NetArchTest boundary rules). — `test: clean-architecture boundary rules`

## M1 — Domain & persistence foundation
- [x] Domain entities: User, Tweet, Follow, Like (+ Reply via Tweet.ParentId). — `feat(domain): core entities and rules`
- [x] EF Core DbContext, configurations, first migration. — `feat(infra): ef core context and initial migration`
- [x] Repository interfaces (Application) + EF implementations (Infrastructure). — `feat(infra): repositories`

## M2 — Auth (vertical slice, end-to-end)
- [x] Register + login + refresh + logout use cases; BCrypt hashing; JWT issuance; refresh token in httpOnly cookie. — `feat(auth): register, login, refresh, logout`
- [x] Route protection middleware/policies. — `feat(auth): protected routes`
- [x] Integration tests + **E2E auth flow** test. — `test(auth): integration + e2e auth flow`
- [x] Frontend: auth store, login/register views, axios refresh interceptor, guarded routes. — `feat(web): auth screens and session`

## M3 — Tweets & timeline
- [x] Create tweet (≤280), delete own tweet. — `feat(tweets): create and delete`
- [x] Timeline: followed users, chronological, paginated. — `feat(timeline): paginated following feed`
- [x] Frontend: composer, timeline with infinite scroll. — `feat(web): composer and timeline`
- [x] Tests for tweet + timeline (incl. frontend create-tweet flow). — `test: tweets and timeline`

## M4 — Social graph
- [x] Follow / unfollow. — `feat(social): follow and unfollow`
- [x] Like / unlike + counter. — `feat(social): likes`
- [x] Profile: bio, avatar placeholder, followers/following lists. — `feat(profile): profile and follow lists`
- [x] Tests (incl. frontend follow flow). — `test: social graph`

## M5 — Search & responsive polish
- [x] User search by name/username. — `feat(search): user search`
- [x] Mobile-first responsive pass across all views (640/1024 breakpoints). — `style(web): responsive mobile-first pass`

## M6 — Seed & Runbook
- [x] Seed: ≥10 users with tweets, follows, crossed likes; idempotent. — `feat(seed): realistic demo data`
- [x] Fill README Runbook end-to-end; verify cold boot from clean clone. — `docs: complete runbook`

## M7 — Bonus (in value order; do only what time allows)
- [x] Docker compose full-stack one-command up. — `build: full-stack docker-compose`
- [x] Real-time timeline via SignalR. — `feat(realtime): live timeline via signalr`
- [x] Reply threads (Tweet.ParentId + thread view). — `feat(replies): threaded replies` · [SPEC §reply-threads](docs/SPEC.md#reply-threads)
- [~] Image upload on tweet. — `feat(media): image upload on tweets` · [SPEC §image-upload](docs/SPEC.md#image-upload)

## M8 — Final polish
- [x] Coverage audit ≥85%, remove dead code, consistent naming. — `refactor: cleanup and coverage audit`
- [x] Final docs pass (decisions, trade-offs, AI usage). — `docs: architecture decisions and trade-offs`

> Reorder allowed, but keep tests inside each milestone and never batch all tests at the end.
