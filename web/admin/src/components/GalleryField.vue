<script setup>
import { ref } from 'vue'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import { uploadImage } from '@/services/api'

// Danh sách ảnh: tải lên (chọn được nhiều ảnh), dán đường dẫn, đổi thứ tự, bỏ — tối đa `max` ảnh
const urls = defineModel({ type: Array, default: () => [] })
const props = defineProps({
  max: { type: Number, default: 4 },
  folder: { type: String, default: 'products' },
})
const toast = useToast()
const input = ref(null)
const link = ref('')
const busy = ref(false)

async function pick(e) {
  const files = [...e.target.files].slice(0, props.max - urls.value.length)
  e.target.value = ''
  // Gom ảnh tải xong rồi thêm một lần: v-model chỉ cập nhật sau khi component cha render lại
  const added = []
  busy.value = true
  try {
    for (const file of files) {
      if (file.size > 5 * 1024 * 1024) {
        toast.add({ severity: 'warn', summary: 'Ảnh quá lớn', detail: `${file.name}: chọn ảnh nhỏ hơn 5 MB.`, life: 4000 })
        continue
      }
      added.push(await uploadImage(file, props.folder))
    }
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được ảnh', detail: err.message, life: 5000 })
  } finally {
    if (added.length) urls.value = [...urls.value, ...added]
    busy.value = false
  }
}

function addLink() {
  const u = link.value.trim()
  if (u && !urls.value.includes(u) && urls.value.length < props.max) urls.value = [...urls.value, u]
  link.value = ''
}

function move(i, d) {
  const a = [...urls.value]
  ;[a[i], a[i + d]] = [a[i + d], a[i]]
  urls.value = a
}
const remove = i => (urls.value = urls.value.filter((_, j) => j !== i))
</script>

<template>
  <div class="gallery-field">
    <div class="gallery-grid">
      <div v-for="(u, i) in urls" :key="u" class="gallery-item">
        <img :src="u" alt="">
        <div class="gallery-tools">
          <Button icon="pi pi-arrow-left" text rounded size="small" aria-label="Lên trước" :disabled="i === 0" @click="move(i, -1)" />
          <Button icon="pi pi-arrow-right" text rounded size="small" aria-label="Ra sau" :disabled="i === urls.length - 1" @click="move(i, 1)" />
          <Button icon="pi pi-times" text rounded size="small" severity="danger" aria-label="Bỏ ảnh" @click="remove(i)" />
        </div>
      </div>
      <button v-if="urls.length < max" type="button" class="gallery-add" :disabled="busy" @click="input.click()">
        <i :class="busy ? 'pi pi-spin pi-spinner' : 'pi pi-plus'" />{{ busy ? 'Đang tải…' : 'Thêm ảnh' }}
      </button>
    </div>
    <input ref="input" type="file" accept="image/jpeg,image/png,image/webp" multiple hidden @change="pick">
    <InputText
      v-if="urls.length < max"
      v-model="link"
      placeholder="hoặc dán đường dẫn ảnh rồi Enter"
      size="small"
      fluid
      @keydown.enter.prevent="addLink"
      @blur="addLink"
    />
  </div>
</template>
