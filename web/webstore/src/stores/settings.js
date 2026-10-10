import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/services/api'

// Cài đặt admin chỉnh (GET /api/settings/contact). zaloUrl null = chưa cấu hình, ẩn nút Zalo
export const useSettingsStore = defineStore('settings', () => {
  const hotline = ref('1900 6868')
  const zaloUrl = ref(null)
  const tel = computed(() => 'tel:' + hotline.value.replace(/[^\d+]/g, ''))
  let pending = null

  function load() {
    pending ||= api('/settings/contact')
      .then(s => {
        hotline.value = s.hotline || hotline.value
        zaloUrl.value = s.zaloUrl || null
      })
      .catch(err => {
        console.error(err)
        pending = null
      })
    return pending
  }

  return { hotline, zaloUrl, tel, load }
})
