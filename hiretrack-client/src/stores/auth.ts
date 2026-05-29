import { defineStore } from 'pinia'
import { ref } from 'vue'
import client from '../api/client'
import type { AuthResponse } from '../types'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const user = ref<Omit<AuthResponse, 'token'> | null>(null)

  const isAuthenticated = ref(!!token.value)

  async function login(email: string, password: string) {
    const res = await client.post<AuthResponse>('/api/auth/login', { email, password })
    token.value = res.data.token
    user.value = { email: res.data.email, firstName: res.data.firstName, lastName: res.data.lastName, role: res.data.role, tenantId: res.data.tenantId }
    localStorage.setItem('token', res.data.token)
    isAuthenticated.value = true
  }

  async function register(payload: { companyName: string; firstName: string; lastName: string; email: string; password: string }) {
    const res = await client.post<AuthResponse>('/api/auth/register', payload)
    token.value = res.data.token
    user.value = { email: res.data.email, firstName: res.data.firstName, lastName: res.data.lastName, role: res.data.role, tenantId: res.data.tenantId }
    localStorage.setItem('token', res.data.token)
    isAuthenticated.value = true
  }

  function logout() {
    token.value = null
    user.value = null
    isAuthenticated.value = false
    localStorage.removeItem('token')
  }

  return { token, user, isAuthenticated, login, register, logout }
})
