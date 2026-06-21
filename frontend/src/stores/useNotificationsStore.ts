import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { AppNotification } from '@/types/notification'

export const useNotificationsStore = defineStore('notifications', () => {
  const notifications = ref<AppNotification[]>([])
  const unreadCount = computed(() => notifications.value.filter(n => !n.read).length)
  function add(n: AppNotification) { notifications.value.unshift(n) }
  function markAllRead() { notifications.value.forEach(n => { n.read = true }) }
  return { notifications, unreadCount, add, markAllRead }
})
