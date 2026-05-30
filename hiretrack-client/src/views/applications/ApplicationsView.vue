<template>
  <div class="min-h-screen bg-gray-50">
    <nav class="bg-white border-b px-6 py-4 flex justify-between items-center">
      <router-link to="/jobs" class="text-xl font-bold text-blue-600">← Jobs</router-link>
      <h2 class="font-semibold text-gray-700">{{ jobTitle }} — Pipeline</h2>
<div class="flex gap-2">
  <button @click="exportCsv" class="bg-white border border-gray-300 text-gray-700 px-4 py-2 rounded-lg text-sm hover:bg-gray-50">↓ Export CSV</button>
  <button @click="showCreate = true" class="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">+ Add Applicant</button>
</div>
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
              <div v-if="stage === 'Interview'" class="mb-2">
                <div v-if="getInterview(app.id)" class="text-xs text-gray-500 bg-blue-50 rounded p-2 mb-1">
                  <p class="font-medium text-blue-700">📅 {{ formatDate(getInterview(app.id).scheduledAt) }}</p>
                  <p>{{ getInterview(app.id).interviewerName }}</p>
                  <p v-if="getInterview(app.id).feedbackSubmitted" class="text-green-600 font-medium mt-1">
                    Feedback: {{ getInterview(app.id).rating }}/5
                  </p>
                  <button v-else @click="openFeedback(app.id)" class="mt-1 text-xs bg-yellow-50 text-yellow-700 px-2 py-1 rounded hover:bg-yellow-100 w-full">
                    Submit Feedback
                  </button>
                </div>
                <button v-else @click="openSchedule(app.id)" class="text-xs bg-blue-50 text-blue-600 px-2 py-1 rounded hover:bg-blue-100 w-full">
                  + Schedule Interview
                </button>
              </div>
              <div v-if="stage !== 'Hired' && stage !== 'Rejected'" class="flex flex-col gap-1">
                <button v-for="next in getNextStages(stage)" :key="next" @click="moveStage(app.id, next)" class="text-xs bg-blue-50 text-blue-600 px-2 py-1 rounded hover:bg-blue-100">
                  -> {{ next }}
                </button>
                <button @click="moveStage(app.id, 'Rejected')" class="text-xs bg-red-50 text-red-500 px-2 py-1 rounded hover:bg-red-100">Reject</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div v-if="showCreate" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div class="bg-white rounded-xl p-6 w-full max-w-md shadow-xl">
        <h3 class="text-lg font-bold mb-4">Add Applicant</h3>
        <form @submit.prevent="handleCreate" class="space-y-3">
          <input v-model="form.candidateName" placeholder="Full Name" required class="w-full border rounded-lg px-3 py-2 text-sm" />
          <input v-model="form.candidateEmail" placeholder="Email" type="email" required class="w-full border rounded-lg px-3 py-2 text-sm" />
          <input v-model="form.candidatePhone" placeholder="Phone (optional)" class="w-full border rounded-lg px-3 py-2 text-sm" />
          <textarea v-model="form.coverNote" placeholder="Cover note (optional)" class="w-full border rounded-lg px-3 py-2 text-sm" rows="3"></textarea>
          <div class="flex gap-3 pt-2">
            <button type="submit" :disabled="creating" class="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm font-medium disabled:opacity-50">
              {{ creating ? 'Adding...' : 'Add' }}
            </button>
            <button type="button" @click="showCreate = false" class="flex-1 border py-2 rounded-lg text-sm">Cancel</button>
          </div>
        </form>
      </div>
    </div>
    <div v-if="showSchedule" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div class="bg-white rounded-xl p-6 w-full max-w-md shadow-xl">
        <h3 class="text-lg font-bold mb-4">Schedule Interview</h3>
        <form @submit.prevent="handleSchedule" class="space-y-3">
          <div>
            <label class="block text-xs font-medium text-gray-600 mb-1">Date and Time</label>
            <input v-model="scheduleForm.scheduledAt" type="datetime-local" required class="w-full border rounded-lg px-3 py-2 text-sm" />
          </div>
          <div>
            <label class="block text-xs font-medium text-gray-600 mb-1">Interviewer</label>
            <select v-model="scheduleForm.interviewerId" required class="w-full border rounded-lg px-3 py-2 text-sm">
              <option value="">Select interviewer</option>
              <option v-for="u in teamMembers" :key="u.id" :value="u.id">{{ u.name }}</option>
            </select>
          </div>
          <input v-model="scheduleForm.location" placeholder="Location or Video link (optional)" class="w-full border rounded-lg px-3 py-2 text-sm" />
          <textarea v-model="scheduleForm.notes" placeholder="Notes (optional)" class="w-full border rounded-lg px-3 py-2 text-sm" rows="2"></textarea>
          <div class="flex gap-3 pt-2">
            <button type="submit" :disabled="scheduling" class="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm font-medium disabled:opacity-50">
              {{ scheduling ? 'Scheduling...' : 'Schedule' }}
            </button>
            <button type="button" @click="showSchedule = false" class="flex-1 border py-2 rounded-lg text-sm">Cancel</button>
          </div>
        </form>
      </div>
    </div>
    <div v-if="showFeedback" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div class="bg-white rounded-xl p-6 w-full max-w-md shadow-xl">
        <h3 class="text-lg font-bold mb-4">Submit Interview Feedback</h3>
        <form @submit.prevent="handleFeedback" class="space-y-3">
          <div>
            <label class="block text-xs font-medium text-gray-600 mb-2">Rating (1-5)</label>
            <div class="flex gap-2">
              <button v-for="n in 5" :key="n" type="button" @click="feedbackForm.rating = n"
                class="w-10 h-10 rounded-lg border text-sm font-medium transition-colors"
                :class="feedbackForm.rating === n ? 'bg-blue-600 text-white border-blue-600' : 'hover:bg-gray-50'">
                {{ n }}
              </button>
            </div>
          </div>
          <textarea v-model="feedbackForm.feedbackNotes" placeholder="Feedback notes" required class="w-full border rounded-lg px-3 py-2 text-sm" rows="4"></textarea>
          <div class="flex gap-3 pt-2">
            <button type="submit" :disabled="submittingFeedback" class="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm font-medium disabled:opacity-50">
              {{ submittingFeedback ? 'Submitting...' : 'Submit Feedback' }}
            </button>
            <button type="button" @click="showFeedback = false" class="flex-1 border py-2 rounded-lg text-sm">Cancel</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import * as signalR from '@microsoft/signalr'
import client from '../../api/client'
import type { Application, Interview } from '../../types'

const route = useRoute()
const jobId = route.params.id as string
const jobTitle = ref('')
const applications = ref<Application[]>([])
const interviews = ref<Interview[]>([])
const teamMembers = ref<{ id: string; name: string }[]>([])
const showCreate = ref(false)
const creating = ref(false)
const form = ref({ candidateName: '', candidateEmail: '', candidatePhone: '', coverNote: '' })
const showSchedule = ref(false)
const scheduling = ref(false)
const selectedAppId = ref('')
const scheduleForm = ref({ scheduledAt: '', interviewerId: '', location: '', notes: '' })
const showFeedback = ref(false)
const submittingFeedback = ref(false)
const selectedInterviewId = ref('')
const feedbackForm = ref({ rating: 0, feedbackNotes: '' })
const stages = ['Applied', 'Screened', 'Interview', 'Offer', 'Hired', 'Rejected']
const nextStageMap: Record<string, string[]> = {
  Applied: ['Screened'],
  Screened: ['Interview'],
  Interview: ['Offer'],
  Offer: ['Hired'],
}
let connection: signalR.HubConnection | null = null
function getNextStages(stage: string): string[] { return nextStageMap[stage] ?? [] }
function byStage(stage: string) { return applications.value.filter(a => a.stage === stage) }
function getInterview(appId: string) { return interviews.value.find(i => i.applicationId === appId) }
function formatDate(date: string) {
  return new Date(date).toLocaleString('en-GB', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' })
}
function openSchedule(appId: string) {
  selectedAppId.value = appId
  scheduleForm.value = { scheduledAt: '', interviewerId: '', location: '', notes: '' }
  showSchedule.value = true
}
function openFeedback(appId: string) {
  const interview = getInterview(appId)
  if (!interview) return
  selectedInterviewId.value = interview.id
  feedbackForm.value = { rating: 0, feedbackNotes: '' }
  showFeedback.value = true
}
async function loadAll() {
  const [appsRes, jobRes, interviewsRes] = await Promise.all([
    client.get<Application[]>(`/api/applications?jobId=${jobId}`),
    client.get(`/api/jobs/${jobId}`),
    client.get<Interview[]>(`/api/interviews`)
  ])
  applications.value = appsRes.data
  jobTitle.value = jobRes.data.title
  interviews.value = interviewsRes.data.filter((i: Interview) =>
    appsRes.data.some((a: Application) => a.id === i.applicationId)
  )
}
async function loadTeamMembers() {
  const res = await client.get<any[]>('/api/users')
  teamMembers.value = res.data.map((u: any) => ({ id: u.id, name: `${u.firstName} ${u.lastName}` }))
}
onMounted(async () => {
  await Promise.all([loadAll(), loadTeamMembers()])
 const baseUrl = import.meta.env.VITE_API_URL ?? ''
  const token = localStorage.getItem('token')
  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/pipeline`, { accessTokenFactory: () => token ?? '' })
    .withAutomaticReconnect()
    .build()
  connection.on('StageChanged', async () => { await loadAll() })
  try {
    await connection.start()
    await connection.invoke('JoinJob', jobId)
  } catch (e) {
    console.warn('SignalR connection failed:', e)
  }
})
onUnmounted(async () => {
  if (connection) {
    await connection.invoke('LeaveJob', jobId).catch(() => {})
    await connection.stop()
  }
})
async function moveStage(appId: string, toStage: string) {
  await client.put(`/api/applications/${appId}/stage`, { toStage, note: null })
  await loadAll()
}
async function handleCreate() {
  if (creating.value) return
  creating.value = true
  try {
    await client.post('/api/applications', { jobId, ...form.value })
    showCreate.value = false
    form.value = { candidateName: '', candidateEmail: '', candidatePhone: '', coverNote: '' }
    await loadAll()
  } finally { creating.value = false }
}
async function handleSchedule() {
  if (scheduling.value) return
  scheduling.value = true
  try {
    await client.post('/api/interviews', {
      applicationId: selectedAppId.value,
      interviewerId: scheduleForm.value.interviewerId,
      scheduledAt: new Date(scheduleForm.value.scheduledAt).toISOString(),
      location: scheduleForm.value.location || null,
      notes: scheduleForm.value.notes || null
    })
    showSchedule.value = false
    await loadAll()
  } finally { scheduling.value = false }
}
async function handleFeedback() {
  if (submittingFeedback.value || feedbackForm.value.rating === 0) return
  submittingFeedback.value = true
  try {
    await client.put(`/api/interviews/${selectedInterviewId.value}/feedback`, feedbackForm.value)
    showFeedback.value = false
    await loadAll()
  } finally { submittingFeedback.value = false }
}

async function exportCsv() {
  const token = localStorage.getItem('token')
  const url = `/api/export/applications?jobId=${jobId}`
  const res = await fetch(url, {
    headers: { Authorization: `Bearer ${token}` }
  })
  const blob = await res.blob()
  const link = document.createElement('a')
  link.href = URL.createObjectURL(blob)
  link.download = `applications-${jobId}-${new Date().toISOString().slice(0, 10)}.csv`
  link.click()
  URL.revokeObjectURL(link.href)
}
</script>
