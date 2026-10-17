import { defineStore } from 'pinia'
import { ref } from 'vue'
import { get } from '@/services/api'

export const useOrderBadgeStore = defineStore('orderBadge', () => {
  const newCount = ref(0)

  async function refresh() {
    try {
      newCount.value = (await get('/admin/orders/new-count')).count
    } catch {
    }
  }

  return { newCount, refresh }
})
