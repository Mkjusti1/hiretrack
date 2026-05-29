<template>
  <div class="min-h-screen bg-gray-50">
    <nav class="bg-white border-b px-6 py-4 flex justify-between items-center">
      <router-link to="/jobs" class="text-xl font-bold text-blue-600">← Jobs</router-link>
      <h2 class="font-semibold text-gray-700">{{ jobTitle }} — Pipeline</h2>
      <button @click="showCreate = true" class="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">+ Add Applicant</button>
    </nav>
    <div class="max-w-7xl mx-auto px-6 py-8">
      <div class="grid grid-cols-6 gap-4">
        <div v-for="stage in stages" :key="stage" class="bg-white rounded-xl shadow-sm border">
          <div class="px-4 py-3 border-b">
            <h3 class="font-semibold text-sm text-gray-700">{{ stage }}</h3>
            <p class="text-xs text-gray-400">{{ byStage(stage).length }} candidates</p>
          </div>
          <div class="p-3 space-y-2 min-h-32">
            <div v-for="app in byStage(stage)" :key="app.id" class="bg-gray-50 rounded-lg p-3 border text-sm">
              <p class="font-medium text-gray-800">{{ app.candidateName }}</p>
              <p class="text-xs text-gray-400 mb-2">{{ app.candidateEmail }}</p>
              <div v-if="stage !== 'Hired' && stage !== 'Rejected'" class="flex flex-col gap-1">
                <button v-for="next in getNextStages(stage)" :key="next" @click="moveStage(app.id, next)" class="text-xs bg-blue-50 text-blue-600 px-2 py-1 rounded hover:bg-blue-100">
                  → {{ next }}
                </button>
                <button @click="moveStage(app.id, 'Rejected')" class="text-xs bg-red-50 text-red-500 px-2 py-1 rounded hover:bg-red-100">Reject</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <!-- Add Applicant Modal -->
    <div v-if="showCreate" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div class="bg-white rounded-xl p-6 w-full max-w-md shadow-xl">
        <h3 class="text-lg font-bold mb-4">Add Applicant</h3>
        <form @submit.prevent="handleCreate" class="space-y-3">
          <input v-model="form.candidateName" placeholder="Full Name" required class="w-full border rounded-lg px-3 py-2 text-sm" />
          <input v-model="form.candidateEmail" placeholder="Email" type="email" required class="w-full border rounded-lg px-3 py-2 text-sm" />
          <input v-model="form.candidatePhone" placeholder="Phone (optional)" class="w-full border rounded-lg px-3 py-2 text-sm" />
          <textarea v-model="form.coverNote" placeholder="Cover note (optional)" class="w-full border rounded-lg px-3 py-2 text-sm" rows="3" />
          <div class="flex gap-3 pt-2">
<button type="submit" :disabled="creating" class="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm font-medium disabled:opacity-50">
  {{ creating ? 'Adding...' : 'Add' }}
</button>           
 <button type="button" @click="showCreate = false" class="flex-1 border py-2 rounded-lg text-sm">Cancel</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import client from '../../api/client'
import type { Application } from '../../types'

const route = useRoute()
const jobId = route.params.id as string
const jobTitle = ref('')
const applications = ref<Application[]>([])
const showCreate = ref(false)
const form = ref({ candidateName: '', candidateEmail: '', candidatePhone: '', coverNote: '' })

const stages = ['Applied', 'Screened', 'Interview', 'Offer', 'Hired', 'Rejected']

const nextStageMap: Record<string, string[]> = {
  Applied: ['Screened'],
  Screened: ['Interview'],
  Interview: ['Offer'],
  Offer: ['Hired'],
}

function getNextStages(stage: string): string[] {
  return nextStageMap[stage] ?? []
}

function byStage(stage: string) {
  return applications.value.filter(a => a.stage === stage)
}

onMounted(async () => {
  const [appsRes, jobRes] = await Promise.all([
    client.get<Application[]>(`/api/applications?jobId=${jobId}`),
    client.get(`/api/jobs/${jobId}`)
  ])
  applications.value = appsRes.data
  jobTitle.value = jobRes.data.title
})

async function moveStage(appId: string, toStage: string) {
  await client.put(`/api/applications/${appId}/stage`, { toStage, note: null })
  const res = await client.get<Application[]>(`/api/applications?jobId=${jobId}`)
  applications.value = res.data
}

async function handleCreate() {
  if (creating.value) return
  creating.value = true
  try {
    await client.post('/api/applications', { jobId, ...form.value })
    showCreate.value = false
    form.value = { candidateName: '', candidateEmail: '', candidatePhone: '', coverNote: '' }
    const res = await client.get<Application[]>(`/api/applications?jobId=${jobId}`)
    applications.value = res.data
  } finally {
    creating.value = false
  }
}
</script>
