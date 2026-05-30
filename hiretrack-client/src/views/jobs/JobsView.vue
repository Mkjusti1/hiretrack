<template>
  <AppLayout>
    <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:24px;">
      <h1 style="font-size:22px;font-weight:500;color:var(--ht-text);">Jobs</h1>
      <div style="display:flex;gap:8px;">
        <button @click="exportAll" style="background:var(--ht-card);border:0.5px solid var(--ht-border);color:var(--ht-text);padding:8px 14px;border-radius:8px;font-size:12px;cursor:pointer;display:flex;align-items:center;gap:6px;">
          <i class="ti ti-download" aria-hidden="true"></i> Export
        </button>
        <button @click="showCreate = true" style="background:#2C3E50;color:#B08D57;border:none;padding:8px 16px;border-radius:8px;font-size:12px;font-weight:500;cursor:pointer;display:flex;align-items:center;gap:6px;">
          <i class="ti ti-plus" aria-hidden="true"></i> New Job
        </button>
      </div>
    </div>

    <div style="display:flex;flex-direction:column;gap:8px;">
      <div v-for="job in jobs" :key="job.id" style="background:var(--ht-card);border:0.5px solid var(--ht-border);border-radius:10px;padding:16px 20px;display:flex;justify-content:space-between;align-items:center;">
        <div>
          <div style="font-size:13px;font-weight:500;color:var(--ht-text);margin-bottom:4px;">{{ job.title }}</div>
          <div style="font-size:11px;color:var(--ht-muted);margin-bottom:8px;">{{ job.department }} · {{ job.location }}</div>
          <span :style="job.status === 'Open' ? openBadge : archivedBadge">{{ job.status }}</span>
        </div>
        <div style="text-align:right;">
          <div style="font-size:22px;font-weight:500;color:#2C3E50;">{{ job.applicationCount }}</div>
          <div style="font-size:10px;color:var(--ht-muted);margin-bottom:8px;">applicants</div>
          <router-link :to="`/jobs/${job.id}/applications`" style="font-size:11px;color:#B08D57;text-decoration:none;display:block;margin-bottom:4px;">View pipeline →</router-link>
          <div style="display:flex;gap:8px;justify-content:flex-end;">
            <button v-if="job.status === 'Open'" @click="archiveJob(job.id)" style="font-size:11px;color:var(--ht-muted);background:none;border:none;cursor:pointer;padding:0;">Archive</button>
            <button v-if="job.status === 'Archived'" @click="unarchiveJob(job.id)" style="font-size:11px;color:#3B6D11;background:none;border:none;cursor:pointer;padding:0;">Unarchive</button>
            <button @click="deleteJob(job.id)" style="font-size:11px;color:#A32D2D;background:none;border:none;cursor:pointer;padding:0;">Delete</button>
          </div>
        </div>
      </div>
      <div v-if="jobs.length === 0" style="background:var(--ht-card);border:0.5px solid var(--ht-border);border-radius:10px;padding:48px;text-align:center;color:var(--ht-muted);font-size:13px;">
        No jobs yet. Create your first job posting.
      </div>
    </div>

    <div v-if="showCreate" style="position:fixed;inset:0;background:rgba(0,0,0,0.5);display:flex;align-items:center;justify-content:center;z-index:50;">
      <div style="background:var(--ht-card);border-radius:12px;padding:24px;width:100%;max-width:440px;border:0.5px solid var(--ht-border);">
        <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:20px;">
          <h3 style="font-size:15px;font-weight:500;color:var(--ht-text);">Create new job</h3>
          <button @click="showCreate = false" style="background:none;border:none;cursor:pointer;color:var(--ht-muted);font-size:18px;">×</button>
        </div>
        <div style="display:flex;flex-direction:column;gap:12px;">
          <input v-model="form.title" placeholder="Job title" style="width:100%;border:0.5px solid var(--ht-border);border-radius:8px;padding:9px 12px;font-size:13px;background:var(--ht-page);color:var(--ht-text);" />
          <input v-model="form.department" placeholder="Department" style="width:100%;border:0.5px solid var(--ht-border);border-radius:8px;padding:9px 12px;font-size:13px;background:var(--ht-page);color:var(--ht-text);" />
          <input v-model="form.location" placeholder="Location" style="width:100%;border:0.5px solid var(--ht-border);border-radius:8px;padding:9px 12px;font-size:13px;background:var(--ht-page);color:var(--ht-text);" />
          <textarea v-model="form.description" placeholder="Description (optional)" rows="3" style="width:100%;border:0.5px solid var(--ht-border);border-radius:8px;padding:9px 12px;font-size:13px;background:var(--ht-page);color:var(--ht-text);resize:none;"></textarea>
          <div style="display:flex;gap:8px;margin-top:4px;">
            <button @click="handleCreate" :disabled="creating" style="flex:1;background:#2C3E50;color:#B08D57;border:none;padding:10px;border-radius:8px;font-size:13px;font-weight:500;cursor:pointer;opacity:creating?0.6:1;">
              {{ creating ? 'Creating...' : 'Create job' }}
            </button>
            <button @click="showCreate = false" style="flex:1;background:var(--ht-page);border:0.5px solid var(--ht-border);color:var(--ht-text);padding:10px;border-radius:8px;font-size:13px;cursor:pointer;">Cancel</button>
          </div>
        </div>
      </div>
    </div>
  </AppLayout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import AppLayout from '../../components/AppLayout.vue'
import client from '../../api/client'
import type { Job } from '../../types'

const jobs = ref<Job[]>([])
const showCreate = ref(false)
const creating = ref(false)
const form = ref({ title: '', department: '', location: '', description: '' })

const openBadge = { display:'inline-block', fontSize:'11px', padding:'3px 8px', borderRadius:'20px', background:'#eaf3de', color:'#3B6D11', fontWeight:'500' }
const archivedBadge = { display:'inline-block', fontSize:'11px', padding:'3px 8px', borderRadius:'20px', background:'var(--color-background-secondary)', color:'var(--color-text-secondary)', fontWeight:'500' }

onMounted(loadJobs)

async function loadJobs() {
  const res = await client.get<Job[]>('/api/jobs')
  jobs.value = res.data
}

async function handleCreate() {
  if (creating.value) return
  creating.value = true
  try {
    await client.post('/api/jobs', form.value)
    showCreate.value = false
    form.value = { title: '', department: '', location: '', description: '' }
    await loadJobs()
  } finally { creating.value = false }
}

async function archiveJob(id: string) {
  await client.delete(`/api/jobs/${id}`)
  await loadJobs()
}

async function unarchiveJob(id: string) {
  await client.put(`/api/jobs/${id}/unarchive`)
  await loadJobs()
}

async function deleteJob(id: string) {
  if (!confirm('Permanently delete this job?')) return
  await client.delete(`/api/jobs/${id}/permanent`)
  await loadJobs()
}

async function exportAll() {
  const token = localStorage.getItem('token')
  const res = await fetch(`${import.meta.env.VITE_API_URL ?? ''}/api/export/applications`, { headers: { Authorization: `Bearer ${token}` } })
  const blob = await res.blob()
  const link = document.createElement('a')
  link.href = URL.createObjectURL(blob)
  link.download = `all-applications-${new Date().toISOString().slice(0, 10)}.csv`
  link.click()
  URL.revokeObjectURL(link.href)
}
</script>
