import { ref, watch, toValue, onUnmounted } from 'vue'
import { reducedMotion } from '@/utils/motion'

// Giá trị hiển thị chạy dần tới giá trị mới (ease-out cubic), làm tròn nghìn đồng trong lúc chạy
export function useTween(source, duration = 420) {
  const shown = ref(toValue(source))
  let raf

  watch(() => toValue(source), to => {
    cancelAnimationFrame(raf)
    const from = shown.value
    if (from === to || reducedMotion()) {
      shown.value = to
      return
    }
    const t0 = performance.now()
    const frame = now => {
      const k = Math.min(1, (now - t0) / duration)
      const e = 1 - (1 - k) ** 3
      shown.value = k < 1 ? Math.round((from + (to - from) * e) / 1000) * 1000 : to
      if (k < 1) raf = requestAnimationFrame(frame)
    }
    raf = requestAnimationFrame(frame)
  })
  onUnmounted(() => cancelAnimationFrame(raf))

  return shown
}
