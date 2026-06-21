import { apiClient } from './client'

export interface UserSearchResult {
  id: string
  username: string
  displayName: string | null
  avatarUrl: string | null
}

export const usersApi = {
  validateUsernames: (usernames: string[]) =>
    apiClient
      .get<Record<string, boolean>>('/api/users/validate-usernames', {
        params: { usernames: usernames.join(',') },
      })
      .then((r) => r.data),

  searchUsers: (q: string) =>
    apiClient
      .get<UserSearchResult[]>('/api/search/users', { params: { q } })
      .then((r) => r.data),
}
