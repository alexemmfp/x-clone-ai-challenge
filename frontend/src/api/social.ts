import { apiClient } from './client'
import type { Profile } from '@/types/profile'
import type { UserSummary } from '@/types/userSummary'

export const socialApi = {
  getProfile: (username: string) =>
    apiClient.get<Profile>(`/api/users/${username}`).then((r) => r.data),

  updateProfile: (data: { bio?: string; avatarUrl?: string }) =>
    apiClient.patch<Profile>('/api/me', data).then((r) => r.data),

  getFollowers: (username: string) =>
    apiClient.get<UserSummary[]>(`/api/users/${username}/followers`).then((r) => r.data),

  getFollowing: (username: string) =>
    apiClient.get<UserSummary[]>(`/api/users/${username}/following`).then((r) => r.data),

  follow: (username: string) => apiClient.post(`/api/users/${username}/follow`),
  unfollow: (username: string) => apiClient.delete(`/api/users/${username}/follow`),

  likeTweet: (tweetId: string) => apiClient.post(`/api/tweets/${tweetId}/like`),
  unlikeTweet: (tweetId: string) => apiClient.delete(`/api/tweets/${tweetId}/like`),

  retweet: (tweetId: string) =>
    apiClient.post<{ retweetCount: number }>(`/api/tweets/${tweetId}/retweet`).then((r) => r.data),
  unretweet: (tweetId: string) => apiClient.delete(`/api/tweets/${tweetId}/retweet`),
}
