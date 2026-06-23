import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import HomeView from '@/views/HomeView.vue'

vi.mock('@/api/tweets', () => ({
  tweetsApi: {
    getTimeline: vi.fn(),
    create: vi.fn(),
    delete: vi.fn(),
    uploadImage: vi.fn(),
    getById: vi.fn(),
    getReplies: vi.fn(),
  },
}))

vi.mock('@/api/social', () => ({
  socialApi: {
    likeTweet: vi.fn(),
    unlikeTweet: vi.fn(),
    retweet: vi.fn(),
    undoRetweet: vi.fn(),
    getProfile: vi.fn(),
    follow: vi.fn(),
    unfollow: vi.fn(),
    getFollowers: vi.fn(),
    getFollowing: vi.fn(),
    updateProfile: vi.fn(),
  },
}))

vi.mock('@/stores/useAuthStore', () => ({
  useAuthStore: () => ({
    user: { id: 'u1', username: 'alice' },
    logout: vi.fn(),
  }),
}))

vi.mock('@/stores/useMentionsStore', () => ({
  useMentionsStore: () => ({
    validateBatch: vi.fn(),
    knownUsers: [],
  }),
}))

vi.mock('@/composables/useSignalR', () => ({
  useTimelineHub: () => ({ stop: vi.fn() }),
}))

import { tweetsApi } from '@/api/tweets'

const sampleTweet = {
  id: 'new-id',
  text: 'Hello world',
  authorId: 'u1',
  authorUsername: 'alice',
  authorDisplayName: 'Alice',
  authorAvatarUrl: null,
  imageUrl: null,
  likeCount: 0,
  retweetCount: 0,
  replyCount: 0,
  likedByViewer: false,
  retweetedByViewer: false,
  isRetweet: false,
  retweetedByUsername: null,
  parentId: null,
  createdAt: new Date().toISOString(),
}

function mountHome() {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: HomeView },
      { path: '/tweet/:id', component: { template: '<div/>' } },
      { path: '/profile/:username', component: { template: '<div/>' } },
    ],
  })
  router.push('/')
  return mount(HomeView, { global: { plugins: [createPinia(), router] } })
}

describe('Create tweet flow', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.mocked(tweetsApi.getTimeline).mockResolvedValue([])
  })

  it('Post button disabled when composer is empty', async () => {
    const wrapper = mountHome()
    await flushPromises()
    const btn = wrapper.find('[data-testid="post-submit"]')
    expect((btn.element as HTMLButtonElement).disabled).toBe(true)
  })

  it('Post button enabled after typing text', async () => {
    const wrapper = mountHome()
    await flushPromises()
    await wrapper.find('textarea').setValue('Hello world')
    const btn = wrapper.find('[data-testid="post-submit"]')
    expect((btn.element as HTMLButtonElement).disabled).toBe(false)
  })

  it('calls tweetsApi.create and clears composer on success', async () => {
    vi.mocked(tweetsApi.create).mockResolvedValue(sampleTweet)

    const wrapper = mountHome()
    await flushPromises()

    await wrapper.find('textarea').setValue('Hello world')
    await wrapper.find('[data-testid="post-submit"]').trigger('click')
    await flushPromises()

    expect(tweetsApi.create).toHaveBeenCalledWith({ text: 'Hello world' })
    expect((wrapper.find('textarea').element as HTMLTextAreaElement).value).toBe('')
  })
})
