# M9 Design: Reply with Image, @Mentions, Retweet

Date: 2026-06-20

## Overview

Three features extending the existing Twitter clone:
1. **Reply with image** — image upload in the reply composer (ThreadView)
2. **@mentions** — clickable links to user profiles, validated via batch endpoint + client cache
3. **Retweet** — native retweet with timeline propagation, stored as separate domain entity

---

## Feature 1: Reply with Image

### Scope
Frontend only. Backend already accepts `imageUrl` on `POST /api/tweets`.

### Changes
- `ThreadView.vue`: add image upload block identical to HomeView composer — hidden file input, thumbnail preview, clear button, `uploadsImage` ref, `selectedFile` ref.
- On reply submit: upload image first if selected → pass `imageUrl` to `tweetsApi.create({ text, parentId, imageUrl })`.

### Constraints
- Max 5 MB, same allowed content types (jpeg, png, gif, webp) — enforced by existing backend.
- No backend changes required.

---

## Feature 2: @Mentions

### Design Decisions
- Validation: only `@username` patterns that match an existing user become links (Option B).
- Strategy: batch validate + Pinia cache per session (Option B+cache).

### Backend

**New endpoint:**
```
GET /api/users/validate-usernames?usernames=alice,bob,carol
Response 200: { "alice": true, "bob": true, "carol": false }
```
- Query: `SELECT username FROM "Users" WHERE username = ANY(@usernames)` — uses existing unique index on `Username`.
- Max 50 usernames per request (guard against abuse).
- No auth required (public user existence check).

**Layer placement:** Application use case `ValidateUsernamesQuery` → `IUserRepository.GetExistingUsernamesAsync(IEnumerable<string>)` → Infrastructure implementation.

### Frontend

**`useMentionsStore` (Pinia):**
```ts
validatedUsernames: Set<string>       // usernames confirmed to exist
pendingUsernames: Set<string>         // currently being validated (avoid duplicate requests)

async validateBatch(words: string[]): Promise<void>
  // filters already-known and in-flight
  // calls GET /api/users/validate-usernames for unknowns only
  // merges results into validatedUsernames
```

**`MentionText.vue` component:**
- Props: `text: string`
- Splits text on `/(@\w+)/g`
- For each `@word`: if `validatedUsernames.has(word.slice(1))` → `<RouterLink to="/profile/word">@word</RouterLink>`, else plain text span.
- Reactive: updates automatically when store resolves.

**Integration:**
- HomeView and ThreadView: after fetching tweets, call `mentionsStore.validateBatch(allMentions)` once with all unique `@words` from the visible tweet batch.
- Composer: no validation needed at write time.
- Replace raw `<p>{{ tweet.text }}</p>` with `<MentionText :text="tweet.text" />` everywhere.

### Testing
- Backend: unit test `ValidateUsernamesQuery`, integration test for endpoint (returns correct true/false map).
- Frontend: unit test `MentionText` renders link for validated username, plain text for unknown.

---

## Feature 3: Retweet

### Design Decisions
- Storage: separate `Retweet` entity (Option A — clean domain boundary).
- Timeline display: "🔁 @retweeter retweeted" header + original tweet card (Option A).
- Constraints: cannot retweet own tweet; cannot retweet same tweet twice.

### Domain

**Entity `Retweet`:**
```csharp
public sealed class Retweet
{
    public Guid Id { get; private set; }
    public Guid RetweeterId { get; private set; }
    public Guid TweetId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Retweet Create(Guid retweeterId, Guid tweetId, Guid authorId)
    {
        if (retweeterId == authorId) throw new DomainException("Cannot retweet own tweet.");
        return new Retweet { Id = Guid.NewGuid(), RetweeterId = retweeterId, TweetId = tweetId, CreatedAt = DateTime.UtcNow };
    }
}
```

### Application

**`IRetweetRepository`:**
```csharp
Task<bool> ExistsAsync(Guid retweeterId, Guid tweetId, CancellationToken ct);
Task AddAsync(Retweet retweet, CancellationToken ct);
Task RemoveAsync(Guid retweeterId, Guid tweetId, CancellationToken ct);
Task<int> CountAsync(Guid tweetId, CancellationToken ct);
```

**Commands/Queries:**
- `RetweetCommand(RetweeterId, TweetId)` + `RetweetHandler`
- `UnretweetCommand(RetweeterId, TweetId)` + `UnretweetHandler`

**`TweetDto` additions:**
```csharp
int RetweetCount
bool RetweetedByViewer
```

**Timeline entry types:**
```csharp
public sealed record TimelineEntry(
    TweetDto Tweet,
    bool IsRetweet,
    string? RetweetedByUsername,
    DateTime DisplayAt   // used for chronological sort: retweet's CreatedAt
);
```
Timeline query returns `IEnumerable<TimelineEntry>` ordered by `DisplayAt DESC`.

### Infrastructure

**EF Core:**
- `RetweetConfiguration`: table `"Retweets"`, composite unique index `(RetweeterId, TweetId)`.
- New migration: `AddRetweetsTable`.

**Timeline query (pseudo-SQL):**
```sql
-- Own tweets from followed users
SELECT t.*, false as is_retweet, null as retweeted_by, t.created_at as display_at
FROM Tweets t
JOIN Follows f ON f.FolloweeId = t.AuthorId
WHERE f.FollowerId = @viewerId AND t.ParentId IS NULL

UNION ALL

-- Retweets from followed users
SELECT t.*, true as is_retweet, u.Username as retweeted_by, r.CreatedAt as display_at
FROM Retweets r
JOIN Tweets t ON t.Id = r.TweetId
JOIN Users u ON u.Id = r.RetweeterId
JOIN Follows f ON f.FolloweeId = r.RetweeterId
WHERE f.FollowerId = @viewerId AND t.ParentId IS NULL

ORDER BY display_at DESC
LIMIT @pageSize OFFSET @offset
```

### API

```
POST   /api/tweets/{id}/retweet    → 200 { retweetCount }    RequireAuthorization
DELETE /api/tweets/{id}/retweet    → 204                      RequireAuthorization
```

Timeline response: existing `Tweet[]` extended — each entry gains `isRetweet`, `retweetedByUsername`, `retweetCount`, `retweetedByViewer`.

### Frontend

**Types:**
```ts
interface Tweet {
  // existing fields...
  retweetCount: number
  retweetedByViewer: boolean
  isRetweet?: boolean
  retweetedByUsername?: string
}
```

**`socialApi` additions:**
```ts
retweet: (id: string) => apiClient.post<{ retweetCount: number }>(`/api/tweets/${id}/retweet`)
unretweet: (id: string) => apiClient.delete(`/api/tweets/${id}/retweet`)
```

**Tweet card:**
- Retweet button: `🔁 {retweetCount}` — toggles `retweetedByViewer`.
- If `isRetweet`: render `"🔁 @{retweetedByUsername} retweeted"` header above card.

### Testing
- Domain: unit test `Retweet.Create` blocks own-tweet retweet.
- Application: unit tests for `RetweetHandler`, `UnretweetHandler`.
- Integration: POST retweet, DELETE retweet, verify timeline propagation.
- Frontend: unit test retweet button toggle, retweet header render.

---

## Implementation Order

1. Reply with image (frontend only, no dependencies)
2. @mentions backend endpoint + frontend store + MentionText component
3. Retweet domain → application → infrastructure → migration → API → frontend

## Out of Scope
- Quote retweet
- Mention notifications
- Retweet of a retweet
- @mention autocomplete in composer
