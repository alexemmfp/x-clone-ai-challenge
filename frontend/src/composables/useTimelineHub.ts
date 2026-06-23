import { onUnmounted, watch } from 'vue'
import { HubConnectionBuilder, LogLevel, HubConnectionState } from '@microsoft/signalr'
import type { Tweet } from '@/types/tweet'
import { useAuthStore } from '@/stores/useAuthStore'
import { useNotificationsStore } from '@/stores/useNotificationsStore'

export function useTimelineHub(onTweetCreated: (tweet: Tweet) => void) {
  const baseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080'
  const auth = useAuthStore()

  const connection = new HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/timeline`, {
      accessTokenFactory: () => auth.accessToken ?? '',
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()

  connection.on('TweetCreated', onTweetCreated)

  const notifs = useNotificationsStore()

  connection.on('FollowNotification', (data: { followerUsername: string; displayName?: string; avatarUrl?: string }) => {
    notifs.add({ id: crypto.randomUUID(), type: 'follow',
      actorUsername: data.followerUsername, actorDisplayName: data.displayName ?? null,
      actorAvatarUrl: data.avatarUrl ?? null, createdAt: new Date().toISOString(), read: false })
  })

  connection.on('MentionNotification', (data: { tweetId: string; authorUsername: string; tweetText: string }) => {
    notifs.add({ id: crypto.randomUUID(), type: 'mention',
      actorUsername: data.authorUsername, actorDisplayName: null, actorAvatarUrl: null,
      tweetId: data.tweetId, tweetText: data.tweetText,
      createdAt: new Date().toISOString(), read: false })
  })

  connection.on('RetweetNotification', (data: { tweetId: string; retweeterUsername: string }) => {
    notifs.add({ id: crypto.randomUUID(), type: 'retweet',
      actorUsername: data.retweeterUsername, actorDisplayName: null, actorAvatarUrl: null,
      tweetId: data.tweetId, createdAt: new Date().toISOString(), read: false })
  })

  connection.on('LikeNotification', (data: { tweetId: string; likerUsername: string }) => {
    notifs.add({ id: crypto.randomUUID(), type: 'like',
      actorUsername: data.likerUsername, actorDisplayName: null, actorAvatarUrl: null,
      tweetId: data.tweetId, createdAt: new Date().toISOString(), read: false })
  })

  connection.on('ReplyNotification', (data: { tweetId: string; replierUsername: string; replyText: string }) => {
    notifs.add({ id: crypto.randomUUID(), type: 'reply',
      actorUsername: data.replierUsername, actorDisplayName: null, actorAvatarUrl: null,
      tweetId: data.tweetId, tweetText: data.replyText, createdAt: new Date().toISOString(), read: false })
  })

  watch(() => auth.isAuthenticated, async (isAuth) => {
    if (isAuth && connection.state === HubConnectionState.Disconnected) {
      try { await connection.start() } catch { /* hub optional */ }
    } else if (!isAuth && connection.state !== HubConnectionState.Disconnected) {
      try { await connection.stop() } catch { /* ignore stop errors */ }
    }
  }, { immediate: true })

  onUnmounted(async () => {
    try { await connection.stop() } catch { /* ignore stop errors */ }
  })
}
