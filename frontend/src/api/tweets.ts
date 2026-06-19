import { apiClient } from './client'
import type { Tweet, CreateTweetRequest } from '@/types/tweet'

export const tweetsApi = {
  create: (data: CreateTweetRequest) =>
    apiClient.post<Tweet>('/api/tweets', data).then((r) => r.data),

  delete: (id: string) => apiClient.delete(`/api/tweets/${id}`),

  getTimeline: (page = 1, pageSize = 20) =>
    apiClient
      .get<Tweet[]>('/api/timeline', { params: { page, pageSize } })
      .then((r) => r.data),

  getById: (id: string) =>
    apiClient.get<Tweet>(`/api/tweets/${id}`).then((r) => r.data),

  getReplies: (id: string) =>
    apiClient.get<Tweet[]>(`/api/tweets/${id}/replies`).then((r) => r.data),
}
