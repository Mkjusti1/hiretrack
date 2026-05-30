<template>
  <div style="min-height:100vh;background:#2C3E50;display:flex;align-items:center;justify-content:center;">
    <div style="width:100%;max-width:440px;padding:0 16px;">
      <div style="text-align:center;margin-bottom:32px;">
        <div style="font-size:28px;font-weight:500;color:#fff;letter-spacing:0.02em;">Hire<span style="color:#B08D57;">Track</span></div>
        <div style="font-size:13px;color:rgba(255,255,255,0.45);margin-top:6px;">Create your company workspace</div>
      </div>
      <div style="background:var(--color-background-primary);border-radius:12px;padding:28px;border:0.5px solid rgba(255,255,255,0.08);">
        <div v-if="error" style="background:#FCEBEB;color:#791F1F;padding:10px 14px;border-radius:8px;font-size:12px;margin-bottom:16px;">{{ error }}</div>
        <div style="display:flex;flex-direction:column;gap:12px;">
          <div>
            <label style="font-size:11px;font-weight:500;color:var(--color-text-secondary);display:block;margin-bottom:5px;">Company name</label>
            <input v-model="form.companyName" type="text" required style="width:100%;border:0.5px solid var(--color-border-tertiary);border-radius:8px;padding:9px 12px;font-size:13px;background:var(--color-background-secondary);color:var(--color-text-primary);" />
          </div>
          <div style="display:grid;grid-template-columns:1fr 1fr;gap:10px;">
            <div>
              <label style="font-size:11px;font-weight:500;color:var(--color-text-secondary);display:block;margin-bottom:5px;">First name</label>
              <input v-model="form.firstName" type="text" required style="width:100%;border:0.5px solid var(--color-border-tertiary);border-radius:8px;padding:9px 12px;font-size:13px;background:var(--color-background-secondary);color:var(--color-text-primary);" />
            </div>
            <div>
              <label style="font-size:11px;font-weight:500;color:var(--color-text-secondary);display:block;margin-bottom:5px;">Last name</label>
              <input v-model="form.lastName" type="text" required style="width:100%;border:0.5px solid var(--color-border-tertiary);border-radius:8px;padding:9px 12px;font-size:13px;background:var(--color-background-secondary);color:var(--color-text-primary);" />
            </div>
          </div>
          <div>
            <label style="font-size:11px;font-weight:500;color:var(--color-text-secondary);display:block;margin-bottom:5px;">Email address</label>
            <input v-model="form.email" type="email" required style="width:100%;border:0.5px solid var(--color-border-tertiary);border-radius:8px;padding:9px 12px;font-size:13px;background:var(--color-background-secondary);color:var(--color-text-primary);" />
          </div>
          <div>
            <label style="font-size:11px;font-weight:500;color:var(--color-text-secondary);display:block;margin-bottom:5px;">Password</label>
            <input v-model="form.password" type="password" required style="width:100%;border:0.5px solid var(--color-border-tertiary);border-radius:8px;padding:9px 12px;font-size:13px;background:var(--color-background-secondary);color:var(--color-text-primary);" />
          </div>
          <button @click="handleRegister" :disabled="loading" style="width:100%;background:#B08D57;color:#fff;border:none;padding:11px;border-radius:8px;font-size:13px;font-weight:500;cursor:pointer;margin-top:4px;">
            {{ loading ? 'Creating account...' : 'Create account' }}
          </button>
        </div>
        <p style="text-align:center;font-size:12px;color:var(--color-text-secondary);margin-top:16px;">
          Already have an account? <router-link to="/login" style="color:#B08D57;text-decoration:none;">Sign in</router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'

const auth = useAuthStore()
const router = useRouter()
const loading = ref(false)
const error = ref('')
const form = ref({ companyName: '', firstName: '', lastName: '', email: '', password: '' })

async function handleRegister() {
  loading.value = true
  error.value = ''
  try {
    await auth.register(form.value)
    router.push('/dashboard')
  } catch (e: any) {
    error.value = e.response?.data?.message ?? 'Registration failed'
  } finally { loading.value = false }
}
</script>
