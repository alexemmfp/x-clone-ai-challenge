import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { authApi } from '@/api/auth'
import type { UserProfile, LoginRequest, RegisterRequest } from '@/types/auth'

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(null)
  const user = ref<UserProfile | null>(null)
  const router = useRouter()

  const isAuthenticated = computed(() => accessToken.value !== null)

  function setAuth(token: string, profile: UserProfile) {
    accessToken.value = token
    user.value = profile
  }

  function clearAuth() {
    accessToken.value = null
    user.value = null
  }

  async function login(credentials: LoginRequest) {
    const result = await authApi.login(credentials)
    setAuth(result.accessToken, { id: result.userId, username: result.username })
    await router.push('/')
  }

  async function register(data: RegisterRequest) {
    const result = await authApi.register(data)
    setAuth(result.accessToken, { id: result.userId, username: result.username })
    await router.push('/')
  }

  async function logout() {
    try {
      await authApi.logout()
    } finally {
      clearAuth()
      await router.push('/login')
    }
  }

  async function tryRefresh() {
    try {
      const result = await authApi.refresh()
      setAuth(result.accessToken, { id: result.userId, username: result.username })
    } catch {
      clearAuth()
    }
  }

  return { accessToken, user, isAuthenticated, setAuth, clearAuth, login, register, logout, tryRefresh }
})
