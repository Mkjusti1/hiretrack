<template>
  <div class="min-h-screen bg-gray-50">
    <nav class="bg-white border-b px-6 py-4 flex justify-between items-center">
      <router-link to="/dashboard" class="text-xl font-bold text-blue-600">HireTrack</router-link>
      <button @click="showCreate = true" class="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700">+ New Job</button>
    </nav>
    <div class="max-w-5xl mx-auto px-6 py-10">
      <h2 class="text-2xl font-bold text-gray-900 mb-6">Job Listings</h2>
      <div class="space-y-4">
        <div v-for="job in jobs" :key="job.id" class="bg-white rounded-xl p-5 shadow-sm border flex justify-between items-center">
          <div>
            <h3 class="font-semibold text-gray-900">{{ job.title }}</h3>
            <p class="text-sm text-gray-500">{{ job.department }} · {{ job.location }}</p>
            <span class="inline-block mt-2 text-xs px-2 py-1 rounded-full" :class="job.status === 'Open' ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'">{{ job.status }}</span>
          </div>
          <div class="text-right">
            <p class="text-2xl font-bold text-blue-600">{{ job.applicationCount }}</p>
            <p class="text-xs text-gray-400">applicants</p>
            <router-link :to="`/jobs/${job.id}/applications`" class="inline-block mt-2 text-sm text-blue-600 hover:underline">View pipeline →</router-link>
          </div>
        </div>
      </div>
    </div>
    <!-- Create Job Modal -->
    <div v-if="showCreate" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div class="bg-white rounded-xl p-6 w-full max-w-md shadow-xl">
        <h3 class="text-lg font-bold mb-4">Create New Job</h3>
        <form @submit.prevent="handleCreate" class="space-y-3">
          <input v-model="form.title" placeholder="Job Title" required class="w-full border rounded-lg px-3 py-2 text-sm" />
          <input v-model="form.department" placeholder="Department" required class="w-full border rounded-lg px-3 py-2 text-sm" />
          <input v-model="form.location" placeholder="Location" required class="w-full border rounded-lg px-3 py-2 text-sm" />
          <textarea v-model="form.description" placeholder="Description (optional)" class="w-full border rounded-lg px-3 py-2 text-sm" rows="3" />
          <div class="flex gap-3 pt-2">
            <button type="submit" class="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm font-medium hover:bg-blue-700">Create</button>
            <button type="button" @click="showCreate = false" class="flex-1 border py-2 rounded-lg text-sm">Cancel</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import client from '../../api/client'
import type { Job } from '../../types'

const jobs = ref<Job[]>([])
const showCreate = ref(false)
const form = ref({ title: '', department: '', location: '', description: '' })

onMounted(async () => { await loadJobs() })

async function loadJobs() {
  const res = await client.get<Job[]>('/api/jobs')
  jobs.value = res.data
}

async function handleCreate() {
  await client.post('/api/jobs', form.value)
  showCreate.value = false
  form.value = { title: '', department: '', location: '', description: '' }
  await loadJobs()
}
</script>
