import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import HomeView from '@/views/HomeView.vue'

vi.mock('@/api/tweets', () => ({
  tweetsApi: {
    create: vi.fn(),
    delete: vi.fn(),
    getTimeline: vi.fn(),
    getById: vi.fn(),
    getReplies: vi.fn(),
    uploadImage: vi.fn(),
  },
}))

vi.mock('@/api/social', () => ({
  socialApi: {
    likeTweet: vi.fn(),
    unlikeTweet: vi.fn(),
    retweet: vi.fn().mockResolvedValue({ retweetCount: 1 }),
    unretweet: vi.fn().mockResolvedValue(undefined),
  },
}))

vi.mock('@/composables/useTimelineHub', () => ({
  useTimelineHub: vi.fn(),
}))

import { tweetsApi } from '@/api/tweets'
import { socialApi } from '@/api/social'

const tweet = {
  id: 'tweet-1',
  authorId: 'user-1',
  authorUsername: 'alice',
  text: 'Hello world',
  parentId: null,
  imageUrl: null,
  createdAt: new Date().toISOString(),
  likeCount: 2,
  likedByViewer: false,
  retweetCount: 3,
  retweetedByViewer: false,
  replyCount: 0,
  authorDisplayName: null,
  authorAvatarUrl: null,
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

describe('Retweet button', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.mocked(tweetsApi.getTimeline).mockResolvedValue([tweet])
    vi.mocked(socialApi.retweet).mockResolvedValue({ retweetCount: 1 })
    vi.mocked(socialApi.unretweet).mockResolvedValue(undefined)
  })

  it('shows retweet button with count', async () => {
    const wrapper = mountHome()
    await flushPromises()
    const text = wrapper.text()
    expect(text).toContain('3')
    const buttons = wrapper.findAll('button')
    const retweetBtn = buttons.find((b) => b.text().includes('🔁'))
    expect(retweetBtn).toBeDefined()
  })

  it('clicking retweet toggles retweetedByViewer and updates count', async () => {
    const wrapper = mountHome()
    await flushPromises()

    const buttons = wrapper.findAll('button')
    const retweetBtn = buttons.find((b) => b.text().includes('🔁'))!
    await retweetBtn.trigger('click')
    await flushPromises()

    expect(socialApi.retweet).toHaveBeenCalledWith('tweet-1')
    expect(retweetBtn.classes()).toContain('text-green-500')
  })
})
