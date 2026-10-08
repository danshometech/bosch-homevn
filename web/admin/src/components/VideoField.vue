<script setup>
import { ref } from 'vue'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import ProgressBar from 'primevue/progressbar'
import { uploadImage, uploadVideo } from '@/services/api'

// Video giới thiệu: tải file MP4 / WebM (≤ 50 MB) lên, xem trước, bỏ. Ảnh bìa (v-model:poster) là một khung hình của
// video: tự chụp ở giây thứ 1 khi tải lên, hoặc chụp khung đang dừng trong ô xem trước
const url = defineModel({ type: String, default: null })
const poster = defineModel('poster', { type: String, default: null })
const MAX = 50 * 1024 * 1024
const TYPES = ['video/mp4', 'video/webm']
const toast = useToast()
const input = ref(null)
const preview = ref(null)
const progress = ref(null) // % đang tải lên, null = không tải
const capturing = ref(false)

// Vẽ khung hình hiện tại của <video> ra JPEG (rộng tối đa 1280px) rồi tải lên như ảnh sản phẩm
async function uploadFrame(video) {
  const scale = Math.min(1, 1280 / video.videoWidth)
  const canvas = Object.assign(document.createElement('canvas'), {
    width: Math.round(video.videoWidth * scale),
    height: Math.round(video.videoHeight * scale),
  })
  canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height)
  const blob = await new Promise(r => canvas.toBlob(r, 'image/jpeg', 0.85))
  return uploadImage(new File([blob], 'poster.jpg', { type: 'image/jpeg' }), 'products')
}

// Chụp khung hình ở giây thứ 1 (video ngắn hơn thì lấy 1/10 thời lượng) từ file vừa chọn, chưa cần tải video về
async function frameFromFile(file) {
  const video = Object.assign(document.createElement('video'), { muted: true, playsInline: true, preload: 'auto' })
  video.src = URL.createObjectURL(file)
  try {
    await new Promise((resolve, reject) => {
      video.onloadedmetadata = () => (video.currentTime = Math.min(1, video.duration / 10))
      video.onseeked = resolve
      video.onerror = () => reject(new Error('Trình duyệt không đọc được video này'))
      setTimeout(() => reject(new Error('Quá thời gian chụp ảnh bìa')), 8000)
    })
    return await uploadFrame(video)
  } finally {
    URL.revokeObjectURL(video.src)
  }
}

async function pick(e) {
  const file = e.target.files[0]
  e.target.value = ''
  if (!file) return
  if (file.type && !TYPES.includes(file.type)) {
    toast.add({ severity: 'warn', summary: 'Sai định dạng', detail: 'Chỉ nhận video MP4 hoặc WebM.', life: 4000 })
    return
  }
  if (file.size > MAX) {
    toast.add({ severity: 'warn', summary: 'Video quá lớn', detail: 'Chọn video nhỏ hơn 50 MB.', life: 4000 })
    return
  }
  progress.value = 0
  try {
    const [videoUrl, posterUrl] = await Promise.all([
      uploadVideo(file, p => (progress.value = p)),
      frameFromFile(file).catch(() => null), // không chụp được ảnh bìa thì vẫn lưu video
    ])
    url.value = videoUrl
    poster.value = posterUrl
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được video', detail: err.message, life: 5000 })
  } finally {
    progress.value = null
  }
}

async function captureCurrent() {
  if (!preview.value?.videoWidth) {
    toast.add({ severity: 'warn', summary: 'Video chưa tải xong', detail: 'Bấm phát video rồi dừng ở khung hình muốn chụp.', life: 4000 })
    return
  }
  capturing.value = true
  try {
    poster.value = await uploadFrame(preview.value)
    toast.add({ severity: 'success', summary: 'Đã đổi ảnh bìa', detail: 'Nhớ bấm Lưu.', life: 2500 })
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không chụp được ảnh bìa', detail: err.message, life: 5000 })
  } finally {
    capturing.value = false
  }
}

function remove() {
  url.value = null
  poster.value = null
}
</script>

<template>
  <div class="video-field">
    <video v-if="url" ref="preview" :key="url" class="video-preview" :src="url" :poster="poster || undefined" controls preload="metadata" />
    <div class="actions">
      <Button
        :label="progress !== null ? `Đang tải ${progress}%` : url ? 'Đổi video' : 'Tải video lên'"
        icon="pi pi-video"
        size="small"
        :loading="progress !== null"
        @click="input.click()"
      />
      <Button v-if="url && progress === null" label="Bỏ video" icon="pi pi-times" size="small" severity="secondary" text @click="remove" />
    </div>
    <ProgressBar v-if="progress !== null" :value="progress" :show-value="false" style="height:6px" />
    <div v-if="url && progress === null" class="video-poster">
      <img v-if="poster" :src="poster" alt="Ảnh bìa video">
      <span v-else class="muted">Chưa có ảnh bìa</span>
      <div>
        <small class="muted">Ảnh bìa hiện ở ô ▶ trên trang chi tiết. Dừng video ở khung hình đẹp rồi bấm:</small>
        <Button label="Lấy khung hình đang xem làm ảnh bìa" icon="pi pi-camera" size="small" text :loading="capturing" @click="captureCurrent" />
      </div>
    </div>
    <small class="muted">MP4 hoặc WebM, tối đa 50 MB. Hiện thành ô ▶ cuối dải ảnh ở trang chi tiết.</small>
    <input ref="input" type="file" accept="video/mp4,video/webm" hidden @change="pick">
  </div>
</template>
