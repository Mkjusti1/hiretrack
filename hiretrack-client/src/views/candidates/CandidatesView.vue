<template>
  <div class="min-h-screen bg-gray-50">
    <nav class="bg-white border-b px-6 py-4 flex justify-between items-center">
      <router-link to="/dashboard" class="text-xl font-bold text-blue-600">HireTrack</router-link>
      <div class="flex items-center gap-4">
        <router-link to="/jobs" class="text-sm text-gray-600 hover:text-blue-600">Jobs</router-link>
        <router-link to="/candidates" class="text-sm text-blue-600 font-medium">Candidates</router-link>
      </div>
    </nav>
    <div class="max-w-5xl mx-auto px-6 py-10">
      <h2 class="text-2xl font-bold text-gray-900 mb-6">All Candidates</h2>
      <div class="bg-white rounded-xl shadow-sm border overflow-hidden">
        <table class="w-full text-sm">
          <thead class="bg-gray-50 border-b">
            <tr>
              <th class="text-left px-5 py-3 font-medium text-gray-600">Name</th>
              <th class="text-left px-5 py-3 font-medium text-gray-600">Email</th>
              <th class="text-left px-5 py-3 font-medium text-gray-600">Phone</th>
              <th class="text-left px-5 py-3 font-medium text-gray-600">Applications</th>
              <th class="text-left px-5 py-3 font-medium text-gray-600">Added</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="candidates.length === 0">
              <td colspan="5" class="text-center py-10 text-gray-400">No candidates yet</td>
            </tr>
            <tr v-for="c in candidates" :key="c.id" class="border-b last:border-0 hover:bg-gray-50">
              <td class="px-5 py-3 font-medium text-gray-800">{{ c.name }}</td>
              <td class="px-5 py-3 text-gray-500">{{ c.email }}</td>
              <td class="px-5 py-3 text-gray-500">{{ c.phone ?? '—' }}</td>
              <td class="px-5 py-3">
                <span class="bg-blue-50 text-blue-600 px-2 py-1 rounded-full text-xs font-medium">
                  {{ c.applicationCount }} job{{ c.applicationCount !== 1 ? 's' : '' }}
                </span>
              </td>
              <td class="px-5 py-3 text-gray-400">{{ formatDate(c.createdAt) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import client from '../../api/client'

interface Candidate {
  id: string
  name: string
  email: string
  phone: string | null
  resumeUrl: string | null
  applicationCount: number
  createdAt: string
}

const candidates = ref<Candidate[]>([])

onMounted(async () => {
  const res = await client.get<Candidate[]>('/api/candidates')
  candidates.value = res.data
})

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' })
}
</script>
