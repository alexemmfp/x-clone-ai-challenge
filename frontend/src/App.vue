<script setup lang="ts">
import { onMounted } from 'vue'
import AppLayout from '@/components/AppLayout.vue'
import { useAuthStore } from '@/stores/useAuthStore'

const auth = useAuthStore()
onMounted(() => { if (!auth.isAuthenticated) auth.tryRefresh() })
</script>

<template>
  <AppLayout />
</template>
