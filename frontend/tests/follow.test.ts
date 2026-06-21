import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import ProfileView from '@/views/ProfileView.vue'

vi.mock('@/api/social', () => ({
  socialApi: {
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
    user: { id: 'viewer-id', username: 'viewer' },
    logout: vi.fn(),
  }),
}))

import { socialApi } from '@/api/social'

const bobProfile = {
  id: 'bob-id',
  username: 'bob',
  email: 'bob@example.com',
  bio: 'Backend dev',
  avatarUrl: null,
  followerCount: 5,
  followingCount: 3,
  isFollowedByViewer: false,
}

function mountProfile(username = 'bob') {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [{ path: '/profile/:username', component: ProfileView }],
  })
  router.push(`/profile/${username}`)
  return mount(ProfileView, { global: { plugins: [createPinia(), router] } })
}

describe('Follow flow', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.mocked(socialApi.getProfile).mockResolvedValue({ ...bobProfile })
    vi.mocked(socialApi.follow).mockResolvedValue(undefined)
    vi.mocked(socialApi.unfollow).mockResolvedValue(undefined)
    vi.mocked(socialApi.getFollowers).mockResolvedValue([])
    vi.mocked(socialApi.getFollowing).mockResolvedValue([])
  })

  it('shows Follow button for another user', async () => {
    const wrapper = mountProfile()
    await flushPromises()
    expect(wrapper.find('[data-testid="follow-btn"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="follow-btn"]').text()).toBe('Follow')
  })

  it('calls socialApi.follow on click, switches to Unfollow, increments count', async () => {
    const wrapper = mountProfile()
    await flushPromises()

    const btn = wrapper.find('[data-testid="follow-btn"]')
    expect(btn.text()).toBe('Follow')
    expect(wrapper.text()).toContain('5')

    await btn.trigger('click')
    await flushPromises()

    expect(socialApi.follow).toHaveBeenCalledWith('bob')
    expect(btn.text()).toBe('Unfollow')
    expect(wrapper.text()).toContain('6')
  })

  it('calls socialApi.unfollow and switches back to Follow', async () => {
    vi.mocked(socialApi.getProfile).mockResolvedValue({
      ...bobProfile,
      isFollowedByViewer: true,
      followerCount: 6,
    })

    const wrapper = mountProfile()
    await flushPromises()

    const btn = wrapper.find('[data-testid="follow-btn"]')
    expect(btn.text()).toBe('Unfollow')

    await btn.trigger('click')
    await flushPromises()

    expect(socialApi.unfollow).toHaveBeenCalledWith('bob')
    expect(btn.text()).toBe('Follow')
    expect(wrapper.text()).toContain('5')
  })

  it('shows Edit profile button on own profile, no Follow button', async () => {
    vi.mocked(socialApi.getProfile).mockResolvedValue({
      ...bobProfile,
      id: 'viewer-id',
      username: 'viewer',
    })

    const wrapper = mountProfile('viewer')
    await flushPromises()

    expect(wrapper.text()).toContain('Edit profile')
    expect(wrapper.find('[data-testid="follow-btn"]').exists()).toBe(false)
  })
})
