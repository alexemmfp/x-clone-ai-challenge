<script setup lang="ts">
import { useAuthStore } from '@/stores/useAuthStore'
const auth = useAuthStore()
</script>
<template>
  <div class="p-4">
    <h1 class="text-xl font-bold">Home</h1>
    <p class="text-gray-500">Logged in as {{ auth.user?.username }}</p>
  </div>
</template>
