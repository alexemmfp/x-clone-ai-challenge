<template>
  <ul
    v-if="isOpen && suggestions.length"
    class="absolute z-50 bg-white border border-gray-200 rounded-xl shadow-lg mt-1 w-64 overflow-hidden"
  >
    <li
      v-for="u in suggestions"
      :key="u.username"
      class="flex items-center gap-2 px-3 py-2 hover:bg-sky-50 cursor-pointer text-sm"
      @mousedown.prevent="$emit('pick', u.username)"
    >
      <img v-if="u.avatarUrl" :src="u.avatarUrl" class="w-7 h-7 rounded-full object-cover flex-shrink-0" alt="avatar" />
      <span v-else class="w-7 h-7 rounded-full bg-gray-200 flex items-center justify-center flex-shrink-0">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-4 h-4 text-gray-400">
          <path d="M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z"/>
        </svg>
      </span>
      <div class="min-w-0">
        <div class="font-medium truncate">{{ u.displayName ?? u.username }}</div>
        <div class="text-xs text-gray-400 truncate">@{{ u.username }}</div>
      </div>
    </li>
  </ul>
</template>

<script setup lang="ts">
import type { UserSearchResult } from '@/api/users'

defineProps<{
  isOpen: boolean
  suggestions: UserSearchResult[]
}>()
defineEmits<{ pick: [username: string] }>()
</script>
