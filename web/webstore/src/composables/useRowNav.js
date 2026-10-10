import { reactive, onMounted, onUnmounted } from 'vue'
import { reducedMotion } from '@/utils/motion'

export function useRowNav(step = 300, { nudge = false } = {}) {
  const nav = reactive({ atStart: true, atEnd: true, ratio: 1, progress: 0, bind, sync, scroll })
  let el = null
  let observer = null
  let target = null
  let wheelTimer = 0

  function bind(node) {
    if (el === node) return
    el?.removeEventListener('wheel', onWheel)
    el = node
    el?.addEventListener('wheel', onWheel, { passive: false })
  }
  // Con lăn dọc trên hàng → cuộn ngang (tạm tắt snap, ngừng lăn thì bật lại để căn vào thẻ gần nhất).
  // Tới đầu / cuối hàng thì để trang cuộn dọc tiếp; lăn ngang (touchpad, Shift) hay Ctrl + lăn (zoom) để trình duyệt xử lý
  function onWheel(e) {
    if (!el || e.ctrlKey || e.shiftKey || Math.abs(e.deltaX) >= Math.abs(e.deltaY) || (nav.atStart && nav.atEnd)) return
    const max = el.scrollWidth - el.clientWidth
    const dy = e.deltaY * (e.deltaMode === 1 ? 40 : e.deltaMode === 2 ? el.clientWidth : 1)
    const pos = target ?? el.scrollLeft
    if ((dy < 0 && pos <= 2) || (dy > 0 && pos >= max - 2)) return
    e.preventDefault()
    el.style.scrollSnapType = 'none'
    target = Math.max(0, Math.min(max, pos + dy))
    el.scrollTo({ left: target, behavior: reducedMotion() ? 'auto' : 'smooth' })
    clearTimeout(wheelTimer)
    wheelTimer = setTimeout(settle, 320)
  }
  // Ngừng lăn: tự cuộn tới thẻ gần nhất (Firefox không tự căn lại khi bật snap trở lại) rồi bật snap
  function settle() {
    target = null
    if (!el) return
    const to = nearestSnap()
    if (to != null && Math.abs(to - el.scrollLeft) > 1) el.scrollTo({ left: to, behavior: reducedMotion() ? 'auto' : 'smooth' })
    wheelTimer = setTimeout(() => el && (el.style.scrollSnapType = ''), 380)
  }
  function nearestSnap() {
    const max = el.scrollWidth - el.clientWidth
    if (el.scrollLeft >= max - 2) return null
    const box = el.getBoundingClientRect()
    const pad = parseFloat(getComputedStyle(el).scrollPaddingLeft) || 0
    let best = null
    for (const c of el.children) {
      if (getComputedStyle(c).scrollSnapAlign === 'none') continue
      const at = Math.min(max, Math.max(0, c.getBoundingClientRect().left - box.left - el.clientLeft + el.scrollLeft - pad))
      if (best == null || Math.abs(at - el.scrollLeft) < Math.abs(best - el.scrollLeft)) best = at
    }
    return best
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
    clearTimeout(wheelTimer)
    el?.removeEventListener('wheel', onWheel)
  })

  return nav
}
