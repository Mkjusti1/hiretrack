<template>
  <AppLayout>
    <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:24px;">
      <div>
        <h1 style="font-size:22px;font-weight:500;color:var(--ht-text);">Analytics</h1>
        <p style="font-size:12px;color:var(--ht-muted);margin-top:4px;">Pipeline performance overview</p>
      </div>
      <select v-model="selectedJobId" @change="loadStats" style="border:0.5px solid var(--ht-border);border-radius:8px;padding:8px 12px;font-size:12px;background:var(--ht-card);color:var(--ht-text);">
        <option value="">All Jobs</option>
        <option v-for="job in jobs" :key="job.id" :value="job.id">{{ job.title }}</option>
      </select>
    </div>

    <div v-if="stats">
      <div style="display:grid;grid-template-columns:repeat(5,1fr);gap:10px;margin-bottom:24px;">
        <div style="background:var(--ht-card);border:0.5px solid var(--ht-border);border-radius:10px;padding:16px;">
          <div style="font-size:11px;color:var(--ht-muted);margin-bottom:6px;">Total</div>
          <div style="font-size:24px;font-weight:500;color:var(--ht-text);">{{ stats.summary.total }}</div>
        </div>
        <div style="background:var(--ht-card);border:0.5px solid var(--ht-border);border-radius:10px;padding:16px;">
          <div style="font-size:11px;color:var(--ht-muted);margin-bottom:6px;">Active</div>
          <div style="font-size:24px;font-weight:500;color:#2C3E50;">{{ stats.summary.active }}</div>
        </div>
        <div style="background:var(--ht-card);border:0.5px solid var(--ht-border);border-radius:10px;padding:16px;">
          <div style="font-size:11px;color:var(--ht-muted);margin-bottom:6px;">Hired</div>
          <div style="font-size:24px;font-weight:500;color:#3B6D11;">{{ stats.summary.hired }}</div>
        </div>
        <div style="background:var(--ht-card);border:0.5px solid var(--ht-border);border-radius:10px;padding:16px;">
          <div style="font-size:11px;color:var(--ht-muted);margin-bottom:6px;">Rejected</div>
          <div style="font-size:24px;font-weight:500;color:#A32D2D;">{{ stats.summary.rejected }}</div>
        </div>
        <div style="background:#2C3E50;border:0.5px solid transparent;border-radius:10px;padding:16px;">
          <div style="font-size:11px;color:rgba(255,255,255,0.5);margin-bottom:6px;">Hire Rate</div>
          <div style="font-size:24px;font-weight:500;color:#B08D57;">{{ stats.summary.hireRate }}%</div>
        </div>
      </div>

      <div style="display:grid;grid-template-columns:1fr 1fr;gap:16px;margin-bottom:16px;">
        <div style="background:var(--ht-card);border:0.5px solid var(--ht-border);border-radius:10px;padding:20px;">
          <h3 style="font-size:13px;font-weight:500;color:var(--ht-text);margin-bottom:16px;">Applications by stage</h3>
          <div style="display:flex;flex-direction:column;gap:10px;">
            <div v-for="item in stats.byStage" :key="item.stage" style="display:flex;align-items:center;gap:10px;">
              <div style="width:80px;font-size:11px;color:var(--ht-muted);">{{ item.stage }}</div>
              <div style="flex:1;background:var(--ht-page);border-radius:4px;height:20px;position:relative;overflow:hidden;">
                <div :style="{ width: barWidth(item.count) + '%', height: '100%', background: stageColor(item.stage), borderRadius: '4px', display: 'flex', alignItems: 'center', justifyContent: 'flex-end', paddingRight: '6px' }">
                  <span v-if="item.count > 0" style="font-size:10px;color:#fff;font-weight:500;">{{ item.count }}</span>
                </div>
              </div>
              <div style="width:20px;font-size:11px;color:var(--ht-muted);text-align:right;">{{ item.count }}</div>
            </div>
          </div>
        </div>

        <div style="background:var(--ht-card);border:0.5px solid var(--ht-border);border-radius:10px;padding:20px;">
          <h3 style="font-size:13px;font-weight:500;color:var(--ht-text);margin-bottom:16px;">Avg time in stage</h3>
          <div v-if="stats.stageTimings.filter((s) => s.avgDays > 0).length === 0" style="text-align:center;padding:24px;color:var(--ht-muted);font-size:12px;">
            Not enough data yet
          </div>
          <div v-else style="display:grid;grid-template-columns:1fr 1fr;gap:10px;">
            <div v-for="item in stats.stageTimings.filter((s) => s.avgDays > 0)" :key="item.stage" style="background:var(--ht-page);border-radius:8px;padding:12px;">
              <div style="font-size:11px;color:var(--ht-muted);margin-bottom:4px;">{{ item.stage }}</div>
              <div style="font-size:18px;font-weight:500;color:var(--ht-text);">{{ item.avgDays }}<span style="font-size:11px;font-weight:400;color:var(--ht-muted);margin-left:3px;">days</span></div>
              <div style="font-size:10px;color:var(--ht-muted);margin-top:2px;">{{ item.sampleSize }} sample{{ item.sampleSize !== 1 ? 's' : '' }}</div>
            </div>
          </div>
        </div>
      </div>

      <div style="background:var(--ht-card);border:0.5px solid var(--ht-border);border-radius:10px;padding:20px;">
        <h3 style="font-size:13px;font-weight:500;color:var(--ht-text);margin-bottom:16px;">Applications — last 30 days</h3>
        <div v-if="stats.applicationsOverTime.length === 0" style="text-align:center;padding:24px;color:var(--ht-muted);font-size:12px;">No applications in this period</div>
        <div v-else style="display:flex;align-items:flex-end;gap:4px;height:100px;">
          <div v-for="item in stats.applicationsOverTime" :key="item.date" style="flex:1;display:flex;flex-direction:column;align-items:center;gap:4px;">
            <span style="font-size:10px;color:var(--ht-muted);">{{ item.count }}</span>
            <div :style="{ width: '100%', background: '#2C3E50', borderRadius: '4px 4px 0 0', height: chartHeight(item.count) + 'px' }"></div>
            <span style="font-size:9px;color:var(--ht-muted);transform:rotate(-45deg);transform-origin:top left;white-space:nowrap;margin-top:4px;">{{ item.date }}</span>
          </div>
        </div>
      </div>
    </div>

    <div v-else style="text-align:center;padding:80px;color:var(--ht-muted);font-size:13px;">Loading analytics...</div>
  </AppLayout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import AppLayout from '../../components/AppLayout.vue'
import client from '../../api/client'
import type { Job } from '../../types'

const jobs = ref<Job[]>([])
const selectedJobId = ref('')
const stats = ref<any>(null)

const stageColors: Record<string, string> = {
  Applied: '#2C3E50', Screened: '#B08D57', Interview: '#B08D57',
  Offer: '#3B6D11', Hired: '#3B6D11', Rejected: '#A32D2D'
}

function stageColor(stage: string) { return stageColors[stage] ?? '#888' }

function barWidth(count: number) {
  if (!stats.value) return 0
  const max = Math.max(...stats.value.byStage.map((s: any) => s.count))
  if (max === 0) return 0
  return Math.max((count / max) * 100, count > 0 ? 5 : 0)
}

function chartHeight(count: number) {
  const max = Math.max(...stats.value.applicationsOverTime.map((s: any) => s.count))
  if (max === 0) return 0
  return Math.round((count / max) * 80)
}

async function loadStats() {
  const url = selectedJobId.value ? `/api/analytics/pipeline?jobId=${selectedJobId.value}` : '/api/analytics/pipeline'
  const res = await client.get(url)
  stats.value = res.data
}

onMounted(async () => {
  const res = await client.get<Job[]>('/api/jobs')
  jobs.value = res.data
  await loadStats()
})
</script>
