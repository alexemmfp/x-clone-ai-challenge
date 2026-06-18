import { onMounted, onUnmounted } from 'vue'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import type { Tweet } from '@/types/tweet'
import { useAuthStore } from '@/stores/useAuthStore'

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

  onMounted(async () => {
    try {
      await connection.start()
    } catch {
      // Hub is optional; timeline still works via polling
    }
  })

  onUnmounted(async () => {
    await connection.stop()
  })
}
