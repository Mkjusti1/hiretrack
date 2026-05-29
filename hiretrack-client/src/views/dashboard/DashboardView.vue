<template>
  <div class="min-h-screen bg-gray-50">
    <nav class="bg-white border-b px-6 py-4 flex justify-between items-center">
      <h1 class="text-xl font-bold text-blue-600">HireTrack</h1>
      <div class="flex items-center gap-4">
<router-link to="/jobs" class="text-sm text-gray-600 hover:text-blue-600">Jobs</router-link>
<router-link to="/candidates" class="text-sm text-gray-600 hover:text-blue-600">Candidates</router-link>       
 <button @click="handleLogout" class="text-sm text-red-500 hover:text-red-700">Logout</button>
      </div>
    </nav>
    <div class="max-w-4xl mx-auto px-6 py-10">
      <h2 class="text-2xl font-bold text-gray-900 mb-1">Welcome, {{ auth.user?.firstName }} 👋</h2>
      <p class="text-gray-500 mb-8">Here's an overview of your hiring pipeline.</p>
      <div class="grid grid-cols-3 gap-6">
        <div class="bg-white rounded-xl p-6 shadow-sm border">
          <p class="text-sm text-gray-500">Total Jobs</p>
          <p class="text-3xl font-bold text-gray-900 mt-1">{{ stats.totalJobs }}</p>
        </div>
        <div class="bg-white rounded-xl p-6 shadow-sm border">
          <p class="text-sm text-gray-500">Open Jobs</p>
          <p class="text-3xl font-bold text-green-600 mt-1">{{ stats.openJobs }}</p>
        </div>
        <div class="bg-white rounded-xl p-6 shadow-sm border">
          <p class="text-sm text-gray-500">Total Applications</p>
          <p class="text-3xl font-bold text-blue-600 mt-1">{{ stats.totalApplications }}</p>
        </div>
      </div>
      <div class="mt-8">
        <router-link to="/jobs" class="inline-block bg-blue-600 text-white px-5 py-2 rounded-lg font-medium hover:bg-blue-700">
          View all jobs →
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import client from '../../api/client'
import type { Job } from '../../types'

const auth = useAuthStore()
const router = useRouter()
const stats = ref({ totalJobs: 0, openJobs: 0, totalApplications: 0 })

onMounted(async () => {
  const res = await client.get<Job[]>('/api/jobs')
  stats.value.totalJobs = res.data.length
  stats.value.openJobs = res.data.filter(j => j.status === 'Open').length
  stats.value.totalApplications = res.data.reduce((sum, j) => sum + j.applicationCount, 0)
})

function handleLogout() {
  auth.logout()
  router.push('/login')
}
</script>
