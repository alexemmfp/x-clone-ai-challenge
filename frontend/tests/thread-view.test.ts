import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import ThreadView from '@/views/ThreadView.vue'

vi.mock('@/api/tweets', () => ({
  tweetsApi: {
    getById: vi.fn(),
    getReplies: vi.fn(),
    create: vi.fn(),
    delete: vi.fn(),
    getTimeline: vi.fn(),
    uploadImage: vi.fn(),
  },
}))

vi.mock('@/api/social', () => ({
  socialApi: {
    likeTweet: vi.fn(),
    unlikeTweet: vi.fn(),
    retweet: vi.fn(),
    unretweet: vi.fn(),
  },
}))

vi.mock('@/stores/useMentionsStore', () => ({
  useMentionsStore: () => ({
    validateBatch: vi.fn(),
  }),
}))

import { tweetsApi } from '@/api/tweets'

const parentTweet = {
  id: 'parent-1',
  authorId: 'user-1',
  authorUsername: 'alice',
  text: 'Parent tweet',
  parentId: null,
  imageUrl: null,
  createdAt: new Date().toISOString(),
  likeCount: 0,
  likedByViewer: false,
  retweetCount: 0,
  retweetedByViewer: false,
  replyCount: 0,
  authorDisplayName: null,
  authorAvatarUrl: null,
}

const replyTweet = {
  id: 'reply-1',
  authorId: 'user-2',
  authorUsername: 'bob',
  text: 'A reply',
  parentId: 'parent-1',
  imageUrl: null,
  createdAt: new Date().toISOString(),
  likeCount: 0,
  likedByViewer: false,
  retweetCount: 0,
  retweetedByViewer: false,
  replyCount: 0,
  authorDisplayName: null,
  authorAvatarUrl: null,
}

function mountThread(tweetId = 'parent-1') {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/tweet/:id', name: 'thread', component: ThreadView },
      { path: '/profile/:username', component: { template: '<div/>' } },
    ],
  })
  router.push(`/tweet/${tweetId}`)
  return mount(ThreadView, {
    global: { plugins: [createPinia(), router] },
  })
}

describe('ThreadView', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.mocked(tweetsApi.getById).mockResolvedValue(parentTweet)
    vi.mocked(tweetsApi.getReplies).mockResolvedValue([replyTweet])
  })

  it('renders parent tweet text', async () => {
    const wrapper = mountThread()
    await flushPromises()
    expect(wrapper.text()).toContain('Parent tweet')
  })

  it('renders replies', async () => {
    const wrapper = mountThread()
    await flushPromises()
    expect(wrapper.text()).toContain('A reply')
    expect(wrapper.text()).toContain('bob')
  })

  it('has a reply textarea', async () => {
    const wrapper = mountThread()
    await flushPromises()
    expect(wrapper.find('textarea').exists()).toBe(true)
  })

  it('submits a reply with parentId', async () => {
    const newReply = { ...replyTweet, id: 'reply-2', text: 'My reply' }
    vi.mocked(tweetsApi.create).mockResolvedValue(newReply)

    const wrapper = mountThread()
    await flushPromises()

    await wrapper.find('textarea').setValue('My reply')
    await wrapper.find('button[data-testid="reply-submit"]').trigger('click')
    await flushPromises()

    expect(tweetsApi.create).toHaveBeenCalledWith({ text: 'My reply', parentId: 'parent-1' })
  })

  it('appends new reply after submission', async () => {
    const newReply = { ...replyTweet, id: 'reply-3', text: 'New reply text' }
    vi.mocked(tweetsApi.create).mockResolvedValue(newReply)

    const wrapper = mountThread()
    await flushPromises()

    await wrapper.find('textarea').setValue('New reply text')
    await wrapper.find('button[data-testid="reply-submit"]').trigger('click')
    await flushPromises()

    expect(wrapper.text()).toContain('New reply text')
  })

  it('shows image upload button in reply composer', async () => {
    const wrapper = mountThread()
    await flushPromises()
    expect(wrapper.find('[data-testid="reply-image-btn"]').exists()).toBe(true)
  })

  it('uploads image and passes imageUrl on reply submit', async () => {
    const newReply = { ...replyTweet, id: 'reply-4', text: 'reply with image', imageUrl: '/uploads/test.png' }
    vi.mocked(tweetsApi.uploadImage).mockResolvedValue('/uploads/test.png')
    vi.mocked(tweetsApi.create).mockResolvedValue(newReply)

    const wrapper = mountThread()
    await flushPromises()

    const file = new File(['img'], 'photo.png', { type: 'image/png' })
    const input = wrapper.find<HTMLInputElement>('[data-testid="reply-image-input"]')
    Object.defineProperty(input.element, 'files', { value: [file] })
    await input.trigger('change')

    await wrapper.find('textarea').setValue('reply with image')
    await wrapper.find('button[data-testid="reply-submit"]').trigger('click')
    await flushPromises()

    expect(tweetsApi.uploadImage).toHaveBeenCalledWith(file)
    expect(tweetsApi.create).toHaveBeenCalledWith(expect.objectContaining({ imageUrl: '/uploads/test.png' }))
  })
})
