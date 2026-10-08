<script setup>
import { ref } from 'vue'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import { uploadImage } from '@/services/api'

// Ảnh: xem trước + tải lên (JPG/PNG/WebP ≤ 5 MB → /media/<folder>/…) hoặc dán đường dẫn ảnh có sẵn
const url = defineModel({ type: String, default: null })
const props = defineProps({
  folder: { type: String, default: 'products' },
  wide: { type: Boolean, default: false },
})
const toast = useToast()
const input = ref(null)
const busy = ref(false)

async function pick(e) {
  const file = e.target.files[0]
  e.target.value = ''
  if (!file) return
  if (file.size > 5 * 1024 * 1024) {
    toast.add({ severity: 'warn', summary: 'Ảnh quá lớn', detail: 'Chọn ảnh nhỏ hơn 5 MB.', life: 4000 })
    return
  }
  busy.value = true
  try {
    url.value = await uploadImage(file, props.folder)
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được ảnh', detail: err.message, life: 5000 })
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <div class="image-field">
    <div class="image-preview" :class="{ wide }">
      <img v-if="url" :src="url" alt="">
      <span v-else><i class="pi pi-image" style="font-size:28px" /></span>
    </div>
    <div class="actions">
      <Button label="Tải ảnh lên" icon="pi pi-upload" size="small" :loading="busy" @click="input.click()" />
      <Button v-if="url" label="Bỏ ảnh" icon="pi pi-times" size="small" severity="secondary" text @click="url = null" />
    </div>
    <input ref="input" type="file" accept="image/jpeg,image/png,image/webp" hidden @change="pick">
    <InputText v-model="url" placeholder="hoặc đường dẫn, vd. /media/products/sp-….jpg" size="small" fluid />
  </div>
</template>
