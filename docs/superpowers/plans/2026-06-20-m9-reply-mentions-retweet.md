# M9: Reply with Image, @Mentions, Retweet — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add image upload to reply composer, clickable @mention links validated via batch endpoint, and native retweet with timeline propagation.

**Architecture:** Reply image is frontend-only (backend already supports `imageUrl` on POST /api/tweets). @mentions use a new batch validate endpoint + Pinia cache + `MentionText.vue` component. Retweet uses a separate `Retweet` domain entity, `IRetweetRepository`, new commands/handlers, extended `TweetDto`, and two new API endpoints; timeline handler merges tweets + retweets in memory.

**Tech Stack:** .NET 10 / ASP.NET Core Minimal APIs / EF Core 10 / Npgsql / xUnit / FluentAssertions / Testcontainers · Vue 3 / Pinia / TypeScript / Tailwind / Vitest

## Global Constraints

- Clean Architecture: Domain ← Application ← Infrastructure/Api. No cross-layer leaks.
- `TreatWarningsAsErrors` is set — zero warnings allowed.
- Backend coverage must stay ≥ 85% (enforced in CI).
- Tests committed in same commit as feature (`feat:` commit includes test files).
- Run `pwsh scripts/check.ps1 -Backend` after every backend task; `-Frontend` after every frontend task.
- Conventional Commits: `feat(scope): description`.
- Mark ROADMAP.md `[~]` at task start, `[x]` in same commit as the feature.

---

## File Map

### Task 1 — Reply with image
- Modify: `frontend/src/views/ThreadView.vue`
- Modify: `frontend/tests/thread-view.test.ts`

### Task 2 — @mentions backend
- Modify: `backend/src/TwitterClone.Application/Interfaces/IUserRepository.cs`
- Create: `backend/src/TwitterClone.Application/Users/Queries/ValidateUsernamesQuery.cs`
- Modify: `backend/src/TwitterClone.Infrastructure/Persistence/Repositories/UserRepository.cs`
- Create: `backend/src/TwitterClone.Api/Endpoints/UserEndpoints.cs`
- Modify: `backend/src/TwitterClone.Api/Program.cs`
- Create: `backend/tests/TwitterClone.Application.Tests/Users/ValidateUsernamesQueryTests.cs`
- Create: `backend/tests/TwitterClone.Integration.Tests/Users/ValidateUsernamesEndpointTests.cs`

### Task 3 — @mentions frontend
- Create: `frontend/src/api/users.ts`
- Create: `frontend/src/stores/useMentionsStore.ts`
- Create: `frontend/src/components/MentionText.vue`
- Modify: `frontend/src/views/HomeView.vue`
- Modify: `frontend/src/views/ThreadView.vue`
- Create: `frontend/tests/mention-text.test.ts`

### Task 4 — Retweet domain entity
- Create: `backend/src/TwitterClone.Domain/Entities/Retweet.cs`
- Create: `backend/tests/TwitterClone.Domain.Tests/RetweetTests.cs`

### Task 5 — Retweet application layer
- Create: `backend/src/TwitterClone.Application/Interfaces/IRetweetRepository.cs`
- Modify: `backend/src/TwitterClone.Application/Tweets/Dtos/TweetDto.cs`
- Create: `backend/src/TwitterClone.Application/Tweets/Commands/RetweetCommand.cs`
- Create: `backend/src/TwitterClone.Application/Tweets/Commands/UnretweetCommand.cs`
- Modify: `backend/src/TwitterClone.Application/Tweets/Queries/GetTimelineQuery.cs`
- Create: `backend/tests/TwitterClone.Application.Tests/Tweets/RetweetHandlerTests.cs`

### Task 6 — Retweet infrastructure
- Create: `backend/src/TwitterClone.Infrastructure/Persistence/Configurations/RetweetConfiguration.cs`
- Modify: `backend/src/TwitterClone.Infrastructure/Persistence/AppDbContext.cs`
- Create: `backend/src/TwitterClone.Infrastructure/Persistence/Repositories/RetweetRepository.cs`
- Modify: `backend/src/TwitterClone.Infrastructure/DependencyInjection.cs`
- Create: EF Core migration `AddRetweetsTable`

### Task 7 — Retweet API + integration tests
- Modify: `backend/src/TwitterClone.Api/Endpoints/SocialEndpoints.cs`
- Create: `backend/tests/TwitterClone.Integration.Tests/Tweets/RetweetEndpointTests.cs`

### Task 8 — Retweet frontend
- Modify: `frontend/src/types/tweet.ts`
- Modify: `frontend/src/api/social.ts`
- Modify: `frontend/src/views/HomeView.vue`
- Modify: `frontend/tests/auth.store.test.ts` (no change needed — just re-run check)

---

## Task 1: Reply Composer — Image Upload

**Files:**
- Modify: `frontend/src/views/ThreadView.vue`
- Modify: `frontend/tests/thread-view.test.ts`

**Interfaces:**
- Consumes: `tweetsApi.uploadImage(file: File): Promise<string>` (existing in `frontend/src/api/tweets.ts`)
- Consumes: `tweetsApi.create({ text, parentId, imageUrl? })` (existing)

- [ ] **Step 1: Mark roadmap task in progress**

In `docs/ROADMAP.md` change:
```
- [ ] Fix reply composer: image upload support in ThreadView.
```
to:
```
- [~] Fix reply composer: image upload support in ThreadView.
```
Commit: `git commit -m "chore: mark reply-image in progress"`

- [ ] **Step 2: Write failing test**

Add to `frontend/tests/thread-view.test.ts`:
```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'

vi.mock('@/api/tweets', () => ({
  tweetsApi: {
    getById: vi.fn().mockResolvedValue({
      id: 'tweet-1', text: 'parent tweet', authorUsername: 'alice',
      createdAt: new Date().toISOString(), likeCount: 0, likedByViewer: false,
      parentId: null, imageUrl: null, retweetCount: 0, retweetedByViewer: false,
    }),
    getReplies: vi.fn().mockResolvedValue([]),
    create: vi.fn().mockResolvedValue({
      id: 'reply-1', text: 'my reply', authorUsername: 'bob',
      createdAt: new Date().toISOString(), likeCount: 0, likedByViewer: false,
      parentId: 'tweet-1', imageUrl: null, retweetCount: 0, retweetedByViewer: false,
    }),
    uploadImage: vi.fn().mockResolvedValue('/uploads/test.png'),
  },
}))
vi.mock('@/api/social', () => ({ socialApi: { likeTweet: vi.fn(), unlikeTweet: vi.fn() } }))
vi.mock('@/stores/useMentionsStore', () => ({ useMentionsStore: () => ({ validateBatch: vi.fn() }) }))

import ThreadView from '@/views/ThreadView.vue'
import { tweetsApi } from '@/api/tweets'

const router = createRouter({ history: createMemoryHistory(), routes: [{ path: '/tweet/:id', component: ThreadView }] })

describe('ThreadView', () => {
  beforeEach(() => { setActivePinia(createPinia()); vi.clearAllMocks() })

  it('shows image upload button in reply composer', async () => {
    await router.push('/tweet/tweet-1')
    const wrapper = mount(ThreadView, { global: { plugins: [router, createPinia()] } })
    await flushPromises()
    expect(wrapper.find('[data-testid="reply-image-btn"]').exists()).toBe(true)
  })

  it('uploads image and passes imageUrl on reply submit', async () => {
    await router.push('/tweet/tweet-1')
    const wrapper = mount(ThreadView, { global: { plugins: [router, createPinia()] } })
    await flushPromises()

    const file = new File(['img'], 'photo.png', { type: 'image/png' })
    const input = wrapper.find<HTMLInputElement>('[data-testid="reply-image-input"]')
    Object.defineProperty(input.element, 'files', { value: [file] })
    await input.trigger('change')

    await wrapper.find('textarea').setValue('reply with image')
    await wrapper.find('[data-testid="reply-submit"]').trigger('click')
    await flushPromises()

    expect(tweetsApi.uploadImage).toHaveBeenCalledWith(file)
    expect(tweetsApi.create).toHaveBeenCalledWith(expect.objectContaining({ imageUrl: '/uploads/test.png' }))
  })
})
```

- [ ] **Step 3: Run test — expect FAIL**

```bash
cd frontend && npx vitest run tests/thread-view.test.ts
```
Expected: FAIL — `reply-image-btn` not found.

- [ ] **Step 4: Implement image upload in ThreadView reply composer**

Replace the reply composer `<div>` in `frontend/src/views/ThreadView.vue`. The full `<script setup>` and template section for the composer becomes:

In `<script setup>`, add after `const submitting = ref(false)`:
```ts
const selectedReplyFile = ref<File | null>(null)
const replyImagePreview = ref<string | null>(null)

function onReplyImageChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0] ?? null
  selectedReplyFile.value = file
  replyImagePreview.value = file ? URL.createObjectURL(file) : null
}

function clearReplyImage() {
  selectedReplyFile.value = null
  replyImagePreview.value = null
}
```

Replace `submitReply` function:
```ts
async function submitReply() {
  if (!replyText.value.trim() || !parent.value) return
  submitting.value = true
  try {
    let imageUrl: string | undefined
    if (selectedReplyFile.value) {
      imageUrl = await tweetsApi.uploadImage(selectedReplyFile.value)
    }
    const reply = await tweetsApi.create({ text: replyText.value.trim(), parentId: parent.value.id, imageUrl })
    replies.value = [...replies.value, reply]
    replyText.value = ''
    clearReplyImage()
  } finally {
    submitting.value = false
  }
}
```

Replace the reply composer `<div class="bg-white rounded-2xl shadow p-4 space-y-3">` block with:
```html
<div class="bg-white rounded-2xl shadow p-4 space-y-3">
  <textarea
    v-model="replyText"
    rows="2"
    maxlength="280"
    placeholder="Write a reply…"
    class="w-full resize-none border-none outline-none text-gray-900 placeholder-gray-400 text-sm"
  />
  <div v-if="replyImagePreview" class="relative inline-block">
    <img :src="replyImagePreview" class="max-h-32 rounded-lg object-cover" alt="preview" />
    <button
      class="absolute top-1 right-1 bg-black/50 text-white rounded-full w-5 h-5 text-xs flex items-center justify-center"
      @click="clearReplyImage"
    >✕</button>
  </div>
  <div class="flex items-center justify-between">
    <div class="flex items-center gap-2">
      <label data-testid="reply-image-btn" class="cursor-pointer text-sky-500 hover:text-sky-600 text-sm">
        📷
        <input
          data-testid="reply-image-input"
          type="file"
          accept="image/jpeg,image/png,image/gif,image/webp"
          class="hidden"
          @change="onReplyImageChange"
        />
      </label>
      <span class="text-xs text-gray-400">{{ replyText.length }}/280</span>
    </div>
    <button
      :disabled="!replyText.trim() || submitting"
      data-testid="reply-submit"
      class="bg-sky-500 hover:bg-sky-600 disabled:opacity-40 text-white text-sm font-semibold rounded-full px-5 py-2 transition"
      @click="submitReply"
    >
      {{ submitting ? 'Posting…' : 'Reply' }}
    </button>
  </div>
</div>
```

- [ ] **Step 5: Run tests — expect PASS**

```bash
cd frontend && npx vitest run tests/thread-view.test.ts
```
Expected: PASS (2 tests).

- [ ] **Step 6: Run full frontend check**

```bash
pwsh scripts/check.ps1 -Frontend
```
Expected: CHECK PASSED.

- [ ] **Step 7: Commit**

Update ROADMAP.md `[~]` → `[x]` for reply-image task, then:
```bash
git add frontend/src/views/ThreadView.vue frontend/tests/thread-view.test.ts docs/ROADMAP.md
git commit -m "feat(replies): image upload in reply composer"
```

---

## Task 2: @Mentions — Backend Batch Validate Endpoint

**Files:**
- Modify: `backend/src/TwitterClone.Application/Interfaces/IUserRepository.cs`
- Create: `backend/src/TwitterClone.Application/Users/Queries/ValidateUsernamesQuery.cs`
- Modify: `backend/src/TwitterClone.Infrastructure/Persistence/Repositories/UserRepository.cs`
- Create: `backend/src/TwitterClone.Api/Endpoints/UserEndpoints.cs`
- Modify: `backend/src/TwitterClone.Api/Program.cs`
- Create: `backend/tests/TwitterClone.Application.Tests/Users/ValidateUsernamesQueryTests.cs`
- Create: `backend/tests/TwitterClone.Integration.Tests/Users/ValidateUsernamesEndpointTests.cs`

**Interfaces:**
- Produces: `GET /api/users/validate-usernames?usernames=alice,bob` → `200 { "alice": true, "bob": false }`
- Produces: `IUserRepository.GetExistingUsernamesAsync(IEnumerable<string>, CancellationToken)` → `Task<IReadOnlySet<string>>`

- [ ] **Step 1: Mark roadmap task in progress**

In `docs/ROADMAP.md` change `[ ]` → `[~]` for @mentions task. Commit: `git commit -m "chore: mark mentions in progress"`

- [ ] **Step 2: Write failing application unit test**

Create `backend/tests/TwitterClone.Application.Tests/Users/ValidateUsernamesQueryTests.cs`:
```csharp
using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Users.Queries;

namespace TwitterClone.Application.Tests.Users;

public class ValidateUsernamesQueryTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ValidateUsernamesHandler _handler;

    public ValidateUsernamesQueryTests()
    {
        _handler = new ValidateUsernamesHandler(_users);
    }

    [Fact]
    public async Task HandleAsync_ReturnsCorrectBoolMap()
    {
        _users.GetExistingUsernamesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "alice", "bob" });

        var result = await _handler.HandleAsync(
            new ValidateUsernamesQuery(["alice", "bob", "nobody"]), CancellationToken.None);

        result.Should().BeEquivalentTo(new Dictionary<string, bool>
        {
            ["alice"] = true,
            ["bob"] = true,
            ["nobody"] = false,
        });
    }

    [Fact]
    public async Task HandleAsync_EmptyInput_ReturnsEmptyMap()
    {
        _users.GetExistingUsernamesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<string>());

        var result = await _handler.HandleAsync(new ValidateUsernamesQuery([]), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_TruncatesOver50Usernames()
    {
        var many = Enumerable.Range(1, 60).Select(i => $"user{i}").ToArray();
        _users.GetExistingUsernamesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<string>());

        await _handler.HandleAsync(new ValidateUsernamesQuery(many), CancellationToken.None);

        await _users.Received(1).GetExistingUsernamesAsync(
            Arg.Is<IEnumerable<string>>(u => u.Count() <= 50), Arg.Any<CancellationToken>());
    }
}
```

- [ ] **Step 3: Run test — expect FAIL**

```bash
cd backend && dotnet test tests/TwitterClone.Application.Tests --filter "ValidateUsernamesQueryTests" --no-build 2>&1 | tail -5
```
Expected: build error — types not found yet.

- [ ] **Step 4: Add `GetExistingUsernamesAsync` to IUserRepository**

Append to `backend/src/TwitterClone.Application/Interfaces/IUserRepository.cs`:
```csharp
Task<IReadOnlySet<string>> GetExistingUsernamesAsync(IEnumerable<string> usernames, CancellationToken ct = default);
```

- [ ] **Step 5: Implement in UserRepository**

Append to `backend/src/TwitterClone.Infrastructure/Persistence/Repositories/UserRepository.cs` inside the class:
```csharp
public async Task<IReadOnlySet<string>> GetExistingUsernamesAsync(
    IEnumerable<string> usernames, CancellationToken ct = default)
{
    var list = usernames.ToList();
    var found = await db.Users
        .Where(u => list.Contains(u.Username))
        .Select(u => u.Username)
        .ToListAsync(ct);
    return new HashSet<string>(found, StringComparer.OrdinalIgnoreCase);
}
```

- [ ] **Step 6: Create ValidateUsernamesQuery handler**

Create `backend/src/TwitterClone.Application/Users/Queries/ValidateUsernamesQuery.cs`:
```csharp
using TwitterClone.Application.Interfaces;

namespace TwitterClone.Application.Users.Queries;

public sealed record ValidateUsernamesQuery(IEnumerable<string> Usernames);

public sealed class ValidateUsernamesHandler(IUserRepository users)
{
    private const int MaxUsernames = 50;

    public async Task<IReadOnlyDictionary<string, bool>> HandleAsync(
        ValidateUsernamesQuery query, CancellationToken ct = default)
    {
        var input = query.Usernames.Take(MaxUsernames).ToList();
        if (input.Count == 0)
            return new Dictionary<string, bool>();

        var existing = await users.GetExistingUsernamesAsync(input, ct);
        return input.ToDictionary(u => u, u => existing.Contains(u));
    }
}
```

- [ ] **Step 7: Run application tests — expect PASS**

```bash
cd backend && dotnet test tests/TwitterClone.Application.Tests --filter "ValidateUsernamesQueryTests" 2>&1 | tail -8
```
Expected: 3 tests PASS.

- [ ] **Step 8: Create UserEndpoints**

Create `backend/src/TwitterClone.Api/Endpoints/UserEndpoints.cs`:
```csharp
using TwitterClone.Application.Users.Queries;

namespace TwitterClone.Api.Endpoints;

internal static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/validate-usernames", ValidateUsernamesAsync);
        return app;
    }

    private static async Task<IResult> ValidateUsernamesAsync(
        string? usernames,
        ValidateUsernamesHandler handler,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(usernames))
            return Results.Ok(new Dictionary<string, bool>());

        var parsed = usernames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = await handler.HandleAsync(new ValidateUsernamesQuery(parsed), ct);
        return Results.Ok(result);
    }
}
```

- [ ] **Step 9: Register handler and endpoint in Program.cs**

In `backend/src/TwitterClone.Api/Program.cs`, after the existing handler registrations add:
```csharp
builder.Services.AddScoped<ValidateUsernamesHandler>();
```

After `app.MapSocialEndpoints();` add:
```csharp
app.MapUserEndpoints();
```

- [ ] **Step 10: Write integration test**

Create `backend/tests/TwitterClone.Integration.Tests/Users/ValidateUsernamesEndpointTests.cs`:
```csharp
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace TwitterClone.Integration.Tests.Users;

public class ValidateUsernamesEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task RegisterAsync(string username)
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            email = $"{username}@test.com",
            password = "Password123!",
        });
        resp.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ValidateUsernames_ReturnsCorrectMap()
    {
        await RegisterAsync("mentionuserA");
        await RegisterAsync("mentionuserB");

        var resp = await _client.GetAsync(
            "/api/users/validate-usernames?usernames=mentionuserA,mentionuserB,nobody123");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonSerializer.Deserialize<Dictionary<string, bool>>(
            await resp.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        body["mentionuserA"].Should().BeTrue();
        body["mentionuserB"].Should().BeTrue();
        body["nobody123"].Should().BeFalse();
    }

    [Fact]
    public async Task ValidateUsernames_EmptyParam_ReturnsEmptyObject()
    {
        var resp = await _client.GetAsync("/api/users/validate-usernames");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Be("{}");
    }
}
```

- [ ] **Step 11: Run full backend check**

```bash
pwsh scripts/check.ps1 -Backend
```
Expected: CHECK PASSED.

- [ ] **Step 12: Commit**

Update ROADMAP.md `[~]` → `[x]` for @mentions task (just the backend portion — we'll mark complete in Task 3).
```bash
git add backend/src/ backend/tests/ docs/ROADMAP.md
git commit -m "feat(mentions): batch username validation endpoint"
```

---

## Task 3: @Mentions — Frontend Store, Component, Integration

**Files:**
- Create: `frontend/src/api/users.ts`
- Create: `frontend/src/stores/useMentionsStore.ts`
- Create: `frontend/src/components/MentionText.vue`
- Modify: `frontend/src/views/HomeView.vue`
- Modify: `frontend/src/views/ThreadView.vue`
- Create: `frontend/tests/mention-text.test.ts`

**Interfaces:**
- Consumes: `GET /api/users/validate-usernames?usernames=...` → `{ [username]: boolean }`
- Produces: `useMentionsStore().validateBatch(words: string[]): Promise<void>`
- Produces: `<MentionText :text="string" />` component

- [ ] **Step 1: Write failing MentionText test**

Create `frontend/tests/mention-text.test.ts`:
```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'

vi.mock('@/api/users', () => ({
  usersApi: {
    validateUsernames: vi.fn().mockResolvedValue({ alice: true, nobody: false }),
  },
}))

import MentionText from '@/components/MentionText.vue'
import { useMentionsStore } from '@/stores/useMentionsStore'

const router = createRouter({ history: createMemoryHistory(), routes: [{ path: '/:p(.*)', component: { template: '<div/>' } }] })

describe('MentionText', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('renders plain text with no mentions', () => {
    const wrapper = mount(MentionText, {
      props: { text: 'hello world' },
      global: { plugins: [router, createPinia()] },
    })
    expect(wrapper.text()).toBe('hello world')
    expect(wrapper.find('a').exists()).toBe(false)
  })

  it('renders unknown mention as plain text', async () => {
    const store = useMentionsStore()
    const wrapper = mount(MentionText, {
      props: { text: 'hello @nobody' },
      global: { plugins: [router, createPinia()] },
    })
    await flushPromises()
    expect(wrapper.find('a').exists()).toBe(false)
    expect(wrapper.text()).toContain('@nobody')
  })

  it('renders validated mention as router-link', async () => {
    const store = useMentionsStore()
    store.validatedUsernames.add('alice')
    const wrapper = mount(MentionText, {
      props: { text: 'hello @alice' },
      global: { plugins: [router, createPinia()] },
    })
    await flushPromises()
    expect(wrapper.find('a').exists()).toBe(true)
    expect(wrapper.find('a').text()).toBe('@alice')
  })
})
```

- [ ] **Step 2: Run test — expect FAIL**

```bash
cd frontend && npx vitest run tests/mention-text.test.ts
```
Expected: FAIL — modules not found.

- [ ] **Step 3: Create users API**

Create `frontend/src/api/users.ts`:
```ts
import { apiClient } from './client'

export const usersApi = {
  validateUsernames: (usernames: string[]) =>
    apiClient
      .get<Record<string, boolean>>('/api/users/validate-usernames', {
        params: { usernames: usernames.join(',') },
      })
      .then((r) => r.data),
}
```

- [ ] **Step 4: Create useMentionsStore**

Create `frontend/src/stores/useMentionsStore.ts`:
```ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { usersApi } from '@/api/users'

export const useMentionsStore = defineStore('mentions', () => {
  const validatedUsernames = ref(new Set<string>())
  const pending = ref(new Set<string>())

  async function validateBatch(words: string[]): Promise<void> {
    const unknown = words.filter(
      (w) => !validatedUsernames.value.has(w) && !pending.value.has(w),
    )
    if (unknown.length === 0) return

    unknown.forEach((w) => pending.value.add(w))
    try {
      const result = await usersApi.validateUsernames(unknown)
      Object.entries(result).forEach(([username, exists]) => {
        if (exists) validatedUsernames.value.add(username)
        pending.value.delete(username)
      })
    } catch {
      unknown.forEach((w) => pending.value.delete(w))
    }
  }

  return { validatedUsernames, validateBatch }
})
```

- [ ] **Step 5: Create MentionText component**

Create `frontend/src/components/MentionText.vue`:
```vue
<template>
  <span>
    <template v-for="(part, i) in parts" :key="i">
      <RouterLink
        v-if="part.type === 'mention'"
        :to="`/profile/${part.value}`"
        class="text-sky-500 hover:underline"
      >@{{ part.value }}</RouterLink>
      <span v-else>{{ part.value }}</span>
    </template>
  </span>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { useMentionsStore } from '@/stores/useMentionsStore'

const props = defineProps<{ text: string }>()
const mentionsStore = useMentionsStore()

const parts = computed(() => {
  const segments: { type: 'text' | 'mention'; value: string }[] = []
  const regex = /@(\w+)/g
  let last = 0
  let m: RegExpExecArray | null

  while ((m = regex.exec(props.text)) !== null) {
    if (m.index > last) segments.push({ type: 'text', value: props.text.slice(last, m.index) })
    const username = m[1]
    if (mentionsStore.validatedUsernames.value.has(username)) {
      segments.push({ type: 'mention', value: username })
    } else {
      segments.push({ type: 'text', value: m[0] })
    }
    last = m.index + m[0].length
  }

  if (last < props.text.length) segments.push({ type: 'text', value: props.text.slice(last) })
  return segments
})

onMounted(() => {
  const mentions = [...props.text.matchAll(/@(\w+)/g)].map((m) => m[1])
  if (mentions.length > 0) mentionsStore.validateBatch(mentions)
})
</script>
```

- [ ] **Step 6: Run MentionText tests — expect PASS**

```bash
cd frontend && npx vitest run tests/mention-text.test.ts
```
Expected: 3 tests PASS.

- [ ] **Step 7: Integrate MentionText in HomeView**

In `frontend/src/views/HomeView.vue`:

Add import at top of `<script setup>`:
```ts
import MentionText from '@/components/MentionText.vue'
import { useMentionsStore } from '@/stores/useMentionsStore'
```

Add store reference after other store refs:
```ts
const mentionsStore = useMentionsStore()
```

After fetching tweets in `onMounted` and in the infinite scroll loader, collect and validate mentions. Find where `timeline.value` is set (after `tweets.loadTimeline()`) and add:
```ts
const allMentions = tweets.timeline
  .flatMap((t) => [...t.text.matchAll(/@(\w+)/g)].map((m) => m[1]))
const unique = [...new Set(allMentions)]
if (unique.length) mentionsStore.validateBatch(unique)
```

Replace `<p class="text-gray-800 ...">{{ tweet.text }}</p>` in the tweet card with:
```html
<MentionText :text="tweet.text" class="text-gray-800 text-sm md:text-base whitespace-pre-wrap" />
```

- [ ] **Step 8: Integrate MentionText in ThreadView**

In `frontend/src/views/ThreadView.vue`:

Add import:
```ts
import MentionText from '@/components/MentionText.vue'
import { useMentionsStore } from '@/stores/useMentionsStore'
```

Add store reference:
```ts
const mentionsStore = useMentionsStore()
```

After `parent.value = tweet` in `onMounted`:
```ts
const allText = [tweet.text, ...reps.map((r) => r.text)]
const mentions = allText.flatMap((t) => [...t.matchAll(/@(\w+)/g)].map((m) => m[1]))
const unique = [...new Set(mentions)]
if (unique.length) mentionsStore.validateBatch(unique)
```

Replace parent tweet `<p class="text-gray-800 ...">{{ parent.text }}</p>` with:
```html
<MentionText :text="parent.text" class="text-gray-800 text-sm md:text-base whitespace-pre-wrap" />
```

Replace reply `<p class="text-gray-800 ...">{{ reply.text }}</p>` with:
```html
<MentionText :text="reply.text" class="text-gray-800 text-sm whitespace-pre-wrap" />
```

- [ ] **Step 9: Run full frontend check**

```bash
pwsh scripts/check.ps1 -Frontend
```
Expected: CHECK PASSED.

- [ ] **Step 10: Commit**

```bash
git add frontend/src/ frontend/tests/mention-text.test.ts docs/ROADMAP.md
git commit -m "feat(mentions): clickable @mentions with batch validation and cache"
```

---

## Task 4: Retweet Domain Entity

**Files:**
- Create: `backend/src/TwitterClone.Domain/Entities/Retweet.cs`
- Create: `backend/tests/TwitterClone.Domain.Tests/RetweetTests.cs`

**Interfaces:**
- Produces: `Retweet.Create(retweeterId: Guid, tweetId: Guid, authorId: Guid): Retweet`
- Produces: `Retweet { Id, RetweeterId, TweetId, CreatedAt }`

- [ ] **Step 1: Mark roadmap task in progress**

In `docs/ROADMAP.md` change `[ ]` → `[~]` for retweet task. Commit: `git commit -m "chore: mark retweet in progress"`

- [ ] **Step 2: Write failing domain test**

Create `backend/tests/TwitterClone.Domain.Tests/RetweetTests.cs`:
```csharp
using FluentAssertions;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Domain.Tests;

public class RetweetTests
{
    [Fact]
    public void Create_ValidIds_SetsProperties()
    {
        var retweeterId = Guid.NewGuid();
        var tweetId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var rt = Retweet.Create(retweeterId, tweetId, authorId);

        rt.RetweeterId.Should().Be(retweeterId);
        rt.TweetId.Should().Be(tweetId);
        rt.Id.Should().NotBeEmpty();
        rt.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_RetweeterIsAuthor_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var tweetId = Guid.NewGuid();

        var act = () => Retweet.Create(userId, tweetId, authorId: userId);

        act.Should().Throw<DomainException>().WithMessage("*own tweet*");
    }
}
```

- [ ] **Step 3: Run test — expect FAIL**

```bash
cd backend && dotnet test tests/TwitterClone.Domain.Tests --filter "RetweetTests" 2>&1 | tail -5
```
Expected: build error — `Retweet` not found.

- [ ] **Step 4: Create Retweet entity**

Create `backend/src/TwitterClone.Domain/Entities/Retweet.cs`:
```csharp
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Domain.Entities;

public sealed class Retweet
{
    public Guid Id { get; private set; }
    public Guid RetweeterId { get; private set; }
    public Guid TweetId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Retweet() { }

    public static Retweet Create(Guid retweeterId, Guid tweetId, Guid authorId)
    {
        if (retweeterId == authorId)
            throw new DomainException("Cannot retweet own tweet.");

        return new Retweet
        {
            Id = Guid.NewGuid(),
            RetweeterId = retweeterId,
            TweetId = tweetId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
```

- [ ] **Step 5: Run domain tests — expect PASS**

```bash
cd backend && dotnet test tests/TwitterClone.Domain.Tests --filter "RetweetTests" 2>&1 | tail -5
```
Expected: 2 tests PASS.

- [ ] **Step 6: Run backend check**

```bash
pwsh scripts/check.ps1 -Backend
```
Expected: CHECK PASSED.

- [ ] **Step 7: Commit**

```bash
git add backend/src/TwitterClone.Domain/ backend/tests/TwitterClone.Domain.Tests/
git commit -m "feat(retweet): Retweet domain entity with own-tweet guard"
```

---

## Task 5: Retweet Application Layer

**Files:**
- Create: `backend/src/TwitterClone.Application/Interfaces/IRetweetRepository.cs`
- Modify: `backend/src/TwitterClone.Application/Tweets/Dtos/TweetDto.cs`
- Create: `backend/src/TwitterClone.Application/Tweets/Commands/RetweetCommand.cs`
- Create: `backend/src/TwitterClone.Application/Tweets/Commands/UnretweetCommand.cs`
- Modify: `backend/src/TwitterClone.Application/Tweets/Queries/GetTimelineQuery.cs`
- Create: `backend/tests/TwitterClone.Application.Tests/Tweets/RetweetHandlerTests.cs`

**Interfaces:**
- Produces: `IRetweetRepository` (see Step 1)
- Produces: `TweetDto` with `RetweetCount`, `RetweetedByViewer`, `IsRetweet`, `RetweetedByUsername` fields
- Produces: `RetweetHandler.HandleAsync(RetweetCommand, CancellationToken)`
- Produces: `UnretweetHandler.HandleAsync(UnretweetCommand, CancellationToken)`
- Consumes: `IRetweetRepository`, `ITweetRepository`, `IUserRepository`

- [ ] **Step 1: Create IRetweetRepository**

Create `backend/src/TwitterClone.Application/Interfaces/IRetweetRepository.cs`:
```csharp
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Interfaces;

public interface IRetweetRepository
{
    Task<bool> ExistsAsync(Guid retweeterId, Guid tweetId, CancellationToken ct = default);
    Task AddAsync(Retweet retweet, CancellationToken ct = default);
    Task RemoveAsync(Guid retweeterId, Guid tweetId, CancellationToken ct = default);
    Task<int> CountAsync(Guid tweetId, CancellationToken ct = default);
    Task<IReadOnlyList<(Tweet Tweet, string RetweeterUsername, DateTime RetweetedAt)>>
        GetTimelineRetweetsAsync(Guid viewerId, int count, CancellationToken ct = default);
}
```

- [ ] **Step 2: Extend TweetDto**

Replace `backend/src/TwitterClone.Application/Tweets/Dtos/TweetDto.cs` content:
```csharp
namespace TwitterClone.Application.Tweets.Dtos;

public sealed record TweetDto(
    Guid Id,
    Guid AuthorId,
    string AuthorUsername,
    string Text,
    Guid? ParentId,
    string? ImageUrl,
    DateTime CreatedAt,
    int LikeCount = 0,
    bool LikedByViewer = false,
    int RetweetCount = 0,
    bool RetweetedByViewer = false,
    bool IsRetweet = false,
    string? RetweetedByUsername = null);
```

- [ ] **Step 3: Write failing handler tests**

Create `backend/tests/TwitterClone.Application.Tests/Tweets/RetweetHandlerTests.cs`:
```csharp
using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Commands;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Application.Tests.Tweets;

public class RetweetHandlerTests
{
    private readonly IRetweetRepository _retweets = Substitute.For<IRetweetRepository>();
    private readonly ITweetRepository _tweets = Substitute.For<ITweetRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task RetweetHandler_NewRetweet_AddsAndSaves()
    {
        var retweeterId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var tweet = CreateTweet(authorId);

        _tweets.GetByIdAsync(tweet.Id, Arg.Any<CancellationToken>()).Returns(tweet);
        _retweets.ExistsAsync(retweeterId, tweet.Id, Arg.Any<CancellationToken>()).Returns(false);

        var handler = new RetweetHandler(_retweets, _tweets, _uow);
        await handler.HandleAsync(new RetweetCommand(retweeterId, tweet.Id));

        await _retweets.Received(1).AddAsync(Arg.Any<Retweet>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetweetHandler_AlreadyRetweeted_DoesNotDuplicate()
    {
        var retweeterId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var tweet = CreateTweet(authorId);

        _tweets.GetByIdAsync(tweet.Id, Arg.Any<CancellationToken>()).Returns(tweet);
        _retweets.ExistsAsync(retweeterId, tweet.Id, Arg.Any<CancellationToken>()).Returns(true);

        var handler = new RetweetHandler(_retweets, _tweets, _uow);
        await handler.HandleAsync(new RetweetCommand(retweeterId, tweet.Id));

        await _retweets.DidNotReceive().AddAsync(Arg.Any<Retweet>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetweetHandler_OwnTweet_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var tweet = CreateTweet(userId);

        _tweets.GetByIdAsync(tweet.Id, Arg.Any<CancellationToken>()).Returns(tweet);
        _retweets.ExistsAsync(userId, tweet.Id, Arg.Any<CancellationToken>()).Returns(false);

        var handler = new RetweetHandler(_retweets, _tweets, _uow);
        var act = async () => await handler.HandleAsync(new RetweetCommand(userId, tweet.Id));

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UnretweetHandler_ExistingRetweet_Removes()
    {
        var retweeterId = Guid.NewGuid();
        var tweetId = Guid.NewGuid();
        _retweets.ExistsAsync(retweeterId, tweetId, Arg.Any<CancellationToken>()).Returns(true);

        var handler = new UnretweetHandler(_retweets, _uow);
        await handler.HandleAsync(new UnretweetCommand(retweeterId, tweetId));

        await _retweets.Received(1).RemoveAsync(retweeterId, tweetId, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static Tweet CreateTweet(Guid authorId) =>
        Tweet.Create(authorId, "test tweet");
}
```

- [ ] **Step 4: Run tests — expect FAIL**

```bash
cd backend && dotnet test tests/TwitterClone.Application.Tests --filter "RetweetHandlerTests" 2>&1 | tail -5
```
Expected: build error — handlers not found.

- [ ] **Step 5: Create RetweetCommand handler**

Create `backend/src/TwitterClone.Application/Tweets/Commands/RetweetCommand.cs`:
```csharp
using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tweets.Commands;

public sealed record RetweetCommand(Guid RetweeterId, Guid TweetId);

public sealed class RetweetHandler(IRetweetRepository retweets, ITweetRepository tweets, IUnitOfWork uow)
{
    public async Task HandleAsync(RetweetCommand cmd, CancellationToken ct = default)
    {
        var already = await retweets.ExistsAsync(cmd.RetweeterId, cmd.TweetId, ct);
        if (already) return;

        var tweet = await tweets.GetByIdAsync(cmd.TweetId, ct)
            ?? throw new InvalidOperationException("Tweet not found.");

        var retweet = Retweet.Create(cmd.RetweeterId, cmd.TweetId, tweet.AuthorId);
        await retweets.AddAsync(retweet, ct);
        await uow.SaveChangesAsync(ct);
    }
}
```

- [ ] **Step 6: Create UnretweetCommand handler**

Create `backend/src/TwitterClone.Application/Tweets/Commands/UnretweetCommand.cs`:
```csharp
using TwitterClone.Application.Interfaces;

namespace TwitterClone.Application.Tweets.Commands;

public sealed record UnretweetCommand(Guid RetweeterId, Guid TweetId);

public sealed class UnretweetHandler(IRetweetRepository retweets, IUnitOfWork uow)
{
    public async Task HandleAsync(UnretweetCommand cmd, CancellationToken ct = default)
    {
        var exists = await retweets.ExistsAsync(cmd.RetweeterId, cmd.TweetId, ct);
        if (!exists) return;

        await retweets.RemoveAsync(cmd.RetweeterId, cmd.TweetId, ct);
        await uow.SaveChangesAsync(ct);
    }
}
```

- [ ] **Step 7: Update GetTimelineHandler to include retweets**

Replace `backend/src/TwitterClone.Application/Tweets/Queries/GetTimelineQuery.cs`:
```csharp
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Dtos;

namespace TwitterClone.Application.Tweets.Queries;

public sealed record GetTimelineQuery(Guid UserId, int Page = 1, int PageSize = 20);

public sealed class GetTimelineHandler(
    ITweetRepository tweets,
    IUserRepository users,
    ILikeRepository likes,
    IRetweetRepository retweets)
{
    public async Task<IReadOnlyList<TweetDto>> HandleAsync(GetTimelineQuery query, CancellationToken ct = default)
    {
        var fetchCount = query.PageSize * 2;

        var tweetEntities = await tweets.GetTimelineAsync(query.UserId, 1, fetchCount, ct);
        var retweetEntries = await retweets.GetTimelineRetweetsAsync(query.UserId, fetchCount, ct);

        var authorIds = tweetEntities.Select(t => t.AuthorId)
            .Concat(retweetEntries.Select(r => r.Tweet.AuthorId))
            .Distinct()
            .ToList();

        var authorMap = new Dictionary<Guid, string>();
        foreach (var id in authorIds)
        {
            var user = await users.GetByIdAsync(id, ct);
            if (user is not null) authorMap[id] = user.Username;
        }

        var result = new List<(TweetDto Dto, DateTime DisplayAt)>();

        foreach (var t in tweetEntities)
        {
            var likeCount = await likes.CountAsync(t.Id, ct);
            var likedByViewer = await likes.GetAsync(query.UserId, t.Id, ct) is not null;
            var retweetCount = await retweets.CountAsync(t.Id, ct);
            var retweetedByViewer = await retweets.ExistsAsync(query.UserId, t.Id, ct);
            result.Add((new TweetDto(
                t.Id, t.AuthorId,
                authorMap.TryGetValue(t.AuthorId, out var u) ? u : "unknown",
                t.Text, t.ParentId, t.ImageUrl, t.CreatedAt,
                likeCount, likedByViewer, retweetCount, retweetedByViewer), t.CreatedAt));
        }

        foreach (var (tweet, retweeterUsername, retweetedAt) in retweetEntries)
        {
            var likeCount = await likes.CountAsync(tweet.Id, ct);
            var likedByViewer = await likes.GetAsync(query.UserId, tweet.Id, ct) is not null;
            var retweetCount = await retweets.CountAsync(tweet.Id, ct);
            var retweetedByViewer = await retweets.ExistsAsync(query.UserId, tweet.Id, ct);
            result.Add((new TweetDto(
                tweet.Id, tweet.AuthorId,
                authorMap.TryGetValue(tweet.AuthorId, out var u2) ? u2 : "unknown",
                tweet.Text, tweet.ParentId, tweet.ImageUrl, tweet.CreatedAt,
                likeCount, likedByViewer, retweetCount, retweetedByViewer,
                IsRetweet: true, RetweetedByUsername: retweeterUsername), retweetedAt));
        }

        return result
            .OrderByDescending(x => x.DisplayAt)
            .DistinctBy(x => (x.Dto.Id, x.Dto.IsRetweet ? x.Dto.RetweetedByUsername : null))
            .Take(query.PageSize)
            .Select(x => x.Dto)
            .ToList();
    }
}
```

- [ ] **Step 8: Run application tests — expect PASS**

```bash
cd backend && dotnet test tests/TwitterClone.Application.Tests 2>&1 | tail -8
```
Expected: all tests PASS (new RetweetHandlerTests + existing tests).

- [ ] **Step 9: Run backend check (will fail on infrastructure — expected)**

```bash
pwsh scripts/check.ps1 -Backend
```
Expected: build FAIL — `IRetweetRepository` not implemented yet. Proceed to Task 6.

---

## Task 6: Retweet Infrastructure (EF Core + Migration + DI)

**Files:**
- Create: `backend/src/TwitterClone.Infrastructure/Persistence/Configurations/RetweetConfiguration.cs`
- Modify: `backend/src/TwitterClone.Infrastructure/Persistence/AppDbContext.cs`
- Create: `backend/src/TwitterClone.Infrastructure/Persistence/Repositories/RetweetRepository.cs`
- Modify: `backend/src/TwitterClone.Infrastructure/DependencyInjection.cs`
- Create: EF Core migration `AddRetweetsTable`

**Interfaces:**
- Produces: `RetweetRepository : IRetweetRepository`

- [ ] **Step 1: Create RetweetConfiguration**

Create `backend/src/TwitterClone.Infrastructure/Persistence/Configurations/RetweetConfiguration.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Infrastructure.Persistence.Configurations;

internal sealed class RetweetConfiguration : IEntityTypeConfiguration<Retweet>
{
    public void Configure(EntityTypeBuilder<Retweet> builder)
    {
        builder.ToTable("Retweets");
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.RetweeterId, r.TweetId }).IsUnique();
        builder.HasIndex(r => r.TweetId);
    }
}
```

- [ ] **Step 2: Add Retweets DbSet to AppDbContext**

In `backend/src/TwitterClone.Infrastructure/Persistence/AppDbContext.cs`, add:
```csharp
public DbSet<Retweet> Retweets => Set<Retweet>();
```

- [ ] **Step 3: Create RetweetRepository**

Create `backend/src/TwitterClone.Infrastructure/Persistence/Repositories/RetweetRepository.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Infrastructure.Persistence.Repositories;

internal sealed class RetweetRepository(AppDbContext db) : IRetweetRepository
{
    public Task<bool> ExistsAsync(Guid retweeterId, Guid tweetId, CancellationToken ct = default) =>
        db.Retweets.AnyAsync(r => r.RetweeterId == retweeterId && r.TweetId == tweetId, ct);

    public async Task AddAsync(Retweet retweet, CancellationToken ct = default) =>
        await db.Retweets.AddAsync(retweet, ct);

    public async Task RemoveAsync(Guid retweeterId, Guid tweetId, CancellationToken ct = default)
    {
        var rt = await db.Retweets
            .FirstOrDefaultAsync(r => r.RetweeterId == retweeterId && r.TweetId == tweetId, ct);
        if (rt is not null) db.Retweets.Remove(rt);
    }

    public Task<int> CountAsync(Guid tweetId, CancellationToken ct = default) =>
        db.Retweets.CountAsync(r => r.TweetId == tweetId, ct);

    public async Task<IReadOnlyList<(Tweet Tweet, string RetweeterUsername, DateTime RetweetedAt)>>
        GetTimelineRetweetsAsync(Guid viewerId, int count, CancellationToken ct = default)
    {
        var followedIds = await db.Follows
            .Where(f => f.FollowerId == viewerId)
            .Select(f => f.FolloweeId)
            .ToListAsync(ct);

        followedIds.Add(viewerId);

        var rows = await db.Retweets
            .Where(r => followedIds.Contains(r.RetweeterId))
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .Join(db.Tweets, r => r.TweetId, t => t.Id, (r, t) => new { r, t })
            .Join(db.Users, x => x.r.RetweeterId, u => u.Id,
                (x, u) => new { x.t, RetweeterUsername = u.Username, x.r.CreatedAt })
            .ToListAsync(ct);

        return rows.Select(x => (x.t, x.RetweeterUsername, x.CreatedAt)).ToList();
    }
}
```

- [ ] **Step 4: Register in DependencyInjection**

In `backend/src/TwitterClone.Infrastructure/DependencyInjection.cs`, after `services.AddScoped<ILikeRepository, LikeRepository>();` add:
```csharp
services.AddScoped<IRetweetRepository, RetweetRepository>();
```

- [ ] **Step 5: Register handlers in Program.cs**

In `backend/src/TwitterClone.Api/Program.cs`, after the other handler registrations, add:
```csharp
builder.Services.AddScoped<RetweetHandler>();
builder.Services.AddScoped<UnretweetHandler>();
```

Add the using at the top with other application usings:
```csharp
using TwitterClone.Application.Tweets.Commands;
```

- [ ] **Step 6: Create EF migration**

```bash
cd backend
dotnet ef migrations add AddRetweetsTable \
  --project src/TwitterClone.Infrastructure \
  --startup-project src/TwitterClone.Api
```
Expected: new migration file created in `src/TwitterClone.Infrastructure/Migrations/`.

- [ ] **Step 7: Run backend check**

```bash
pwsh scripts/check.ps1 -Backend
```
Expected: CHECK PASSED.

- [ ] **Step 8: Commit**

```bash
git add backend/src/ backend/tests/
git commit -m "feat(retweet): infrastructure — RetweetRepository, EF config, migration"
```

---

## Task 7: Retweet API Endpoints + Integration Tests

**Files:**
- Modify: `backend/src/TwitterClone.Api/Endpoints/SocialEndpoints.cs`
- Create: `backend/tests/TwitterClone.Integration.Tests/Tweets/RetweetEndpointTests.cs`

**Interfaces:**
- Produces: `POST /api/tweets/{id}/retweet` → 200 `{ retweetCount: int }`
- Produces: `DELETE /api/tweets/{id}/retweet` → 204

- [ ] **Step 1: Write failing integration test**

Create `backend/tests/TwitterClone.Integration.Tests/Tweets/RetweetEndpointTests.cs`:
```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace TwitterClone.Integration.Tests.Tweets;

public class RetweetEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(string token, string username, Guid userId)> RegisterAsync(string suffix)
    {
        var username = $"rt{suffix}";
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            email = $"rt{suffix}@example.com",
            password = "Password123!",
        });
        resp.EnsureSuccessStatusCode();
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        return (body.GetProperty("accessToken").GetString()!, username,
                Guid.Parse(body.GetProperty("userId").GetString()!));
    }

    private async Task<string> CreateTweetAsync(string token, string text)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var resp = await _client.PostAsJsonAsync("/api/tweets", new { text });
        resp.EnsureSuccessStatusCode();
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        return body.GetProperty("id").GetString()!;
    }

    [Fact]
    public async Task Retweet_OtherUsersTweet_Returns200WithCount()
    {
        var (tokenA, _, _) = await RegisterAsync("ra1");
        var (tokenB, _, _) = await RegisterAsync("ra2");

        var tweetId = await CreateTweetAsync(tokenA, "retweet me");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var resp = await _client.PostAsync($"/api/tweets/{tweetId}/retweet", null);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        body.GetProperty("retweetCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task Retweet_OwnTweet_Returns400()
    {
        var (tokenA, _, _) = await RegisterAsync("rb1");
        var tweetId = await CreateTweetAsync(tokenA, "cant retweet own");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var resp = await _client.PostAsync($"/api/tweets/{tweetId}/retweet", null);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Unretweet_ExistingRetweet_Returns204()
    {
        var (tokenA, _, _) = await RegisterAsync("rc1");
        var (tokenB, _, _) = await RegisterAsync("rc2");

        var tweetId = await CreateTweetAsync(tokenA, "unretweet me");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        await _client.PostAsync($"/api/tweets/{tweetId}/retweet", null);

        var resp = await _client.DeleteAsync($"/api/tweets/{tweetId}/retweet");
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Timeline_IncludesRetweetsFromFollowedUsers()
    {
        var (tokenA, usernameA, _) = await RegisterAsync("rd1");
        var (tokenB, usernameB, _) = await RegisterAsync("rd2");
        var (tokenC, _, _) = await RegisterAsync("rd3");

        // C follows A; A retweets B's tweet; C's timeline should show the retweet
        var tweetId = await CreateTweetAsync(tokenB, "original tweet from B");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        await _client.PostAsync($"/api/users/{usernameB}/follow", null);
        await _client.PostAsync($"/api/tweets/{tweetId}/retweet", null);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenC);
        await _client.PostAsync($"/api/users/{usernameA}/follow", null);

        var resp = await _client.GetAsync("/api/timeline");
        resp.EnsureSuccessStatusCode();
        var tweets = JsonSerializer.Deserialize<JsonElement[]>(
            await resp.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        tweets.Should().Contain(t =>
            t.GetProperty("id").GetString() == tweetId &&
            t.GetProperty("isRetweet").GetBoolean() == true &&
            t.GetProperty("retweetedByUsername").GetString() == usernameA);
    }
}
```

- [ ] **Step 2: Run test — expect FAIL**

```bash
cd backend && dotnet test tests/TwitterClone.Integration.Tests --filter "RetweetEndpointTests" 2>&1 | tail -8
```
Expected: FAIL — endpoints not registered yet.

- [ ] **Step 3: Add retweet endpoints to SocialEndpoints**

Add to `MapSocialEndpoints` in `backend/src/TwitterClone.Api/Endpoints/SocialEndpoints.cs`, after the like routes:
```csharp
group.MapPost("/tweets/{id:guid}/retweet", RetweetAsync);
group.MapDelete("/tweets/{id:guid}/retweet", UnretweetAsync);
```

Add the handler methods:
```csharp
private static async Task<IResult> RetweetAsync(
    Guid id,
    RetweetHandler handler,
    IRetweetRepository retweetRepo,
    HttpContext ctx,
    CancellationToken ct)
{
    var userId = GetUserId(ctx);
    if (userId is null) return Results.Unauthorized();

    try
    {
        await handler.HandleAsync(new RetweetCommand(userId.Value, id), ct);
    }
    catch (TwitterClone.Domain.Exceptions.DomainException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }

    var count = await retweetRepo.CountAsync(id, ct);
    return Results.Ok(new { retweetCount = count });
}

private static async Task<IResult> UnretweetAsync(
    Guid id,
    UnretweetHandler handler,
    HttpContext ctx,
    CancellationToken ct)
{
    var userId = GetUserId(ctx);
    if (userId is null) return Results.Unauthorized();

    await handler.HandleAsync(new UnretweetCommand(userId.Value, id), ct);
    return Results.NoContent();
}
```

Add usings at top of file:
```csharp
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Commands;
```

- [ ] **Step 4: Run integration tests — expect PASS**

```bash
cd backend && dotnet test tests/TwitterClone.Integration.Tests --filter "RetweetEndpointTests" 2>&1 | tail -10
```
Expected: 4 tests PASS.

- [ ] **Step 5: Run full backend check**

```bash
pwsh scripts/check.ps1 -Backend
```
Expected: CHECK PASSED.

- [ ] **Step 6: Commit**

```bash
git add backend/src/TwitterClone.Api/ backend/tests/TwitterClone.Integration.Tests/
git commit -m "feat(retweet): retweet/unretweet API endpoints with integration tests"
```

---

## Task 8: Retweet Frontend

**Files:**
- Modify: `frontend/src/types/tweet.ts`
- Modify: `frontend/src/api/social.ts`
- Modify: `frontend/src/views/HomeView.vue`

**Interfaces:**
- Consumes: `POST /api/tweets/{id}/retweet` → `{ retweetCount: number }`
- Consumes: `DELETE /api/tweets/{id}/retweet` → 204
- Consumes: timeline entry with `isRetweet`, `retweetedByUsername`, `retweetCount`, `retweetedByViewer`

- [ ] **Step 1: Extend Tweet type**

Replace `frontend/src/types/tweet.ts`:
```ts
export interface Tweet {
  id: string
  authorId: string
  authorUsername: string
  text: string
  parentId: string | null
  imageUrl: string | null
  createdAt: string
  likeCount: number
  likedByViewer: boolean
  retweetCount: number
  retweetedByViewer: boolean
  isRetweet?: boolean
  retweetedByUsername?: string
}

export interface CreateTweetRequest {
  text: string
  parentId?: string
  imageUrl?: string
}
```

- [ ] **Step 2: Add retweet methods to socialApi**

In `frontend/src/api/social.ts`, add to the `socialApi` object:
```ts
retweet: (tweetId: string) =>
  apiClient.post<{ retweetCount: number }>(`/api/tweets/${tweetId}/retweet`).then((r) => r.data),
unretweet: (tweetId: string) => apiClient.delete(`/api/tweets/${tweetId}/retweet`),
```

- [ ] **Step 3: Add retweet button and retweet header to HomeView tweet card**

In `frontend/src/views/HomeView.vue`, find the tweet card article block and:

1. Add retweet header above the card when `tweet.isRetweet`:
```html
<div v-if="tweet.isRetweet" class="text-xs text-gray-400 px-2 -mb-2">
  🔁 @{{ tweet.retweetedByUsername }} retweeted
</div>
```
Place this immediately before the `<article>` tag in the `v-for` loop.

2. Add retweet button alongside the like button in the tweet card actions:
```html
<button
  class="flex items-center gap-1 transition min-h-[44px] text-xs"
  :class="tweet.retweetedByViewer ? 'text-green-500' : 'text-gray-400 hover:text-green-400'"
  @click="toggleRetweet(tweet)"
>
  🔁 {{ tweet.retweetCount }}
</button>
```

3. Add `toggleRetweet` function in `<script setup>`:
```ts
async function toggleRetweet(tweet: Tweet) {
  if (tweet.retweetedByViewer) {
    await socialApi.unretweet(tweet.id)
    tweet.retweetCount--
    tweet.retweetedByViewer = false
  } else {
    const result = await socialApi.retweet(tweet.id)
    tweet.retweetCount = result.retweetCount
    tweet.retweetedByViewer = true
  }
}
```

- [ ] **Step 4: Run frontend check**

```bash
pwsh scripts/check.ps1 -Frontend
```
Expected: CHECK PASSED.

- [ ] **Step 5: Update ROADMAP and commit**

In `docs/ROADMAP.md` change `[~]` → `[x]` for the retweet task.

```bash
git add frontend/src/ docs/ROADMAP.md
git commit -m "feat(retweet): retweet button, counter, and timeline retweet entries"
```

---

## Task 9: Rebuild Docker + Final Verification

- [ ] **Step 1: Run full check suite**

```bash
pwsh scripts/check.ps1
```
Expected: CHECK PASSED (backend + frontend).

- [ ] **Step 2: Rebuild and restart Docker**

```bash
docker compose build && docker compose up -d
```

- [ ] **Step 3: Smoke test**
- Login as `alice@example.com` / `Seed1234!`
- Reply to a tweet with an image — verify image shows in thread
- Post a tweet mentioning `@bob` — verify link appears
- Retweet `bob`'s tweet — verify 🔁 counter increments
- Log in as another user who follows alice — verify retweet appears in their timeline with "🔁 @alice retweeted"
