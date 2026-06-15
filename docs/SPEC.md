# SPEC — Twitter Clone

Derived from `TwitterClone_Challenge_TheFlock.docx`. This is the source of truth for scope. If a requirement here is unclear, ask before building.

## Mandatory features

### Auth
- Register with **email + password** (minimum).
- Login / Logout.
- Route protection: authenticated actions require a valid session.
- Basic profile: **unique username**, bio, avatar placeholder.

### Tweets
- Create tweet (text, **max 280 chars**).
- Delete own tweet.
- Timeline: tweets from **followed** users, **chronological** order.
- Pagination or infinite scroll on the timeline.

### Social
- Follow / Unfollow users.
- Like / Unlike tweets.
- Visible like counter per tweet.
- Followers and Following lists on the profile.

### Search
- Basic user search by name or username.

### Responsive (mobile-first)
- Fully responsive, usable on mobile. Design mobile-first, scale up.
- Breakpoints: mobile `< 640px`, tablet `640–1024px`, desktop `> 1024px`.

## Technical requirements

### Testing
- Backend coverage **≥ 85%** (challenge floor is 80%; we target 85%+ for the Testing rubric).
  - Unit tests for models and validations.
  - Integration tests for critical API endpoints.
  - At least one **end-to-end** test of the auth flow.
- Frontend: integration tests for the main flows (**login, create tweet, follow**).

### Seed data
- Seed generates realistic data: **≥ 10 users** with tweets, follows, and crossed likes.
- After running the seed, the app shows content immediately.

### README + Runbook (mandatory, graded)
The Runbook must let evaluators boot the app **with no extra manual steps**. Missing/broken Runbook is penalized under Functionality.
- Prerequisites: exact runtime/tool/dependency versions.
- Install steps: exact commands, in order.
- How to run the seed.
- How to run the app in dev mode.
- How to run the full test suite.
- Required env vars: full list + descriptions + example values. **`.env.example` is mandatory.**
- Example login credentials (≥ 1 seeded user with email + password).
- Technical decisions: stack rationale, timeline & follow-graph modeling, auth approach, trade-offs/known limits, which AI tools were used and how.

## Bonus (targeted)
- **Docker compose** — one command brings up the whole stack.
- **Real-time** — live timeline updates (SignalR / WebSockets).
- **Reply threads** — replies to tweets, threaded view.
- **Image upload** — attach one image to a tweet.

## Delivery
- Public GitHub repo (or shared with evaluators), full history visible.
- **No squash.** Code on `main`.
- Logical commit progression: scaffolding → features → tests alongside → polish.

## Out of scope (unless added to this file)
Retweets/quotes, DMs, hashtags/trends, bookmarks, lists, multi-image, video, moderation, admin panel.
