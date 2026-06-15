import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { UserProfile } from '@/types/auth'

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(null)
  const user = ref<UserProfile | null>(null)

  const isAuthenticated = computed(() => accessToken.value !== null)

  function setAuth(token: string, profile: UserProfile) {
    accessToken.value = token
    user.value = profile
  }

  function clearAuth() {
    accessToken.value = null
    user.value = null
  }

  return { accessToken, user, isAuthenticated, setAuth, clearAuth }
})
