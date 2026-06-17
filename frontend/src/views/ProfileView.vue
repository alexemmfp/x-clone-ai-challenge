<template>
  <div class="min-h-screen bg-gray-50">
    <div class="max-w-xl mx-auto py-6 px-4 space-y-4">
      <RouterLink to="/" class="text-sm text-sky-500 hover:underline">← Home</RouterLink>

      <div v-if="loading" class="text-center text-gray-400 py-12">Loading…</div>

      <div v-else-if="profile" class="bg-white rounded-2xl shadow p-6 space-y-3">
        <div class="flex items-start justify-between">
          <div>
            <h1 class="text-xl font-bold text-gray-900">@{{ profile.username }}</h1>
            <p v-if="profile.bio" class="text-sm text-gray-500 mt-1">{{ profile.bio }}</p>
          </div>

          <button
            v-if="auth.user && auth.user.id !== profile.id"
            class="text-sm font-semibold rounded-full px-4 py-1.5 transition border"
            :class="profile.isFollowedByViewer
              ? 'border-gray-300 text-gray-700 hover:bg-gray-50'
              : 'bg-sky-500 text-white border-sky-500 hover:bg-sky-600'"
            @click="toggleFollow"
          >
            {{ profile.isFollowedByViewer ? 'Unfollow' : 'Follow' }}
          </button>
        </div>

        <div class="flex gap-6 text-sm text-gray-500">
          <span><strong class="text-gray-900">{{ profile.followingCount }}</strong> Following</span>
          <span><strong class="text-gray-900">{{ profile.followerCount }}</strong> Followers</span>
        </div>
      </div>

      <div v-else class="text-center text-gray-400 py-12">User not found.</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { socialApi } from '@/api/social'
import { useAuthStore } from '@/stores/useAuthStore'
import type { Profile } from '@/types/profile'

const route = useRoute()
const auth = useAuthStore()

const profile = ref<Profile | null>(null)
const loading = ref(false)

async function loadProfile() {
  loading.value = true
  try {
    profile.value = await socialApi.getProfile(route.params.username as string)
  } catch {
    profile.value = null
  } finally {
    loading.value = false
  }
}

async function toggleFollow() {
  if (!profile.value) return
  if (profile.value.isFollowedByViewer) {
    await socialApi.unfollow(profile.value.username)
    profile.value.isFollowedByViewer = false
    profile.value.followerCount--
  } else {
    await socialApi.follow(profile.value.username)
    profile.value.isFollowedByViewer = true
    profile.value.followerCount++
  }
}

onMounted(loadProfile)
watch(() => route.params.username, loadProfile)
</script>
