<template>
  <AppLayout>
    <div style="margin-bottom:24px;">
      <h1 style="font-size:22px;font-weight:500;color:var(--ht-text);">Candidates</h1>
      <p style="font-size:12px;color:var(--ht-muted);margin-top:4px;">All candidates across your pipeline</p>
    </div>

    <div style="background:var(--ht-card);border:0.5px solid var(--ht-border);border-radius:10px;overflow:hidden;">
      <table style="width:100%;border-collapse:collapse;font-size:12px;">
        <thead>
          <tr style="border-bottom:0.5px solid var(--ht-border);background:var(--ht-page);">
            <th style="text-align:left;padding:10px 16px;font-weight:500;color:var(--ht-muted);">Candidate</th>
            <th style="text-align:left;padding:10px 16px;font-weight:500;color:var(--ht-muted);">Phone</th>
            <th style="text-align:left;padding:10px 16px;font-weight:500;color:var(--ht-muted);">Applications</th>
            <th style="text-align:left;padding:10px 16px;font-weight:500;color:var(--ht-muted);">Added</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="candidates.length === 0">
            <td colspan="4" style="text-align:center;padding:48px;color:var(--ht-muted);">No candidates yet</td>
          </tr>
          <tr v-for="c in candidates" :key="c.id" style="border-bottom:0.5px solid var(--ht-border);">
            <td style="padding:12px 16px;">
              <div style="display:flex;align-items:center;gap:10px;">
                <div style="width:32px;height:32px;border-radius:50%;background:rgba(44,62,80,0.08);display:flex;align-items:center;justify-content:center;font-size:11px;font-weight:500;color:#2C3E50;flex-shrink:0;">{{ initials(c.name) }}</div>
                <div>
                  <div style="font-weight:500;color:var(--ht-text);">{{ c.name }}</div>
                  <div style="color:var(--ht-muted);font-size:11px;">{{ c.email }}</div>
                </div>
              </div>
            </td>
            <td style="padding:12px 16px;color:var(--ht-muted);">{{ c.phone ?? '—' }}</td>
            <td style="padding:12px 16px;">
              <span style="background:rgba(176,141,87,0.1);color:#B08D57;font-size:11px;font-weight:500;padding:3px 8px;border-radius:20px;">{{ c.applicationCount }} job{{ c.applicationCount !== 1 ? 's' : '' }}</span>
            </td>
            <td style="padding:12px 16px;color:var(--ht-muted);">{{ formatDate(c.createdAt) }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </AppLayout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import AppLayout from '../../components/AppLayout.vue'
import client from '../../api/client'

interface Candidate { id: string; name: string; email: string; phone: string | null; resumeUrl: string | null; applicationCount: number; createdAt: string; }

const candidates = ref<Candidate[]>([])

onMounted(async () => {
  const res = await client.get<Candidate[]>('/api/candidates')
  candidates.value = res.data
})

function initials(name: string) {
  return name.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase()
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' })
}
</script>
