<template>
  <span>
    <template v-for="(part, i) in parts" :key="i">
      <RouterLink
        v-if="part.type === 'mention'"
        :to="`/profile/${part.value}`"
        class="text-sky-500 hover:underline"
      >@{{ part.value }}</RouterLink>
      <span v-else>{{ part.value }}</span>
    </template>
  </span>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { useMentionsStore } from '@/stores/useMentionsStore'

const props = defineProps<{ text: string }>()
const mentionsStore = useMentionsStore()

const parts = computed(() => {
  const segments: { type: 'text' | 'mention'; value: string }[] = []
  const regex = /@(\w+)/g
  let last = 0
  let m: RegExpExecArray | null

  while ((m = regex.exec(props.text)) !== null) {
    if (m.index > last) segments.push({ type: 'text', value: props.text.slice(last, m.index) })
    const username = m[1]
    if (mentionsStore.validatedUsernames.has(username)) {
      segments.push({ type: 'mention', value: username })
    } else {
      segments.push({ type: 'text', value: m[0] })
    }
    last = m.index + m[0].length
  }

  if (last < props.text.length) segments.push({ type: 'text', value: props.text.slice(last) })
  return segments
})

onMounted(() => {
  const mentions = [...props.text.matchAll(/@(\w+)/g)].map((m) => m[1])
  if (mentions.length > 0) mentionsStore.validateBatch(mentions)
})
</script>
