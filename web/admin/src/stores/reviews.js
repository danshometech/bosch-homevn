import { defineStore } from 'pinia'
import { ref } from 'vue'
import { get } from '@/services/api'

export const useReviewBadgeStore = defineStore('reviewBadge', () => {
  const pendingCount = ref(0)

  async function refresh() {
    try {
      pendingCount.value = (await get('/admin/reviews/pending-count')).count
    } catch {
    }
  }

  return { pendingCount, refresh }
})
