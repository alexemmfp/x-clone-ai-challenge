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

  async function createTweet(text: string) {
    const tweet = await tweetsApi.create({ text })
    timeline.value = [tweet, ...timeline.value]
  }

  async function deleteTweet(id: string) {
    await tweetsApi.delete(id)
    timeline.value = timeline.value.filter((t) => t.id !== id)
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

  return { timeline, loading, hasMore, loadTimeline, createTweet, deleteTweet, toggleLike }
})
