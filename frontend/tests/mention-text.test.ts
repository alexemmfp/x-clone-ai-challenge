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
