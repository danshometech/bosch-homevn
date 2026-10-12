// Tải script Cloudflare Turnstile một lần (render tay qua window.turnstile.render), dùng chung cho mọi ô xác minh
const SRC = 'https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit'
let loading = null

export function loadTurnstile() {
  loading ||= new Promise((resolve, reject) => {
    if (window.turnstile) return resolve(window.turnstile)
    const s = document.createElement('script')
    s.src = SRC
    s.async = true
    s.defer = true
    s.onload = () => resolve(window.turnstile)
    s.onerror = () => {
      loading = null
      s.remove()
      reject(new Error('Không tải được Cloudflare Turnstile'))
    }
    document.head.appendChild(s)
  })
  return loading
}
