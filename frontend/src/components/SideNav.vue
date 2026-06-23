<template>
  <aside class="hidden sm:flex fixed left-0 top-0 h-full z-40 flex-col justify-between bg-white border-r border-gray-100 w-20 lg:w-64 p-3 lg:p-4">
    <!-- Nav items -->
    <nav class="flex flex-col gap-1">
      <RouterLink
        to="/"
        class="flex items-center gap-3 rounded-full p-3 hover:bg-gray-100 transition"
        :class="route.path === '/' ? 'text-sky-500 font-bold' : 'text-gray-700'"
      >
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-6 h-6 flex-shrink-0">
          <path d="M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z"/>
        </svg>
        <span class="hidden lg:block text-sm">Home</span>
      </RouterLink>

      <RouterLink
        to="/notifications"
        class="flex items-center gap-3 rounded-full p-3 hover:bg-gray-100 transition"
        :class="route.path === '/notifications' ? 'text-sky-500 font-bold' : 'text-gray-700'"
      >
        <div class="relative flex-shrink-0">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-6 h-6">
            <path d="M12 22c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2zm6-6V11c0-3.07-1.64-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.63 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z"/>
          </svg>
          <span v-if="notifs.unreadCount > 0"
            class="absolute -top-1 -right-1 bg-sky-500 text-white text-[10px] rounded-full min-w-[16px] h-4 flex items-center justify-center px-0.5 leading-none">
            {{ notifs.unreadCount > 9 ? '9+' : notifs.unreadCount }}
          </span>
        </div>
        <span class="hidden lg:block text-sm">Notifications</span>
      </RouterLink>

      <RouterLink
        :to="`/profile/${auth.user?.username}`"
        class="flex items-center gap-3 rounded-full p-3 hover:bg-gray-100 transition"
        :class="route.path.startsWith('/profile') ? 'text-sky-500 font-bold' : 'text-gray-700'"
      >
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-6 h-6 flex-shrink-0">
          <path d="M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z"/>
        </svg>
        <span class="hidden lg:block text-sm">Profile</span>
      </RouterLink>
    </nav>

    <!-- Search -->
    <div class="relative my-2" ref="searchContainer">
      <!-- lg: full search input -->
      <div class="hidden lg:flex items-center gap-2 bg-gray-100 rounded-full px-3 py-2">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-4 h-4 text-gray-400 flex-shrink-0">
          <path d="M15.5 14h-.79l-.28-.27A6.471 6.471 0 0 0 16 9.5 6.5 6.5 0 1 0 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z"/>
        </svg>
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search users"
          class="bg-transparent text-sm outline-none w-full text-gray-700 placeholder-gray-400"
          @focus="searchOpen = true"
          @keydown.escape="closeSearch"
        />
      </div>

      <!-- sm: icon-only search button -->
      <button
        class="flex lg:hidden w-12 h-12 rounded-full hover:bg-gray-100 items-center justify-center mx-auto transition text-gray-700"
        @click="searchOpen = !searchOpen"
      >
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-6 h-6">
          <path d="M15.5 14h-.79l-.28-.27A6.471 6.471 0 0 0 16 9.5 6.5 6.5 0 1 0 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z"/>
        </svg>
      </button>

      <!-- Results dropdown -->
      <div
        v-if="searchOpen && searchResults.length > 0"
        class="absolute left-0 right-0 bg-white border border-gray-200 rounded-2xl shadow-lg mt-1 overflow-hidden z-50"
      >
        <RouterLink
          v-for="u in searchResults"
          :key="u.id"
          :to="`/profile/${u.username}`"
          class="flex items-center gap-2 px-3 py-2 hover:bg-sky-50 transition"
          @click="closeSearch"
        >
          <img v-if="u.avatarUrl" :src="u.avatarUrl" class="w-8 h-8 rounded-full object-cover flex-shrink-0" alt="avatar" />
          <span v-else class="w-8 h-8 rounded-full bg-gray-200 flex items-center justify-center flex-shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-4 h-4 text-gray-400">
              <path d="M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z"/>
            </svg>
          </span>
          <div class="min-w-0">
            <div class="text-sm font-medium truncate text-gray-900">{{ u.displayName ?? u.username }}</div>
            <div class="text-xs text-gray-400 truncate">@{{ u.username }}</div>
          </div>
        </RouterLink>
      </div>
    </div>

    <!-- User card -->
    <div class="relative" ref="userCardContainer">
      <!-- lg: full card with name + ··· menu -->
      <div
        class="hidden lg:flex items-center gap-3 p-3 rounded-full hover:bg-gray-100 cursor-pointer"
        @click="router.push(`/profile/${auth.user?.username}`)"
      >
        <div class="w-9 h-9 rounded-full bg-gray-200 flex items-center justify-center overflow-hidden flex-shrink-0">
          <img v-if="auth.user?.avatarUrl" :src="auth.user.avatarUrl" class="w-full h-full object-cover" alt="avatar" />
          <svg v-else xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5 text-gray-400">
            <path d="M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z"/>
          </svg>
        </div>
        <div class="flex flex-col min-w-0 flex-1">
          <span class="text-sm font-semibold truncate">{{ profile?.username ?? auth.user?.username }}</span>
          <span class="text-xs text-gray-400 truncate">@{{ auth.user?.username }}</span>
        </div>
        <button
          class="ml-auto text-gray-400 hover:text-gray-700 font-bold px-1"
          @click.stop="menuOpen = !menuOpen"
        >···</button>
      </div>

      <!-- logout dropdown (lg) -->
      <div
        v-if="menuOpen"
        class="hidden lg:block absolute bottom-full left-0 right-0 mb-1 bg-white border border-gray-200 rounded-2xl shadow-lg overflow-hidden z-50"
      >
        <button
          class="w-full text-left px-4 py-3 text-sm font-semibold text-gray-800 hover:bg-gray-50 transition"
          @click="menuOpen = false; auth.logout()"
        >
          Cerrar sesión @{{ auth.user?.username }}
        </button>
      </div>

      <!-- sm: icon-only avatar → navigate to profile -->
      <button
        class="flex lg:hidden w-10 h-10 rounded-full bg-gray-200 items-center justify-center overflow-hidden mx-auto hover:opacity-80 transition"
        @click="router.push(`/profile/${auth.user?.username}`)"
        title="Mi perfil"
      >
        <img v-if="auth.user?.avatarUrl" :src="auth.user.avatarUrl" class="w-full h-full object-cover" alt="avatar" />
        <svg v-else xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5 text-gray-400">
          <path d="M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z"/>
        </svg>
      </button>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/useAuthStore'
import { useNotificationsStore } from '@/stores/useNotificationsStore'
import { socialApi } from '@/api/social'
import { usersApi, type UserSearchResult } from '@/api/users'
import type { Profile } from '@/types/profile'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const notifs = useNotificationsStore()
const profile = ref<Profile | null>(null)

const menuOpen = ref(false)
const userCardContainer = ref<HTMLElement | null>(null)

const searchQuery = ref('')
const searchResults = ref<UserSearchResult[]>([])
const searchOpen = ref(false)
const searchContainer = ref<HTMLElement | null>(null)

let debounceTimer: ReturnType<typeof setTimeout> | null = null

watch(searchQuery, (q) => {
  if (debounceTimer) clearTimeout(debounceTimer)
  if (!q.trim()) { searchResults.value = []; return }
  debounceTimer = setTimeout(async () => {
    try {
      searchResults.value = (await usersApi.searchUsers(q)).slice(0, 6)
      searchOpen.value = true
    } catch {
      searchResults.value = []
    }
  }, 200)
})

function closeSearch() {
  searchOpen.value = false
  searchQuery.value = ''
  searchResults.value = []
}

function onClickOutside(e: MouseEvent) {
  if (searchContainer.value && !searchContainer.value.contains(e.target as Node)) {
    closeSearch()
  }
  if (userCardContainer.value && !userCardContainer.value.contains(e.target as Node)) {
    menuOpen.value = false
  }
}

onMounted(async () => {
  document.addEventListener('mousedown', onClickOutside)
  if (auth.user?.username) {
    try {
      profile.value = await socialApi.getProfile(auth.user.username)
      if (auth.user && profile.value?.avatarUrl) auth.user.avatarUrl = profile.value.avatarUrl
    } catch {
      // silently ignore — avatar/name will fall back to username
    }
  }
})

onUnmounted(() => {
  document.removeEventListener('mousedown', onClickOutside)
})
</script>
