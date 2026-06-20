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
    retweet: vi.fn(),
    unretweet: vi.fn(),
  },
}))

vi.mock('@/composables/useTimelineHub', () => ({
  useTimelineHub: vi.fn(),
}))

import { tweetsApi } from '@/api/tweets'

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

describe('Image upload in composer', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.mocked(tweetsApi.getTimeline).mockResolvedValue([])
  })

  it('has a file input for image attachment', async () => {
    const wrapper = mountHome()
    await flushPromises()
    expect(wrapper.find('input[type="file"]').exists()).toBe(true)
  })

  it('uploads image then creates tweet with imageUrl', async () => {
    vi.mocked(tweetsApi.uploadImage).mockResolvedValue('/uploads/img.jpg')
    vi.mocked(tweetsApi.create).mockResolvedValue({
      id: 't1',
      authorId: 'u1',
      authorUsername: 'alice',
      text: 'With image',
      parentId: null,
      imageUrl: '/uploads/img.jpg',
      createdAt: new Date().toISOString(),
      likeCount: 0,
      likedByViewer: false,
      retweetCount: 0,
      retweetedByViewer: false,
    })

    const wrapper = mountHome()
    await flushPromises()

    await wrapper.find('textarea').setValue('With image')

    const file = new File(['fake-image'], 'photo.jpg', { type: 'image/jpeg' })
    const input = wrapper.find('input[type="file"]')
    Object.defineProperty(input.element, 'files', { value: [file] })
    await input.trigger('change')
    await flushPromises()

    await wrapper.find('button[data-testid="post-submit"]').trigger('click')
    await flushPromises()

    expect(tweetsApi.uploadImage).toHaveBeenCalledWith(file)
    expect(tweetsApi.create).toHaveBeenCalledWith(
      expect.objectContaining({ text: 'With image', imageUrl: '/uploads/img.jpg' }),
    )
  })

  it('creates tweet without imageUrl when no file selected', async () => {
    vi.mocked(tweetsApi.create).mockResolvedValue({
      id: 't2',
      authorId: 'u1',
      authorUsername: 'alice',
      text: 'No image',
      parentId: null,
      imageUrl: null,
      createdAt: new Date().toISOString(),
      likeCount: 0,
      likedByViewer: false,
      retweetCount: 0,
      retweetedByViewer: false,
    })

    const wrapper = mountHome()
    await flushPromises()
    await wrapper.find('textarea').setValue('No image')
    await wrapper.find('button[data-testid="post-submit"]').trigger('click')
    await flushPromises()

    expect(tweetsApi.uploadImage).not.toHaveBeenCalled()
    expect(tweetsApi.create).toHaveBeenCalledWith({ text: 'No image' })
  })
})
