<template>
  <nav class="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-100 flex justify-around py-2 z-40 sm:hidden">
    <RouterLink
      to="/"
      class="p-3"
      :class="route.path === '/' ? 'text-sky-500' : 'text-gray-500'"
    >
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-6 h-6">
        <path d="M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z"/>
      </svg>
    </RouterLink>

    <RouterLink
      to="/notifications"
      class="p-3 relative"
      :class="route.path === '/notifications' ? 'text-sky-500' : 'text-gray-500'"
    >
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-6 h-6">
        <path d="M12 22c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2zm6-6V11c0-3.07-1.64-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.63 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z"/>
      </svg>
      <span
v-if="notifs.unreadCount > 0"
        class="absolute top-1 right-1 bg-sky-500 text-white text-[10px] rounded-full min-w-[14px] h-3.5 flex items-center justify-center px-0.5 leading-none">
        {{ notifs.unreadCount > 9 ? '9+' : notifs.unreadCount }}
      </span>
    </RouterLink>

    <RouterLink
      :to="`/profile/${auth.user?.username}`"
      class="p-3"
      :class="route.path.startsWith('/profile') ? 'text-sky-500' : 'text-gray-500'"
    >
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-6 h-6">
        <path d="M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z"/>
      </svg>
    </RouterLink>
  </nav>
</template>

<script setup lang="ts">
import { RouterLink, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/useAuthStore'
import { useNotificationsStore } from '@/stores/useNotificationsStore'

const auth = useAuthStore()
const route = useRoute()
const notifs = useNotificationsStore()
</script>
