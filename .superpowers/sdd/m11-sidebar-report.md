# M11 Sidebar Report

## Status
DONE — check.ps1 -Frontend GREEN (lint, type-check, unit tests all pass).

## Commits
Pending commit: `feat(ui): fixed sidebar navigation and mobile bottom nav`

## Tests
No new unit tests required for pure layout/nav components (no business logic). Existing unit tests unchanged and passing.

## Concerns
- `Profile` type has no `displayName` field — sidebar shows `profile.username` instead of a display name. This matches the actual type; the spec was aspirational.
- Clicking the sm icon-only avatar calls `auth.logout()` directly (no confirmation). Matches spec.
- Notifications item is intentionally passive (no link, grayed out) per spec.
