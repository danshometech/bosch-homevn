import { api } from '@/services/api'
import { getProvinces } from '@/services/address'

// Cách giao hàng / thanh toán đang bật (admin: "Giao hàng & thanh toán"). Bản lần trước lưu ở trình duyệt để hiện ngay,
// vẫn tải lại bản mới vì admin có thể đổi phí; trong 60 giây dùng chung một lần tải
const KEY = 'bh_checkout_opts'
const FRESH_MS = 60_000
let job = null
let jobAt = 0

export function savedCheckoutOptions() {
  try {
    const s = JSON.parse(localStorage.getItem(KEY))
    return Array.isArray(s?.shipping) && Array.isArray(s?.payment) ? s : null
  } catch {
    return null
  }
}

export function getCheckoutOptions() {
  if (!job || Date.now() - jobAt > FRESH_MS) {
    jobAt = Date.now()
    job = api('/checkout/options').then(o => {
      try { localStorage.setItem(KEY, JSON.stringify(o)) } catch { /* private mode / storage đầy */ }
      return o
    })
    job.catch(() => (job = null))
  }
  return job
}

// Gọi từ giỏ hàng: tải trước dữ liệu + code trang thanh toán để bấm "Thanh toán" là hiện ngay
export function prefetchCheckout() {
  const quiet = p => p.catch(() => {})
  quiet(getCheckoutOptions())
  quiet(getProvinces())
  quiet(import('@/views/CheckoutView.vue'))
}
