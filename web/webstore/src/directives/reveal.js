import { reducedMotion } from '@/utils/motion'

// v-reveal: phần tử ẩn sẵn (CSS [data-reveal]) và hiện dần khi cuộn tới, bằng cách gắn class `in`.
// Bật giảm chuyển động thì hiện ngay. Không dùng trên phần tử có :class động (Vue sẽ ghi đè class `in`).
let io
const observer = () => (io ??= new IntersectionObserver(entries => {
  for (const e of entries) {
    if (e.isIntersecting) {
      e.target.classList.add('in')
      io.unobserve(e.target)
    }
  }
}, { rootMargin: '0px 0px -12% 0px', threshold: 0.15 }))

export const vReveal = {
  beforeMount(el) {
    el.dataset.reveal = ''
  },
  mounted(el) {
    if (reducedMotion() || !('IntersectionObserver' in window)) el.classList.add('in')
    else observer().observe(el)
  },
  beforeUnmount(el) {
    io?.unobserve(el)
  },
}
