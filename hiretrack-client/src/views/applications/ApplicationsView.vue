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

function getNextStages(stage: string): string[] {
  return nextStageMap[stage] ?? []
}

function byStage(stage: string) {
  return applications.value.filter(a => a.stage === stage)
}

function getInterview(appId: string) {
  return interviews.value.find(i => i.applicationId === appId)
}

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

  const baseUrl = 'https://automatic-space-happiness-69pvxj5j6jj93559-5140.app.github.dev'
  const token = localStorage.getItem('token')

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/pipeline`, {
      accessTokenFactory: () => token ?? ''
    })
    .withAutomaticReconnect()
    .build()

  connection.on('StageChanged', async () => {
    await loadAll()
  })

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
  } finally {
    creating.value = false
  }
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
  } finally {
    scheduling.value = false
  }
}

async function handleFeedback() {
  if (submittingFeedback.value || feedbackForm.value.rating === 0) return
  submittingFeedback.value = true
  try {
    await client.put(`/api/interviews/${selectedInterviewId.value}/feedback`, feedbackForm.value)
    showFeedback.value = false
    await loadAll()
  } finally {
    submittingFeedback.value = false
  }
}
</script>