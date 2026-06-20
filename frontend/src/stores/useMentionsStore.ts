import { defineStore } from 'pinia'
import { ref } from 'vue'
import { usersApi } from '@/api/users'

export const useMentionsStore = defineStore('mentions', () => {
  const validatedUsernames = ref(new Set<string>())
  const pending = ref(new Set<string>())

  async function validateBatch(words: string[]): Promise<void> {
    const unknown = words.filter(
      (w) => !validatedUsernames.value.has(w) && !pending.value.has(w),
    )
    if (unknown.length === 0) return

    unknown.forEach((w) => pending.value.add(w))
    try {
      const result = await usersApi.validateUsernames(unknown)
      Object.entries(result).forEach(([username, exists]) => {
        if (exists) validatedUsernames.value.add(username)
        pending.value.delete(username)
      })
    } catch {
      unknown.forEach((w) => pending.value.delete(w))
    }
  }

  return { validatedUsernames, validateBatch }
})
