import { apiClient } from './client'
import type { Profile } from '@/types/profile'

export const socialApi = {
  getProfile: (username: string) =>
    apiClient.get<Profile>(`/api/users/${username}`).then((r) => r.data),

  updateProfile: (data: { bio?: string; avatarUrl?: string }) =>
    apiClient.patch<Profile>('/api/me', data).then((r) => r.data),

  follow: (username: string) => apiClient.post(`/api/users/${username}/follow`),
  unfollow: (username: string) => apiClient.delete(`/api/users/${username}/follow`),

  likeTweet: (tweetId: string) => apiClient.post(`/api/tweets/${tweetId}/like`),
  unlikeTweet: (tweetId: string) => apiClient.delete(`/api/tweets/${tweetId}/like`),
}
