import { defineStore } from 'pinia'
import { ref } from 'vue'
import { get } from '@/services/api'

// Số đơn mới (chưa xử lý) hiện trên menu "Đơn hàng"; layout tải định kỳ, trang đơn hàng gọi refresh() sau khi đổi trạng thái
export const useOrderBadgeStore = defineStore('orderBadge', () => {
  const newCount = ref(0)

  async function refresh() {
    try {
      newCount.value = (await get('/admin/orders/new-count')).count
    } catch {
      /* lỗi mạng / hết phiên: giữ số cũ */
    }
  }

  return { newCount, refresh }
})
