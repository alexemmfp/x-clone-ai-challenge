import { apiClient } from './client'

export const usersApi = {
  validateUsernames: (usernames: string[]) =>
    apiClient
      .get<Record<string, boolean>>('/api/users/validate-usernames', {
        params: { usernames: usernames.join(',') },
      })
      .then((r) => r.data),
}
