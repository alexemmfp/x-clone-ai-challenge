<template>
  <div class="min-h-screen bg-gray-50">
    <div class="max-w-xl mx-auto py-6 px-4 space-y-4">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <h1 class="text-xl font-bold text-gray-900">Home</h1>
        <button
          class="text-sm text-gray-500 hover:text-red-500 transition"
          @click="auth.logout()"
        >
          Sign out
        </button>
      </div>

      <!-- Composer -->
      <div class="bg-white rounded-2xl shadow p-4 space-y-3">
        <textarea
          v-model="draftText"
          rows="3"
          maxlength="280"
          placeholder="What's happening?"
          class="w-full resize-none border-none outline-none text-gray-900 placeholder-gray-400 text-sm"
        />
        <div class="flex items-center justify-between">
          <span class="text-xs text-gray-400">{{ draftText.length }}/280</span>
          <button
            :disabled="!draftText.trim() || posting"
            class="bg-sky-500 hover:bg-sky-600 disabled:opacity-40 text-white text-sm font-semibold rounded-full px-5 py-2 transition"
            @click="post"
          >
            {{ posting ? 'Posting…' : 'Post' }}
          </button>
        </div>
      </div>

      <!-- Timeline -->
      <div v-if="tweets.loading && tweets.timeline.length === 0" class="text-center text-gray-400 py-8">
        Loading…
      </div>

      <div v-else-if="tweets.timeline.length === 0" class="text-center text-gray-400 py-8">
        Nothing here yet. Follow someone or post your first tweet!
      </div>

      <article
        v-for="tweet in tweets.timeline"
        :key="tweet.id"
        class="bg-white rounded-2xl shadow p-4 space-y-1"
      >
        <div class="flex items-center justify-between">
          <span class="font-semibold text-sm text-gray-900">@{{ tweet.authorUsername }}</span>
          <span class="text-xs text-gray-400">{{ formatDate(tweet.createdAt) }}</span>
        </div>
        <p class="text-gray-800 text-sm whitespace-pre-wrap">{{ tweet.text }}</p>
        <button
          v-if="tweet.authorId === auth.user?.id"
          class="text-xs text-red-400 hover:text-red-600 transition"
          @click="tweets.deleteTweet(tweet.id)"
        >
          Delete
        </button>
      </article>

      <div v-if="tweets.hasMore && tweets.timeline.length > 0" class="text-center">
        <button
          class="text-sm text-sky-500 hover:underline"
          :disabled="tweets.loading"
          @click="tweets.loadTimeline()"
        >
          {{ tweets.loading ? 'Loading…' : 'Load more' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/useAuthStore'
import { useTweetStore } from '@/stores/useTweetStore'

const auth = useAuthStore()
const tweets = useTweetStore()

const draftText = ref('')
const posting = ref(false)

onMounted(() => tweets.loadTimeline(true))

async function post() {
  if (!draftText.value.trim()) return
  posting.value = true
  try {
    await tweets.createTweet(draftText.value.trim())
    draftText.value = ''
  } finally {
    posting.value = false
  }
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleString(undefined, {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}
</script>
