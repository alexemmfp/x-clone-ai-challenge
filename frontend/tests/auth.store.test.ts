import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '@/stores/useAuthStore'

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: vi.fn() }),
}))

vi.mock('@/api/auth', () => ({
  authApi: {
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    refresh: vi.fn(),
  },
}))

import { authApi } from '@/api/auth'

describe('useAuthStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('is unauthenticated initially', () => {
    const auth = useAuthStore()
    expect(auth.isAuthenticated).toBe(false)
    expect(auth.accessToken).toBeNull()
  })

  it('setAuth stores token and user', () => {
    const auth = useAuthStore()
    auth.setAuth('tok', { id: '1', username: 'alice' })
    expect(auth.isAuthenticated).toBe(true)
    expect(auth.accessToken).toBe('tok')
    expect(auth.user?.username).toBe('alice')
  })

  it('clearAuth removes token and user', () => {
    const auth = useAuthStore()
    auth.setAuth('tok', { id: '1', username: 'alice' })
    auth.clearAuth()
    expect(auth.isAuthenticated).toBe(false)
    expect(auth.accessToken).toBeNull()
  })

  it('login sets token from api response', async () => {
    vi.mocked(authApi.login).mockResolvedValue({
      accessToken: 'new_token',
      userId: 'u1',
      username: 'bob',
    })
    const auth = useAuthStore()
    await auth.login({ email: 'bob@example.com', password: 'pass' })
    expect(auth.accessToken).toBe('new_token')
    expect(auth.user?.username).toBe('bob')
  })

  it('tryRefresh updates accessToken on success', async () => {
    vi.mocked(authApi.refresh).mockResolvedValue({ accessToken: 'refreshed' })
    const auth = useAuthStore()
    await auth.tryRefresh()
    expect(auth.accessToken).toBe('refreshed')
  })

  it('tryRefresh clears auth on failure', async () => {
    vi.mocked(authApi.refresh).mockRejectedValue(new Error('401'))
    const auth = useAuthStore()
    auth.setAuth('old', { id: '1', username: 'alice' })
    await auth.tryRefresh()
    expect(auth.isAuthenticated).toBe(false)
  })
})
