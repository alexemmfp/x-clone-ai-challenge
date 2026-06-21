<template>
  <div class="min-h-screen bg-gray-50">
    <div class="max-w-xl md:max-w-2xl mx-auto py-6 px-4 md:px-6 space-y-4">
      <RouterLink to="/" class="text-sm text-sky-500 hover:underline">← Home</RouterLink>

      <div v-if="loading" class="text-center text-gray-400 py-12">Loading…</div>

      <div v-else-if="profile" class="bg-white rounded-2xl shadow p-4 sm:p-6 space-y-3">
        <!-- Avatar + header row -->
        <div class="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
          <div class="flex items-center gap-3">
            <img
              v-if="profile.avatarUrl"
              :src="profile.avatarUrl"
              class="w-16 h-16 rounded-full object-cover flex-shrink-0"
              alt="avatar"
            />
            <span
              v-else
              class="w-16 h-16 rounded-full bg-gray-200 flex items-center justify-center flex-shrink-0"
            >
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-8 h-8 text-gray-400">
                <path d="M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z"/>
              </svg>
            </span>
            <div>
              <h1 class="text-xl font-bold text-gray-900">@{{ profile.username }}</h1>
              <p v-if="profile.bio && !editing" class="text-sm text-gray-500 mt-0.5">{{ profile.bio }}</p>
              <p v-else-if="!editing" class="text-sm text-gray-400 mt-0.5 italic">No bio yet.</p>
            </div>
          </div>

          <div class="flex gap-2 flex-shrink-0">
            <!-- Edit button: own profile -->
            <button
              v-if="auth.user && auth.user.id === profile.id && !editing"
              class="text-sm font-semibold rounded-full px-4 py-1.5 border border-gray-300 text-gray-700 hover:bg-gray-50 transition"
              @click="startEdit"
            >
              Edit profile
            </button>

            <!-- Follow/Unfollow: other profile -->
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
        </div>

        <!-- Inline edit form -->
        <div v-if="editing" class="space-y-2 pt-1">
          <div>
            <label class="text-xs text-gray-500 font-medium">Bio</label>
            <textarea
              v-model="editBio"
              rows="2"
              maxlength="160"
              placeholder="Tell the world about yourself…"
              class="w-full mt-0.5 resize-none rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-sky-400"
            />
          </div>
          <div>
            <label class="text-xs text-gray-500 font-medium">Avatar URL</label>
            <input
              v-model="editAvatarUrl"
              type="url"
              maxlength="512"
              placeholder="https://example.com/avatar.jpg"
              class="w-full mt-0.5 rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-sky-400"
            />
          </div>
          <div class="flex gap-2 justify-end pt-1">
            <button
              class="text-sm px-4 py-1.5 rounded-full border border-gray-300 text-gray-600 hover:bg-gray-50"
              @click="cancelEdit"
            >
              Cancel
            </button>
            <button
              class="text-sm px-4 py-1.5 rounded-full bg-sky-500 text-white hover:bg-sky-600 disabled:opacity-50"
              :disabled="saving"
              @click="saveEdit"
            >
              {{ saving ? 'Saving…' : 'Save' }}
            </button>
          </div>
        </div>

        <!-- Stats row -->
        <div class="flex flex-wrap gap-4 sm:gap-6 text-sm text-gray-500">
          <button
            class="hover:underline"
            :class="activeTab === 'following' ? 'text-sky-500 font-semibold' : ''"
            @click="showTab('following')"
          >
            <strong class="text-gray-900">{{ profile.followingCount }}</strong> Following
          </button>
          <button
            class="hover:underline"
            :class="activeTab === 'followers' ? 'text-sky-500 font-semibold' : ''"
            @click="showTab('followers')"
          >
            <strong class="text-gray-900">{{ profile.followerCount }}</strong> Followers
          </button>
        </div>

        <!-- Followers / Following list -->
        <div v-if="activeTab" class="border-t pt-3 space-y-2">
          <div v-if="listLoading" class="text-center text-gray-400 py-4 text-sm">Loading…</div>
          <div v-else-if="userList.length === 0" class="text-center text-gray-400 py-4 text-sm">No users yet.</div>
          <RouterLink
            v-for="u in userList"
            :key="u.id"
            :to="`/profile/${u.username}`"
            class="flex items-center gap-3 p-2 rounded-xl hover:bg-gray-50 transition"
            @click="activeTab = null"
          >
            <img v-if="u.avatarUrl" :src="u.avatarUrl" class="w-9 h-9 rounded-full object-cover flex-shrink-0" alt="avatar" />
            <span v-else class="w-9 h-9 rounded-full bg-gray-200 flex items-center justify-center flex-shrink-0">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5 text-gray-400">
                <path d="M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z"/>
              </svg>
            </span>
            <div class="min-w-0">
              <div class="font-medium text-sm text-gray-900 truncate">{{ u.displayName ?? u.username }}</div>
              <div class="text-xs text-gray-400 truncate">@{{ u.username }}</div>
            </div>
          </RouterLink>
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
import type { UserSummary } from '@/types/userSummary'

const route = useRoute()
const auth = useAuthStore()

const profile = ref<Profile | null>(null)
const loading = ref(false)

const editing = ref(false)
const editBio = ref('')
const editAvatarUrl = ref('')
const saving = ref(false)

const activeTab = ref<'followers' | 'following' | null>(null)
const userList = ref<UserSummary[]>([])
const listLoading = ref(false)

async function loadProfile() {
  loading.value = true
  activeTab.value = null
  userList.value = []
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

function startEdit() {
  editBio.value = profile.value?.bio ?? ''
  editAvatarUrl.value = profile.value?.avatarUrl ?? ''
  editing.value = true
}

function cancelEdit() {
  editing.value = false
}

async function saveEdit() {
  if (!profile.value) return
  saving.value = true
  try {
    const updated = await socialApi.updateProfile({
      bio: editBio.value || undefined,
      avatarUrl: editAvatarUrl.value || undefined,
    })
    profile.value = { ...profile.value, bio: updated.bio, avatarUrl: updated.avatarUrl }
    editing.value = false
  } finally {
    saving.value = false
  }
}

async function showTab(tab: 'followers' | 'following') {
  if (activeTab.value === tab) { activeTab.value = null; return }
  activeTab.value = tab
  if (!profile.value) return
  listLoading.value = true
  userList.value = []
  try {
    userList.value = tab === 'followers'
      ? await socialApi.getFollowers(profile.value.username)
      : await socialApi.getFollowing(profile.value.username)
  } finally {
    listLoading.value = false
  }
}

onMounted(loadProfile)
watch(() => route.params.username, loadProfile)
</script>
