import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/useAuthStore'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/login', name: 'login', component: () => import('@/views/LoginView.vue') },
    { path: '/register', name: 'register', component: () => import('@/views/RegisterView.vue') },
    {
      path: '/',
      name: 'home',
      component: () => import('@/views/HomeView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/profile/:username',
      name: 'profile',
      component: () => import('@/views/ProfileView.vue'),
    },
    {
      path: '/tweet/:id',
      name: 'thread',
      component: () => import('@/views/ThreadView.vue'),
      meta: { requiresAuth: true },
    },
    { path: '/notifications', name: 'notifications', component: () => import('@/views/NotificationsView.vue'), meta: { requiresAuth: true } },
    { path: '/:pathMatch(.*)*', redirect: '/' },
  ],
})

let refreshAttempted = false

router.beforeEach(async (to) => {
  const auth = useAuthStore()
  if (!refreshAttempted) {
    refreshAttempted = true
    await auth.tryRefresh()
  }
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login' }
  }
})

export default router
