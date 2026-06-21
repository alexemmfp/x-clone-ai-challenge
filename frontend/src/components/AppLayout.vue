<template>
  <div class="min-h-screen bg-gray-50 flex">
    <SideNav v-if="auth.isAuthenticated" />
    <BottomNav v-if="auth.isAuthenticated" />
    <div :class="auth.isAuthenticated ? 'flex-1 sm:ml-20 lg:ml-64 pb-16 sm:pb-0' : 'flex-1'">
      <RouterView />
    </div>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/useAuthStore'
import SideNav from '@/components/SideNav.vue'
import BottomNav from '@/components/BottomNav.vue'
import { useTimelineHub } from '@/composables/useTimelineHub'
import { useTweetStore } from '@/stores/useTweetStore'

const auth = useAuthStore()
const tweets = useTweetStore()
useTimelineHub((tweet) => tweets.prependTweet(tweet))
</script>
