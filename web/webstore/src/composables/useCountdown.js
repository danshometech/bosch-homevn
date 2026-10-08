import { ref, computed, onMounted, onUnmounted } from 'vue'

// Đếm ngược đến 23:59:59 hôm nay, trả về ['hh', 'mm', 'ss']
export function useCountdown() {
  const end = new Date()
  end.setHours(23, 59, 59, 0)
  const now = ref(Date.now())
  let timer

  onMounted(() => { timer = setInterval(() => (now.value = Date.now()), 1000) })
  onUnmounted(() => clearInterval(timer))

  return computed(() => {
    const s = Math.max(0, Math.floor((end - now.value) / 1000))
    return [Math.floor(s / 3600), Math.floor(s / 60) % 60, s % 60].map(x => String(x).padStart(2, '0'))
  })
}
