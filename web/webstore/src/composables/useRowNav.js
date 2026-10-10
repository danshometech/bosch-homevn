import { reactive, onMounted, onUnmounted } from 'vue'
import { reducedMotion } from '@/utils/motion'

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
    const fits = max <= parseFloat(getComputedStyle(el).paddingRight) + 2
    nav.atStart = fits || el.scrollLeft <= 2
    nav.atEnd = fits || el.scrollLeft >= max - 2
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
