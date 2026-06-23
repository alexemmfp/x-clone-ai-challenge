import { ref, onUnmounted, getCurrentInstance } from 'vue'
import { usersApi, type UserSearchResult } from '@/api/users'

export function useMentionAutocomplete(
  getText: () => string,
  setText: (v: string) => void,
) {
  const suggestions = ref<UserSearchResult[]>([])
  const isOpen = ref(false)
  const query = ref('')
  const triggerStart = ref(-1)

  let debounceTimer: ReturnType<typeof setTimeout> | null = null

  function onInput(caretPos: number) {
    const text = getText()
    let i = caretPos - 1
    while (i >= 0 && /\w/.test(text[i])) i--
    if (i >= 0 && text[i] === '@') {
      const partial = text.slice(i + 1, caretPos)
      triggerStart.value = i
      query.value = partial
      if (debounceTimer) clearTimeout(debounceTimer)
      debounceTimer = setTimeout(async () => {
        if (query.value.length === 0) { suggestions.value = []; isOpen.value = false; return }
        try {
          const results = await usersApi.searchUsers(query.value)
          suggestions.value = results.slice(0, 5)
          isOpen.value = suggestions.value.length > 0
        } catch {
          close()
        }
      }, 150)
    } else {
      close()
    }
  }

  function pick(username: string) {
    const text = getText()
    const before = text.slice(0, triggerStart.value)
    const after = text.slice(triggerStart.value + 1 + query.value.length)
    setText(`${before}@${username} ${after}`)
    close()
  }

  function close() {
    isOpen.value = false
    suggestions.value = []
    query.value = ''
    triggerStart.value = -1
    if (debounceTimer) { clearTimeout(debounceTimer); debounceTimer = null }
  }

  function onKeydown(e: KeyboardEvent) {
    if (!isOpen.value) return
    if (e.key === 'Escape') { e.preventDefault(); close() }
  }

  if (getCurrentInstance()) {
    onUnmounted(() => {
      if (debounceTimer) { clearTimeout(debounceTimer); debounceTimer = null }
    })
  }

  return { suggestions, isOpen, onInput, pick, close, onKeydown }
}
