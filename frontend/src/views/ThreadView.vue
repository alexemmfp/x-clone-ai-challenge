<template>
  <div class="min-h-screen bg-gray-50">
    <div class="max-w-xl md:max-w-2xl mx-auto py-6 px-4 md:px-6 space-y-4">
      <RouterLink to="/" class="text-sm text-sky-500 hover:underline">&larr; Back</RouterLink>

      <div v-if="loading" class="text-center text-gray-400 py-8">Loading…</div>

      <template v-else-if="parent">
        <!-- Parent tweet -->
        <article class="bg-white rounded-2xl shadow p-4 space-y-2">
          <div class="flex items-center justify-between">
            <RouterLink
              :to="`/profile/${parent.authorUsername}`"
              class="font-semibold text-sm text-gray-900 hover:underline"
            >@{{ parent.authorUsername }}</RouterLink>
            <span class="text-xs text-gray-400">{{ formatDate(parent.createdAt) }}</span>
          </div>
          <p class="text-gray-800 text-sm md:text-base whitespace-pre-wrap">{{ parent.text }}</p>
          <div class="flex items-center gap-4">
            <button
              class="flex items-center gap-1 transition min-h-[44px] text-xs"
              :class="parent.likedByViewer ? 'text-rose-500' : 'text-gray-400 hover:text-rose-400'"
              @click="toggleLike(parent)"
            >
              ♥ {{ parent.likeCount }}
            </button>
          </div>
        </article>

        <!-- Reply composer -->
        <div class="bg-white rounded-2xl shadow p-4 space-y-3">
          <textarea
            v-model="replyText"
            rows="2"
            maxlength="280"
            placeholder="Write a reply…"
            class="w-full resize-none border-none outline-none text-gray-900 placeholder-gray-400 text-sm"
          />
          <div class="flex items-center justify-between">
            <span class="text-xs text-gray-400">{{ replyText.length }}/280</span>
            <button
              :disabled="!replyText.trim() || submitting"
              data-testid="reply-submit"
              class="bg-sky-500 hover:bg-sky-600 disabled:opacity-40 text-white text-sm font-semibold rounded-full px-5 py-2 transition"
              @click="submitReply"
            >
              {{ submitting ? 'Posting…' : 'Reply' }}
            </button>
          </div>
        </div>

        <!-- Replies -->
        <div v-if="replies.length === 0" class="text-center text-gray-400 py-4 text-sm">
          No replies yet.
        </div>

        <article
          v-for="reply in replies"
          :key="reply.id"
          class="bg-white rounded-2xl shadow p-3 sm:p-4 space-y-2 ml-4 border-l-2 border-sky-200"
        >
          <div class="flex items-center justify-between">
            <RouterLink
              :to="`/profile/${reply.authorUsername}`"
              class="font-semibold text-sm text-gray-900 hover:underline"
            >@{{ reply.authorUsername }}</RouterLink>
            <span class="text-xs text-gray-400">{{ formatDate(reply.createdAt) }}</span>
          </div>
          <p class="text-gray-800 text-sm whitespace-pre-wrap">{{ reply.text }}</p>
          <div class="flex items-center gap-4">
            <button
              class="flex items-center gap-1 transition min-h-[44px] text-xs"
              :class="reply.likedByViewer ? 'text-rose-500' : 'text-gray-400 hover:text-rose-400'"
              @click="toggleLike(reply)"
            >
              ♥ {{ reply.likeCount }}
            </button>
          </div>
        </article>
      </template>

      <div v-else class="text-center text-gray-400 py-8">Tweet not found.</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { tweetsApi } from '@/api/tweets'
import { socialApi } from '@/api/social'
import type { Tweet } from '@/types/tweet'

const route = useRoute()
const loading = ref(true)
const parent = ref<Tweet | null>(null)
const replies = ref<Tweet[]>([])
const replyText = ref('')
const submitting = ref(false)

onMounted(async () => {
  const id = route.params.id as string
  try {
    const [tweet, reps] = await Promise.all([tweetsApi.getById(id), tweetsApi.getReplies(id)])
    parent.value = tweet
    replies.value = reps
  } finally {
    loading.value = false
  }
})

async function submitReply() {
  if (!replyText.value.trim() || !parent.value) return
  submitting.value = true
  try {
    const reply = await tweetsApi.create({ text: replyText.value.trim(), parentId: parent.value.id })
    replies.value = [...replies.value, reply]
    replyText.value = ''
  } finally {
    submitting.value = false
  }
}

async function toggleLike(tweet: Tweet) {
  if (tweet.likedByViewer) {
    await socialApi.unlikeTweet(tweet.id)
    tweet.likeCount--
    tweet.likedByViewer = false
  } else {
    await socialApi.likeTweet(tweet.id)
    tweet.likeCount++
    tweet.likedByViewer = true
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
