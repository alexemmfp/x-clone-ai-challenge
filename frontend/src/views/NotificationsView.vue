<template>
  <div class="min-h-screen bg-gray-50">
    <div class="max-w-xl md:max-w-2xl mx-auto py-6 px-4 space-y-3">
      <h1 class="text-xl font-bold text-gray-900">Notifications</h1>

      <div v-if="notifs.notifications.length === 0" class="text-center text-gray-400 py-8">
        No notifications yet.
      </div>

      <article
        v-for="n in notifs.notifications"
        :key="n.id"
        class="bg-white rounded-2xl shadow p-4 flex items-start gap-3"
        :class="{ 'opacity-60': n.read }"
      >
        <span class="text-xl flex-shrink-0">
          {{ n.type === 'follow' ? '👤' : n.type === 'mention' ? '💬' : '🔁' }}
        </span>
        <div class="min-w-0 flex-1">
          <RouterLink :to="`/profile/${n.actorUsername}`" class="font-semibold text-sky-600 hover:underline text-sm">
            @{{ n.actorUsername }}
          </RouterLink>
          <span class="text-sm text-gray-700">
            {{ n.type === 'follow' ? ' followed you' : n.type === 'mention' ? ' mentioned you' : ' retweeted your tweet' }}
          </span>
          <p v-if="n.tweetText" class="text-xs text-gray-400 truncate mt-0.5">{{ n.tweetText }}</p>
          <RouterLink v-if="n.tweetId" :to="`/tweet/${n.tweetId}`" class="text-xs text-sky-500 hover:underline mt-1 block">
            View tweet →
          </RouterLink>
        </div>
        <span class="text-xs text-gray-400 flex-shrink-0">{{ formatDate(n.createdAt) }}</span>
      </article>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { useNotificationsStore } from '@/stores/useNotificationsStore'

const notifs = useNotificationsStore()

onMounted(() => notifs.markAllRead())

function formatDate(iso: string) {
  return new Date(iso).toLocaleString(undefined, { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })
}
</script>
