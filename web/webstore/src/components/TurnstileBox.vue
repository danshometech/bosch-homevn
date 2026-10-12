<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { loadTurnstile } from '@/services/turnstile'

// Xác minh Cloudflare Turnstile (chống spam đặt hàng): chỉ chạy khi gọi getToken() lúc khách bấm Đặt hàng, không chạy
// lúc mở trang. Mỗi lần bấm ô xác minh hiện ra (đang xác minh → ✓), Cloudflare nghi ngờ thì bắt khách tích.
// Mỗi lần gọi lấy token mới (token chỉ dùng được một lần)
const props = defineProps({ siteKey: { type: String, required: true } })
const el = ref(null)
const failed = ref(false)
// Ô chỉ chiếm chỗ từ lần bấm Đặt hàng đầu tiên
const shown = ref(false)
let widget = null
let pending = null
let ready = null

function settle(err, token) {
  const p = pending
  pending = null
  if (!p) return
  if (err) p.reject(err)
  else p.resolve(token)
}

async function setup() {
  try {
    const turnstile = await loadTurnstile()
    if (!el.value) return
    widget = turnstile.render(el.value, {
      sitekey: props.siteKey,
      language: 'vi',
      size: 'flexible',
      execution: 'execute',
      appearance: 'execute',
      callback: token => settle(null, token),
      'error-callback': () => {
        settle(new Error('Turnstile báo lỗi'))
        return true
      },
      'timeout-callback': () => settle(new Error('Hết thời gian xác minh')),
    })
  } catch (e) {
    console.error(e)
    failed.value = true
  }
}
onMounted(() => (ready = setup()))
onBeforeUnmount(() => {
  settle(new Error('Đã rời trang'))
  if (widget != null) window.turnstile?.remove(widget)
})

async function getToken() {
  await ready
  if (widget == null) throw new Error('Chưa tải được Turnstile')
  shown.value = true
  return new Promise((resolve, reject) => {
    settle(new Error('Đã gửi lại'))
    pending = { resolve, reject }
    window.turnstile.reset(widget)
    window.turnstile.execute(widget)
  })
}
defineExpose({ getToken })
</script>

<template>
  <div class="captcha" :class="{ on: shown }">
    <div ref="el" />
    <small v-if="failed" class="muted">Không tải được bước xác minh chống spam — kiểm tra kết nối rồi tải lại trang.</small>
  </div>
</template>
