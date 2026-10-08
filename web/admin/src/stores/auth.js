import { defineStore } from 'pinia'
import { ref } from 'vue'
import { get, post } from '@/services/api'

// Phiên đăng nhập trang quản trị (cookie HttpOnly do Api đặt — JS không đọc được, chỉ hỏi /auth/me)
export const useAuthStore = defineStore('auth', () => {
  const user = ref(null)
  let checked = false

  async function ensure() {
    if (!checked) {
      try {
        user.value = await get('/admin/auth/me')
      } catch {
        user.value = null
      }
      checked = true
    }
    return user.value
  }

  async function login(userName, password, remember) {
    user.value = await post('/admin/auth/login', { userName, password, remember })
    checked = true
  }

  async function logout() {
    await post('/admin/auth/logout').catch(() => {})
    user.value = null
  }

  const expire = () => (user.value = null)

  return { user, ensure, login, logout, expire }
})
