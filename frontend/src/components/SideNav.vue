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

    <!-- User card -->
    <div>
      <!-- lg: full card with name + logout button -->
      <div class="hidden lg:flex items-center gap-3 p-3 rounded-full hover:bg-gray-100 cursor-pointer" @click="auth.logout()">
        <div class="w-9 h-9 rounded-full bg-gray-200 flex items-center justify-center overflow-hidden flex-shrink-0">
          <img v-if="profile?.avatarUrl" :src="profile.avatarUrl" class="w-full h-full object-cover" alt="avatar" />
          <svg v-else xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5 text-gray-400">
            <path d="M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z"/>
          </svg>
        </div>
        <div class="flex flex-col min-w-0 flex-1">
          <span class="text-sm font-semibold truncate">{{ profile?.username ?? auth.user?.username }}</span>
          <span class="text-xs text-gray-400 truncate">@{{ auth.user?.username }}</span>
        </div>
        <button class="ml-auto text-gray-400 hover:text-gray-700 font-bold px-1" @click.stop="auth.logout()">···</button>
      </div>

      <!-- sm: icon-only avatar -->
      <button
        class="flex lg:hidden w-10 h-10 rounded-full bg-gray-200 items-center justify-center overflow-hidden mx-auto hover:opacity-80 transition"
        @click="auth.logout()"
        title="Sign out"
      >
        <img v-if="profile?.avatarUrl" :src="profile.avatarUrl" class="w-full h-full object-cover" alt="avatar" />
        <svg v-else xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5 text-gray-400">
          <path d="M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z"/>
        </svg>
      </button>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/useAuthStore'
import { useNotificationsStore } from '@/stores/useNotificationsStore'
import { socialApi } from '@/api/social'
import type { Profile } from '@/types/profile'

const auth = useAuthStore()
const route = useRoute()
const notifs = useNotificationsStore()
const profile = ref<Profile | null>(null)

onMounted(async () => {
  if (auth.user?.username) {
    try {
      profile.value = await socialApi.getProfile(auth.user.username)
    } catch {
      // silently ignore — avatar/name will fall back to username
    }
  }
})
</script>
