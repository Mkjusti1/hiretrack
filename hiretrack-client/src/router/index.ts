import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/dashboard' },
    { path: '/login', component: () => import('../views/auth/LoginView.vue') },
    { path: '/register', component: () => import('../views/auth/RegisterView.vue') },
    {
      path: '/dashboard',
      component: () => import('../views/dashboard/DashboardView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/jobs',
      component: () => import('../views/jobs/JobsView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/jobs/:id/applications',
      component: () => import('../views/applications/ApplicationsView.vue'),
      meta: { requiresAuth: true }
    },
    {
  path: '/candidates',
  component: () => import('../views/candidates/CandidatesView.vue'),
  meta: { requiresAuth: true }
},
  ]
})

router.beforeEach((to) => {
  const token = localStorage.getItem('token')
  if (to.meta.requiresAuth && !token) return '/login'
})

export default router
