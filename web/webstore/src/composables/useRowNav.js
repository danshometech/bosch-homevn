import { reactive, onMounted, onUnmounted } from 'vue'
import { reducedMotion } from '@/utils/motion'

// Hàng thẻ cuộn ngang có nút ‹ ›: biết đang ở đầu/cuối hàng để ẩn/khóa nút; `ratio` (phần đang thấy) và
// `progress` (0..1) cho thanh vị trí dưới hàng. `nudge`: lần đầu hàng hiện trên màn hình thì các thẻ nhích sang trái
// rồi về chỗ (class .nudge), báo hàng này cuộn ngang được.
// Gắn vào phần tử cuộn bằng :ref="nav.bind" và @scroll.passive="nav.sync".
export function useRowNav(step = 300, { nudge = false } = {}) {
  const nav = reactive({ atStart: true, atEnd: true, ratio: 1, progress: 0, bind, sync, scroll })
  let el = null
  let observer = null

  function bind(node) {
    el = node
  }
  function sync() {
    if (!el) return
    const max = el.scrollWidth - el.clientWidth
    nav.atStart = el.scrollLeft <= 2
    nav.atEnd = el.scrollLeft >= max - 2
    nav.ratio = el.scrollWidth ? Math.min(1, el.clientWidth / el.scrollWidth) : 1
    nav.progress = max > 0 ? Math.min(1, Math.max(0, el.scrollLeft / max)) : 0
  }
  function scroll(dir) {
    el?.scrollBy({ left: dir * step, behavior: reducedMotion() ? 'auto' : 'smooth' })
  }

  onMounted(() => {
    sync()
    window.addEventListener('resize', sync)
    if (!nudge || !el || reducedMotion()) return
    observer = new IntersectionObserver(([entry]) => {
      if (!entry.isIntersecting) return
      observer.disconnect()
      sync()
      if (nav.atEnd || !nav.atStart) return
      el.classList.add('nudge')
      const done = e => {
        if (e.target.parentElement !== el) return
        el.classList.remove('nudge')
        el.removeEventListener('animationend', done)
      }
      el.addEventListener('animationend', done)
    }, { threshold: 0.6 })
    observer.observe(el)
  })
  onUnmounted(() => {
    window.removeEventListener('resize', sync)
    observer?.disconnect()
  })

  return nav
}
