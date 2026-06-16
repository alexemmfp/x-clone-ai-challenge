import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import LoginView from '@/views/LoginView.vue'
import { useAuthStore } from '@/stores/useAuthStore'

vi.mock('@/api/auth', () => ({
  authApi: {
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    refresh: vi.fn(),
  },
}))

import { authApi } from '@/api/auth'

function mountLogin() {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [{ path: '/', component: { template: '<div/>' } }],
  })
  return mount(LoginView, {
    global: { plugins: [createPinia(), router] },
  })
}

describe('LoginView', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('renders email and password inputs', () => {
    const wrapper = mountLogin()
    expect(wrapper.find('input[type="email"]').exists()).toBe(true)
    expect(wrapper.find('input[type="password"]').exists()).toBe(true)
  })

  it('calls auth.login on submit and navigates on success', async () => {
    vi.mocked(authApi.login).mockResolvedValue({
      accessToken: 'tok',
      userId: 'u1',
      username: 'alice',
    })
    const wrapper = mountLogin()
    await wrapper.find('input[type="email"]').setValue('alice@example.com')
    await wrapper.find('input[type="password"]').setValue('password123')
    await wrapper.find('form').trigger('submit')
    await flushPromises()
    expect(authApi.login).toHaveBeenCalledWith({ email: 'alice@example.com', password: 'password123' })
  })

  it('shows error message on failed login', async () => {
    vi.mocked(authApi.login).mockRejectedValue({
      response: { data: { error: 'Invalid credentials.' } },
    })
    const wrapper = mountLogin()
    await wrapper.find('input[type="email"]').setValue('x@x.com')
    await wrapper.find('input[type="password"]').setValue('wrong')
    await wrapper.find('form').trigger('submit')
    await flushPromises()
    expect(wrapper.text()).toContain('Invalid credentials.')
  })
})
