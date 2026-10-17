import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/services/api'

// Cài đặt admin chỉnh (GET /api/settings/contact). zaloUrl / facebookUrl / tikTokUrl / youTubeUrl null = chưa cấu hình, ẩn nút tương ứng
export const useSettingsStore = defineStore('settings', () => {
  const hotline = ref('1900 6868')
  const zaloUrl = ref(null)
  const facebookUrl = ref(null)
  const tikTokUrl = ref(null)
  const youTubeUrl = ref(null)
  const tel = computed(() => 'tel:' + hotline.value.replace(/[^\d+]/g, ''))
  let pending = null

  function load() {
    pending ||= api('/settings/contact')
      .then(s => {
        hotline.value = s.hotline || hotline.value
        zaloUrl.value = s.zaloUrl || null
        facebookUrl.value = s.facebookUrl || null
        tikTokUrl.value = s.tikTokUrl || null
        youTubeUrl.value = s.youTubeUrl || null
      })
      .catch(err => {
        console.error(err)
        pending = null
      })
    return pending
  }

  return { hotline, zaloUrl, facebookUrl, tikTokUrl, youTubeUrl, tel, load }
})
