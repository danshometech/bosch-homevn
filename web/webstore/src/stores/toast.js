import { defineStore } from 'pinia'
import { ref } from 'vue'

const HOLD = 1500 // khớp với --toast-hold trong styles.css

export const useToastStore = defineStore('toast', () => {
  // { id, kind: 'cart' | 'info', text, product?, qty? } — id đổi mỗi lần để chạy lại animation.
  // paused: đang rê chuột / focus vào toast thì dừng đếm giờ, rời ra chạy tiếp phần còn lại
  const current = ref(null)
  const paused = ref(false)
  let seq = 0
  let timer
  let endsAt = 0
  let left = HOLD

  function start(ms) {
    clearTimeout(timer)
    endsAt = Date.now() + ms
    timer = setTimeout(hide, ms)
  }
  function open(toast) {
    current.value = { id: ++seq, ...toast }
    paused.value = false
    start(HOLD)
  }
  const show = (text, kind = 'info') => open({ kind, text })
  const added = (product, qty = 1) => open({ kind: 'cart', text: product?.name ?? 'Sản phẩm', product, qty })
  // on: true = vừa thích, false = vừa bỏ thích (toast có nút hoàn tác)
  const liked = (product, on) => open({ kind: 'wish', text: product?.name ?? 'Sản phẩm', product, on })

  function pause() {
    if (!current.value || paused.value) return
    clearTimeout(timer)
    left = Math.max(0, endsAt - Date.now())
    paused.value = true
  }
  function resume() {
    if (!paused.value) return
    paused.value = false
    start(left)
  }
  function hide() {
    clearTimeout(timer)
    current.value = null
    paused.value = false
  }

  return { current, paused, show, added, liked, pause, resume, hide }
})
