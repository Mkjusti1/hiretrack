<template>
  <AppLayout>
    <div style="margin-bottom:24px;">
      <div style="font-size:12px;color:var(--color-text-secondary);margin-bottom:4px;">Good to have you back</div>
      <h1 style="font-size:22px;font-weight:500;color:var(--color-text-primary);">{{ auth.user?.firstName }} {{ auth.user?.lastName }}</h1>
    </div>

    <div style="display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin-bottom:28px;">
      <div v-for="stat in stats" :key="stat.label" style="background:var(--color-background-primary);border:0.5px solid var(--color-border-tertiary);border-radius:10px;padding:16px;">
        <div style="font-size:11px;color:var(--color-text-secondary);margin-bottom:6px;">{{ stat.label }}</div>
        <div style="font-size:24px;font-weight:500;" :style="{ color: stat.color }">{{ stat.value }}</div>
      </div>
    </div>

    <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:14px;">
      <h2 style="font-size:14px;font-weight:500;color:var(--color-text-primary);">Active jobs</h2>
      <router-link to="/jobs" style="font-size:12px;color:#B08D57;text-decoration:none;">View all →</router-link>
    </div>

    <div style="display:flex;flex-direction:column;gap:8px;">
      <div v-for="job in openJobs" :key="job.id" style="background:var(--color-background-primary);border:0.5px solid var(--color-border-tertiary);border-radius:10px;padding:14px 18px;display:flex;justify-content:space-between;align-items:center;">
        <div>
          <div style="font-size:13px;font-weight:500;color:var(--color-text-primary);margin-bottom:3px;">{{ job.title }}</div>
          <div style="font-size:11px;color:var(--color-text-secondary);">{{ job.department }} · {{ job.location }}</div>
        </div>
        <div style="text-align:right;">
          <div style="font-size:20px;font-weight:500;color:#2C3E50;">{{ job.applicationCount }}</div>
          <div style="font-size:10px;color:var(--color-text-secondary);">applicants</div>
          <router-link :to="`/jobs/${job.id}/applications`" style="font-size:11px;color:#B08D57;text-decoration:none;display:block;margin-top:4px;">Pipeline →</router-link>
        </div>
      </div>
      <div v-if="openJobs.length === 0" style="background:var(--color-background-primary);border:0.5px solid var(--color-border-tertiary);border-radius:10px;padding:32px;text-align:center;color:var(--color-text-secondary);font-size:13px;">
        No open jobs yet. <router-link to="/jobs" style="color:#B08D57;">Create one →</router-link>
      </div>
    </div>
  </AppLayout>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import AppLayout from '../../components/AppLayout.vue'
import { useAuthStore } from '../../stores/auth'
import client from '../../api/client'
import type { Job } from '../../types'

const auth = useAuthStore()
const jobs = ref<Job[]>([])

const openJobs = computed(() => jobs.value.filter(j => j.status === 'Open'))

const stats = computed(() => [
  { label: 'Total Jobs', value: jobs.value.length, color: 'var(--color-text-primary)' },
  { label: 'Open Jobs', value: openJobs.value.length, color: '#2C3E50' },
  { label: 'Total Applications', value: jobs.value.reduce((s, j) => s + j.applicationCount, 0), color: '#B08D57' },
  { label: 'Hire Rate', value: '40%', color: '#3B6D11' },
])

onMounted(async () => {
  const res = await client.get<Job[]>('/api/jobs')
  jobs.value = res.data
})
</script>
