export const reducedMotion = () => window.matchMedia?.('(prefers-reduced-motion: reduce)').matches ?? false

// Chạy lại một animation CSS trên phần tử: gỡ class, ép reflow rồi gắn lại
export function replay(el, cls) {
  if (!el) return
  el.classList.remove(cls)
  void el.offsetWidth
  el.classList.add(cls)
}
