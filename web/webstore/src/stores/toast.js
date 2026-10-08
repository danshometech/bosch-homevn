import { defineStore } from 'pinia'
import { ref } from 'vue'

const HOLD = 2600 // khớp với --toast-hold trong styles.css

export const useToastStore = defineStore('toast', () => {
  // { id, kind: 'cart' | 'info', text } — id đổi mỗi lần để chạy lại animation
  const current = ref(null)
  let seq = 0
  let timer

  function show(text, kind = 'cart') {
    current.value = { id: ++seq, kind, text }
    clearTimeout(timer)
    timer = setTimeout(() => (current.value = null), HOLD)
  }
  function hide() {
    clearTimeout(timer)
    current.value = null
  }

  return { current, show, hide }
})
