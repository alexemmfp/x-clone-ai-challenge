import { defineStore } from 'pinia'
import { ref } from 'vue'
import { tweetsApi } from '@/api/tweets'
import { socialApi } from '@/api/social'
import type { Tweet } from '@/types/tweet'

export const useTweetStore = defineStore('tweets', () => {
  const timeline = ref<Tweet[]>([])
  const loading = ref(false)
  const page = ref(1)
  const hasMore = ref(true)

  async function loadTimeline(reset = false) {
    if (loading.value || (!hasMore.value && !reset)) {
      return
    }
    if (reset) {
      page.value = 1
      hasMore.value = true
      timeline.value = []
    }
    loading.value = true
    try {
      const tweets = await tweetsApi.getTimeline(page.value)
      timeline.value = reset ? tweets : [...timeline.value, ...tweets]
      hasMore.value = tweets.length === 20
      page.value++
    } finally {
      loading.value = false
    }
  }

  async function createTweet(text: string, imageUrl?: string) {
    const tweet = await tweetsApi.create({ text, ...(imageUrl ? { imageUrl } : {}) })
    if (!timeline.value.some((t) => t.id === tweet.id)) {
      timeline.value = [tweet, ...timeline.value]
    }
  }

  function prependTweet(tweet: Tweet) {
    if (!timeline.value.some((t) => t.id === tweet.id)) {
      timeline.value = [tweet, ...timeline.value]
    }
  }

  async function deleteTweet(id: string) {
    const prev = timeline.value
    timeline.value = timeline.value.filter((t) => t.id !== id)
    try {
      await tweetsApi.delete(id)
    } catch {
      timeline.value = prev
      alert('Failed to delete tweet. Please try again.')
    }
  }

  async function toggleLike(tweet: Tweet) {
    const prevCount = tweet.likeCount
    const prevState = tweet.likedByViewer
    try {
      if (tweet.likedByViewer) {
        tweet.likeCount--
        tweet.likedByViewer = false
        await socialApi.unlikeTweet(tweet.id)
      } else {
        tweet.likeCount++
        tweet.likedByViewer = true
        await socialApi.likeTweet(tweet.id)
      }
    } catch {
      tweet.likeCount = prevCount
      tweet.likedByViewer = prevState
    }
  }

  return { timeline, loading, hasMore, loadTimeline, createTweet, deleteTweet, toggleLike, prependTweet }
})
