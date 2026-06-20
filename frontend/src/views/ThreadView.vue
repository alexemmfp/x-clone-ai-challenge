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
          <MentionText :text="parent.text" class="text-gray-800 text-sm md:text-base whitespace-pre-wrap" />
          <a v-if="parent.imageUrl" :href="parent.imageUrl" target="_blank" rel="noopener">
            <img :src="parent.imageUrl" class="rounded-lg max-w-full object-contain" alt="tweet image" />
          </a>
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
          <div v-if="replyImagePreview" class="relative inline-block">
            <img :src="replyImagePreview" class="max-h-32 rounded-lg object-cover" alt="preview" />
            <button
              class="absolute top-1 right-1 bg-black/50 text-white rounded-full w-5 h-5 text-xs flex items-center justify-center"
              @click="clearReplyImage"
            >✕</button>
          </div>
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-2">
              <label data-testid="reply-image-btn" class="cursor-pointer text-sky-500 hover:text-sky-600 text-sm">
                📷
                <input
                  data-testid="reply-image-input"
                  type="file"
                  accept="image/jpeg,image/png,image/gif,image/webp"
                  class="hidden"
                  @change="onReplyImageChange"
                />
              </label>
              <span class="text-xs text-gray-400">{{ replyText.length }}/280</span>
            </div>
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
          <MentionText :text="reply.text" class="text-gray-800 text-sm whitespace-pre-wrap" />
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
import MentionText from '@/components/MentionText.vue'
import { useMentionsStore } from '@/stores/useMentionsStore'

const route = useRoute()
const mentionsStore = useMentionsStore()
const loading = ref(true)
const parent = ref<Tweet | null>(null)
const replies = ref<Tweet[]>([])
const replyText = ref('')
const submitting = ref(false)
const selectedReplyFile = ref<File | null>(null)
const replyImagePreview = ref<string | null>(null)

onMounted(async () => {
  const id = route.params.id as string
  try {
    const [tweet, reps] = await Promise.all([tweetsApi.getById(id), tweetsApi.getReplies(id)])
    parent.value = tweet
    replies.value = reps
    const allText = [tweet.text, ...reps.map((r) => r.text)]
    const mentions = allText.flatMap((t) => [...t.matchAll(/@(\w+)/g)].map((m) => m[1]))
    const unique = [...new Set(mentions)]
    if (unique.length) mentionsStore.validateBatch(unique)
  } finally {
    loading.value = false
  }
})

function onReplyImageChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0] ?? null
  selectedReplyFile.value = file
  replyImagePreview.value = file ? URL.createObjectURL(file) : null
}

function clearReplyImage() {
  selectedReplyFile.value = null
  replyImagePreview.value = null
}

async function submitReply() {
  if (!replyText.value.trim() || !parent.value) return
  submitting.value = true
  try {
    let imageUrl: string | undefined
    if (selectedReplyFile.value) {
      imageUrl = await tweetsApi.uploadImage(selectedReplyFile.value)
    }
    const reply = await tweetsApi.create({ text: replyText.value.trim(), parentId: parent.value.id, imageUrl })
    replies.value = [...replies.value, reply]
    replyText.value = ''
    clearReplyImage()
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
