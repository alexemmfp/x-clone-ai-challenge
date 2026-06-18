import { apiClient } from './client'
import type { AuthResult, LoginRequest, RegisterRequest } from '@/types/auth'

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post<AuthResult>('/api/auth/login', data).then((r) => r.data),

  register: (data: RegisterRequest) =>
    apiClient.post<AuthResult>('/api/auth/register', data).then((r) => r.data),

  refresh: () =>
    apiClient.post<AuthResult>('/api/auth/refresh').then((r) => r.data),

  logout: () => apiClient.post('/api/auth/logout'),
}
