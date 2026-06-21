import axios from 'axios'
import { useAuthStore } from '@/stores/useAuthStore'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080',
  withCredentials: true,
})

apiClient.interceptors.request.use((config) => {
  const auth = useAuthStore()
  if (auth.accessToken) {
    config.headers.Authorization = `Bearer ${auth.accessToken}`
  }
  return config
})

let refreshing: Promise<void> | null = null

apiClient.interceptors.response.use(
  (r) => r,
  async (error) => {
    const original = error.config
    if (
      error.response?.status !== 401 ||
      original._retry ||
      original.url?.includes('/auth/refresh')
    ) {
      return Promise.reject(error)
    }
    original._retry = true

    if (!refreshing) {
      const auth = useAuthStore()
      refreshing = auth.tryRefresh().finally(() => {
        refreshing = null
      })
    }

    await refreshing
    const auth = useAuthStore()
    if (auth.accessToken) {
      original.headers.Authorization = `Bearer ${auth.accessToken}`
      return apiClient(original)
    }
    return Promise.reject(error)
  },
)
