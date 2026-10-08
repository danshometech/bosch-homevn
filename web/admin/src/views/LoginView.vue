<script setup>
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Checkbox from 'primevue/checkbox'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const userName = ref('')
const password = ref('')
const remember = ref(false)
const error = ref('')
const busy = ref(false)

async function submit() {
  error.value = ''
  busy.value = true
  try {
    await auth.login(userName.value.trim(), password.value, remember.value)
    // chỉ quay lại đường dẫn nội bộ của trang quản trị
    const next = String(route.query.next || '')
    router.replace(next.startsWith('/') && !next.startsWith('//') ? next : { name: 'dashboard' })
  } catch (err) {
    error.value = err.message
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <div class="login">
    <form class="box" @submit.prevent="submit">
      <div>
        <h1>Quản trị bosch-homevn</h1>
        <p class="muted" style="margin:6px 0 0">Đăng nhập để quản lý sản phẩm và trang chủ.</p>
      </div>
      <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
      <div class="field">
        <label for="username">Tên đăng nhập</label>
        <InputText id="username" v-model="userName" autocomplete="username" required autofocus />
      </div>
      <div class="field">
        <label for="password">Mật khẩu</label>
        <Password v-model="password" input-id="password" :feedback="false" toggle-mask fluid autocomplete="current-password" required />
      </div>
      <label class="check"><Checkbox v-model="remember" binary />Ghi nhớ đăng nhập</label>
      <Button type="submit" label="Đăng nhập" icon="pi pi-sign-in" :loading="busy" />
    </form>
  </div>
</template>
