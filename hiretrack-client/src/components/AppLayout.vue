<template>
  <div style="display:flex;min-height:100vh;background:var(--ht-page);">
    <aside style="width:220px;background:#2C3E50;display:flex;flex-direction:column;position:fixed;top:0;left:0;height:100vh;z-index:10;">
      <div style="padding:20px 20px 16px;border-bottom:1px solid rgba(255,255,255,0.08);">
        <span style="font-size:18px;font-weight:500;color:#fff;letter-spacing:0.02em;">Hire<span style="color:#B08D57;">Track</span></span>
      </div>
      <div style="padding:8px 0;flex:1;overflow-y:auto;">
        <div style="padding:12px 20px 4px;font-size:10px;font-weight:500;color:rgba(255,255,255,0.25);letter-spacing:0.08em;text-transform:uppercase;">Main</div>
        <router-link to="/dashboard" custom v-slot="{ isActive, navigate }">
          <div @click="navigate" :style="navStyle(isActive)">
            <i class="ti ti-layout-dashboard" aria-hidden="true" style="font-size:16px;"></i> Dashboard
          </div>
        </router-link>
        <router-link to="/jobs" custom v-slot="{ isActive, navigate }">
          <div @click="navigate" :style="navStyle(isActive)">
            <i class="ti ti-briefcase" aria-hidden="true" style="font-size:16px;"></i> Jobs
          </div>
        </router-link>
        <router-link to="/candidates" custom v-slot="{ isActive, navigate }">
          <div @click="navigate" :style="navStyle(isActive)">
            <i class="ti ti-users" aria-hidden="true" style="font-size:16px;"></i> Candidates
          </div>
        </router-link>
        <div style="padding:12px 20px 4px;font-size:10px;font-weight:500;color:rgba(255,255,255,0.25);letter-spacing:0.08em;text-transform:uppercase;">Insights</div>
        <router-link to="/analytics" custom v-slot="{ isActive, navigate }">
          <div @click="navigate" :style="navStyle(isActive)">
            <i class="ti ti-chart-bar" aria-hidden="true" style="font-size:16px;"></i> Analytics
          </div>
        </router-link>
      </div>
      <div style="padding:12px 0;border-top:1px solid rgba(255,255,255,0.08);">
        <div style="display:flex;align-items:center;gap:10px;padding:10px 20px;">
          <div style="width:32px;height:32px;border-radius:50%;background:rgba(176,141,87,0.15);display:flex;align-items:center;justify-content:center;font-size:12px;font-weight:500;color:#B08D57;">{{ initials }}</div>
          <div style="flex:1;min-width:0;">
            <div style="font-size:12px;font-weight:500;color:#fff;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">{{ auth.user?.firstName }} {{ auth.user?.lastName }}</div>
            <div style="font-size:11px;color:rgba(255,255,255,0.4);">{{ auth.user?.role }}</div>
          </div>
          <button @click="handleLogout" style="background:none;border:none;cursor:pointer;color:rgba(255,255,255,0.3);padding:4px;" title="Logout">
            <i class="ti ti-logout" aria-hidden="true" style="font-size:16px;"></i>
          </button>
        </div>
      </div>
    </aside>
    <main style="flex:1;margin-left:220px;padding:28px 28px 40px;">
      <slot />
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()
const router = useRouter()

const initials = computed(() => {
  const f = auth.user?.firstName?.[0] ?? ''
  const l = auth.user?.lastName?.[0] ?? ''
  return (f + l).toUpperCase()
})

function navStyle(isActive: boolean) {
  return {
    display: 'flex',
    alignItems: 'center',
    gap: '10px',
    padding: '9px 20px',
    fontSize: '13px',
    color: isActive ? '#B08D57' : 'rgba(255,255,255,0.55)',
    background: isActive ? 'rgba(176,141,87,0.1)' : 'transparent',
    borderLeft: isActive ? '2px solid #B08D57' : '2px solid transparent',
    cursor: 'pointer',
    transition: 'all 0.15s',
  }
}

function handleLogout() {
  auth.logout()
  router.push('/login')
}
</script>
