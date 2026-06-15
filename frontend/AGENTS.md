# frontend/AGENTS.md — Vue 3 + TS + Tailwind conventions

Read with [`../CLAUDE.md`](../CLAUDE.md). Rules for frontend code.

## Stack & structure
- Vue 3 `<script setup lang="ts">`, Vite, TypeScript **strict**, Pinia, Vue Router, Tailwind, Axios.
- Layout: `src/components` (dumb/reusable), `src/views` (routed pages), `src/stores` (Pinia), `src/router`, `src/api` (axios clients), `src/composables` (`useXxx`), `src/types` (shared TS types). E2E in `e2e/`.

## Conventions
- Components PascalCase (`TweetCard.vue`). Composables `useThing.ts`. Stores `useAuthStore`, `useTimelineStore`.
- Props/emits typed via `defineProps<T>()` / `defineEmits<T>()`. No `any`. Share API types in `src/types`.
- API calls only through `src/api/*` clients (one axios instance with the refresh interceptor) — never call axios ad hoc in components.
- State that crosses views → Pinia store. Local UI state → `ref`/`reactive` in the component.
- Access token lives in the auth store (memory), **never localStorage**. The axios interceptor calls `/auth/refresh` on 401 and retries once.
- Router guards protect authenticated routes; redirect to `/login` when unauthenticated.

## Styling (mobile-first, mandatory)
- Tailwind utilities, **mobile-first**: base styles target mobile, add `sm:`/`md:`/`lg:` to scale up. Breakpoints: mobile `<640`, tablet `640–1024`, desktop `>1024`.
- Verify each view at 375px, 768px, 1280px. No fixed widths that break mobile; no horizontal scroll.
- Keep design tokens in `tailwind.config`; avoid scattered magic values.

## Testing
- Vitest + @vue/test-utils. Integration tests for the main flows: **login, create tweet, follow** (mandatory).
- Mock the API layer (`src/api`) in component/integration tests; don't hit a real backend there.
- Playwright (`e2e/`) for browser flows (auth happy path at minimum).
- `vue-tsc` must pass (type-check is part of the gate).

## Commands
```bash
npm install
npm run dev          # vite dev server
npm run build        # vite build (prod)
npm run lint         # eslint
npm run format       # prettier
npm run type-check   # vue-tsc --noEmit
npm run test:unit    # vitest
npm run test:e2e     # playwright
```
Full gate: `pwsh scripts/check.ps1 -Frontend`.

## Key packages
vue · vue-router · pinia · axios · tailwindcss · @microsoft/signalr (real-time) · vitest · @vue/test-utils · @playwright/test · eslint · prettier · vue-tsc.
