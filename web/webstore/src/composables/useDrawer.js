import { watch, nextTick, onUnmounted } from 'vue'

const FOCUSABLE = 'a[href],button:not([disabled]),input:not([disabled]),select,textarea,[tabindex]:not([tabindex="-1"])'

// Ngăn kéo trượt từ cạnh (menu danh mục, bộ lọc): khi mở thì khóa cuộn trang, Esc để đóng, Tab chỉ chạy
// trong ngăn kéo; khi đóng thì trả focus về nút đã mở nó.
// `open` là ref đóng/mở, `panel` là ref phần tử ngăn kéo (phần tử có data-autofocus được focus đầu tiên).
export function useDrawer(open, panel) {
  let opener = null

  function focusables() {
    return [...(panel.value?.querySelectorAll(FOCUSABLE) || [])].filter(el => el.offsetParent !== null)
  }
  function onKey(e) {
    if (e.key === 'Escape') {
      open.value = false
    } else if (e.key === 'Tab') {
      const els = focusables()
      if (!els.length) return
      const first = els[0], last = els[els.length - 1]
      if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus() }
      else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus() }
    }
  }
  function release() {
    document.documentElement.classList.remove('drawer-open')
    document.removeEventListener('keydown', onKey)
  }

  watch(open, v => {
    if (v) {
      opener = document.activeElement
      document.documentElement.classList.add('drawer-open')
      document.addEventListener('keydown', onKey)
      nextTick(() => (panel.value?.querySelector('[data-autofocus]') || focusables()[0])?.focus())
    } else {
      release()
      opener?.focus?.()
      opener = null
    }
  })
  onUnmounted(release)
}
