# M10 Frontend Report

## Status
DONE — `pwsh scripts/check.ps1 -Frontend` green (lint PASS, type-check PASS, unit tests PASS).

## Commits
Pending — changes not yet committed.

## Tests
All existing tests updated with `replyCount: 0, authorDisplayName: null, authorAvatarUrl: null` on every mock Tweet object in:
- `frontend/tests/thread-view.test.ts`
- `frontend/tests/retweet.test.ts`
- `frontend/tests/image-upload.test.ts`

## Changes
- `frontend/src/types/tweet.ts`: added `replyCount`, `authorDisplayName`, `authorAvatarUrl` fields.
- `frontend/src/views/HomeView.vue`: card header replaced with avatar + displayName layout; 💬 reply count RouterLink added before like button.
- `frontend/src/views/ThreadView.vue`: parent and reply card headers updated with same avatar + displayName pattern; 💬 replyCount span added to parent action bar.
- `frontend/src/stores/useTweetStore.ts`: no changes needed — store passes API data through without stripping fields.

## Concerns
None.
