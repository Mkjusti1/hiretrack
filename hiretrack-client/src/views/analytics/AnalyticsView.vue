<template>
  <div class="min-h-screen bg-gray-50">
    <nav class="bg-white border-b px-6 py-4 flex justify-between items-center">
      <router-link to="/dashboard" class="text-xl font-bold text-blue-600">HireTrack</router-link>
      <div class="flex items-center gap-4">
        <router-link to="/jobs" class="text-sm text-gray-600 hover:text-blue-600">Jobs</router-link>
        <router-link to="/candidates" class="text-sm text-gray-600 hover:text-blue-600">Candidates</router-link>
        <router-link to="/analytics" class="text-sm text-blue-600 font-medium">Analytics</router-link>
      </div>
    </nav>

    <div class="max-w-6xl mx-auto px-6 py-10">
      <div class="flex justify-between items-center mb-8">
        <h2 class="text-2xl font-bold text-gray-900">Pipeline Analytics</h2>
        <select v-model="selectedJobId" @change="loadStats" class="border rounded-lg px-3 py-2 text-sm">
          <option value="">All Jobs</option>
          <option v-for="job in jobs" :key="job.id" :value="job.id">{{ job.title }}</option>
        </select>
      </div>

      <div v-if="stats" class="space-y-8">
        <!-- Summary cards -->
        <div class="grid grid-cols-5 gap-4">
          <div class="bg-white rounded-xl p-5 shadow-sm border text-center">
            <p class="text-sm text-gray-500 mb-1">Total</p>
            <p class="text-3xl font-bold text-gray-900">{{ stats.summary.total }}</p>
          </div>
          <div class="bg-white rounded-xl p-5 shadow-sm border text-center">
            <p class="text-sm text-gray-500 mb-1">Active</p>
            <p class="text-3xl font-bold text-blue-600">{{ stats.summary.active }}</p>
          </div>
          <div class="bg-white rounded-xl p-5 shadow-sm border text-center">
            <p class="text-sm text-gray-500 mb-1">Hired</p>
            <p class="text-3xl font-bold text-green-600">{{ stats.summary.hired }}</p>
          </div>
          <div class="bg-white rounded-xl p-5 shadow-sm border text-center">
            <p class="text-sm text-gray-500 mb-1">Rejected</p>
            <p class="text-3xl font-bold text-red-500">{{ stats.summary.rejected }}</p>
          </div>
          <div class="bg-white rounded-xl p-5 shadow-sm border text-center">
            <p class="text-sm text-gray-500 mb-1">Hire Rate</p>
            <p class="text-3xl font-bold text-purple-600">{{ stats.summary.hireRate }}%</p>
          </div>
        </div>

        <!-- Applications by stage -->
        <div class="bg-white rounded-xl p-6 shadow-sm border">
          <h3 class="font-semibold text-gray-800 mb-4">Applications by Stage</h3>
          <div class="space-y-3">
            <div v-for="item in stats.byStage" :key="item.stage" class="flex items-center gap-4">
              <div class="w-24 text-sm text-gray-600">{{ item.stage }}</div>
              <div class="flex-1 bg-gray-100 rounded-full h-6 relative">
                <div class="h-6 rounded-full flex items-center justify-end pr-2"
                  :style="{ width: barWidth(item.count) + '%', backgroundColor: stageColor(item.stage) }">
                  <span v-if="item.count > 0" class="text-white text-xs font-medium">{{ item.count }}</span>
                </div>
                <span v-if="item.count === 0" class="absolute left-2 top-1 text-xs text-gray-400">0</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Time in stage -->
        <div class="bg-white rounded-xl p-6 shadow-sm border">
          <h3 class="font-semibold text-gray-800 mb-4">Average Time in Stage</h3>
          <div class="grid grid-cols-3 gap-4">
            <div v-for="item in stats.stageTimings.filter(s => s.avgDays > 0)" :key="item.stage"
              class="bg-gray-50 rounded-lg p-4 border">
              <p class="text-sm text-gray-500">{{ item.stage }}</p>
              <p class="text-2xl font-bold text-gray-900 mt-1">{{ item.avgDays }}<span class="text-sm font-normal text-gray-400 ml-1">days</span></p>
              <p class="text-xs text-gray-400 mt-1">{{ item.sampleSize }} sample{{ item.sampleSize !== 1 ? 's' : '' }}</p>
            </div>
            <div v-if="stats.stageTimings.filter(s => s.avgDays > 0).length === 0"
              class="col-span-3 text-center py-8 text-gray-400 text-sm">
              Not enough data yet — stage timing data appears once candidates move between stages.
            </div>
          </div>
        </div>

        <!-- Applications over time -->
        <div class="bg-white rounded-xl p-6 shadow-sm border">
          <h3 class="font-semibold text-gray-800 mb-4">Applications — Last 30 Days</h3>
          <div v-if="stats.applicationsOverTime.length === 0" class="text-center py-8 text-gray-400 text-sm">
            No applications in the last 30 days.
          </div>
          <div v-else class="flex items-end gap-1 h-32">
            <div v-for="item in stats.applicationsOverTime" :key="item.date"
              class="flex-1 flex flex-col items-center gap-1">
              <span class="text-xs text-gray-500">{{ item.count }}</span>
              <div class="w-full bg-blue-500 rounded-t"
                :style="{ height: chartHeight(item.count) + 'px' }"></div>
              <span class="text-xs text-gray-400 rotate-45 origin-left" style="font-size:9px">{{ item.date }}</span>
            </div>
          </div>
        </div>
      </div>

      <div v-else class="text-center py-20 text-gray-400">Loading analytics...</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import client from '../../api/client'
import type { Job } from '../../types'

const jobs = ref<Job[]>([])
const selectedJobId = ref('')
const stats = ref<any>(null)

const stageColors: Record<string, string> = {
  Applied: '#3B82F6',
  Screened: '#8B5CF6',
  Interview: '#F59E0B',
  Offer: '#10B981',
  Hired: '#059669',
  Rejected: '#EF4444'
}

function stageColor(stage: string) {
  return stageColors[stage] ?? '#6B7280'
}

function barWidth(count: number) {
  if (!stats.value) return 0
  const max = Math.max(...stats.value.byStage.map((s: any) => s.count))
  if (max === 0) return 0
  return Math.max((count / max) * 100, count > 0 ? 5 : 0)
}

function chartHeight(count: number) {
  const max = Math.max(...stats.value.applicationsOverTime.map((s: any) => s.count))
  if (max === 0) return 0
  return Math.round((count / max) * 100)
}

async function loadStats() {
  const url = selectedJobId.value
    ? `/api/analytics/pipeline?jobId=${selectedJobId.value}`
    : '/api/analytics/pipeline'
  const res = await client.get(url)
  stats.value = res.data
}

onMounted(async () => {
  const res = await client.get<Job[]>('/api/jobs')
  jobs.value = res.data
  await loadStats()
})
</script>
