<template>
  <div class="min-h-screen bg-gray-50">
    <div class="max-w-xl md:max-w-2xl mx-auto py-6 px-4 md:px-6 space-y-4">
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
          class="w-full resize-none border-none outline-none text-gray-900 placeholder-gray-400 text-sm md:text-base"
        />
        <div v-if="imagePreview" class="relative inline-block">
          <img :src="imagePreview" class="max-h-40 rounded-lg object-cover" alt="preview" />
          <button
            class="absolute top-1 right-1 bg-black/50 text-white rounded-full w-5 h-5 text-xs flex items-center justify-center"
            @click="clearImage"
          >✕</button>
        </div>
        <div class="flex items-center justify-between">
          <label class="cursor-pointer text-sky-500 hover:text-sky-600 transition">
            <span class="text-sm">📎</span>
            <input type="file" accept="image/*" class="hidden" @change="onFileChange" />
          </label>
          <div class="flex items-center gap-3">
            <span class="text-xs text-gray-400">{{ draftText.length }}/280</span>
            <button
              :disabled="!draftText.trim() || posting"
              data-testid="post-submit"
              class="bg-sky-500 hover:bg-sky-600 disabled:opacity-40 text-white text-sm font-semibold rounded-full px-5 py-2 transition"
              @click="post"
            >
              {{ posting ? 'Posting…' : 'Post' }}
            </button>
          </div>
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
        class="bg-white rounded-2xl shadow p-3 sm:p-4 space-y-2 cursor-pointer hover:bg-gray-50 transition"
        @click.self="$router.push(`/tweet/${tweet.id}`)"
      >
        <div class="flex items-center justify-between">
          <RouterLink
            :to="`/profile/${tweet.authorUsername}`"
            class="font-semibold text-sm text-gray-900 hover:underline"
          >@{{ tweet.authorUsername }}</RouterLink>
          <span class="text-xs text-gray-400">{{ formatDate(tweet.createdAt) }}</span>
        </div>
        <RouterLink :to="`/tweet/${tweet.id}`">
          <MentionText :text="tweet.text" class="text-gray-800 text-sm md:text-base whitespace-pre-wrap hover:text-sky-700 transition" />
        </RouterLink>
        <a v-if="tweet.imageUrl" :href="tweet.imageUrl" target="_blank" rel="noopener">
          <img :src="tweet.imageUrl" class="rounded-lg max-h-64 object-cover w-full hover:opacity-90 transition" alt="tweet image" />
        </a>
        <div class="flex items-center gap-4">
          <button
            class="flex items-center gap-1 transition min-h-[44px] text-xs"
            :class="tweet.likedByViewer ? 'text-rose-500' : 'text-gray-400 hover:text-rose-400'"
            @click="tweets.toggleLike(tweet)"
          >
            ♥ {{ tweet.likeCount }}
          </button>
          <button
            v-if="tweet.authorId === auth.user?.id"
            class="text-xs text-red-400 hover:text-red-600 transition ml-auto min-h-[44px]"
            @click="tweets.deleteTweet(tweet.id)"
          >
            Delete
          </button>
        </div>
      </article>

      <div v-if="tweets.hasMore && tweets.timeline.length > 0" class="text-center">
        <button
          class="text-sm text-sky-500 hover:underline"
          :disabled="tweets.loading"
          @click="loadMore()"
        >
          {{ tweets.loading ? 'Loading…' : 'Load more' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/useAuthStore'
import { useTweetStore } from '@/stores/useTweetStore'
import { useTimelineHub } from '@/composables/useTimelineHub'
import { tweetsApi } from '@/api/tweets'
import MentionText from '@/components/MentionText.vue'
import { useMentionsStore } from '@/stores/useMentionsStore'

const auth = useAuthStore()
const tweets = useTweetStore()
const mentionsStore = useMentionsStore()

const draftText = ref('')
const posting = ref(false)
const selectedFile = ref<File | null>(null)
const imagePreview = ref<string | null>(null)

useTimelineHub((tweet) => tweets.prependTweet(tweet))
onMounted(async () => {
  await tweets.loadTimeline(true)
  const allMentions = tweets.timeline
    .flatMap((t) => [...t.text.matchAll(/@(\w+)/g)].map((m) => m[1]))
  const unique = [...new Set(allMentions)]
  if (unique.length) mentionsStore.validateBatch(unique)
})

function onFileChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0] ?? null
  selectedFile.value = file
  imagePreview.value = file ? URL.createObjectURL(file) : null
}

function clearImage() {
  selectedFile.value = null
  imagePreview.value = null
}

async function post() {
  if (!draftText.value.trim()) return
  posting.value = true
  try {
    let imageUrl: string | undefined
    if (selectedFile.value) {
      imageUrl = await tweetsApi.uploadImage(selectedFile.value)
    }
    await tweets.createTweet(draftText.value.trim(), imageUrl)
    draftText.value = ''
    clearImage()
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

async function loadMore() {
  const before = tweets.timeline.length
  await tweets.loadTimeline()
  const newTweets = tweets.timeline.slice(before)
  const mentions = newTweets.flatMap((t) => [...t.text.matchAll(/@(\w+)/g)].map((m) => m[1]))
  const unique = [...new Set(mentions)]
  if (unique.length) mentionsStore.validateBatch(unique)
}
</script>
