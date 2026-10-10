<script setup>
import { ref, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import { get, put } from '@/services/api'

const toast = useToast()
const form = ref({ hotline: '', zaloUrl: '' })
const saved = ref(null)
const loading = ref(true)
const saving = ref(false)

const apply = s => {
  saved.value = s
  form.value = { hotline: s.hotline, zaloUrl: s.zaloUrl ?? '' }
}

onMounted(async () => {
  try {
    apply(await get('/admin/settings/contact'))
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được cài đặt', detail: err.message, life: 5000 })
  } finally {
    loading.value = false
  }
})

async function save() {
  saving.value = true
  try {
    apply(await put('/admin/settings/contact', { hotline: form.value.hotline.trim(), zaloUrl: form.value.zaloUrl.trim() || null }))
    toast.add({ severity: 'success', summary: 'Đã lưu thông tin liên hệ', life: 2500 })
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không lưu được', detail: err.message, life: 6000 })
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="page-h">
    <h1>Cập nhật liên hệ</h1>
  </div>
  <form v-if="!loading" class="box" style="max-width:720px" @submit.prevent="save">
    <div class="rows" style="gap:18px">
      <div class="field">
        <label for="hotline">Hotline</label>
        <InputText id="hotline" v-model="form.hotline" required maxlength="20" placeholder="vd. 1900 6868" />
        <small>Hiện ở thanh trên cùng, nút gọi, menu điện thoại, chân trang và các thông báo lỗi. Nhập đúng cách viết muốn hiển thị.</small>
      </div>
      <div class="field">
        <label for="zalo">Zalo</label>
        <InputText id="zalo" v-model="form.zaloUrl" maxlength="300" placeholder="vd. 0912345678 hoặc https://zalo.me/0912345678" />
        <small>
          Số điện thoại Zalo hoặc link trang Zalo / Zalo OA. Khách bấm nút Zalo (nút liên hệ nổi, hộp tư vấn trang chủ, chân trang) sẽ mở trang này.
          Để trống thì ẩn nút Zalo.
        </small>
        <small v-if="saved?.zaloUrl">Đang dùng: <a :href="saved.zaloUrl" target="_blank" rel="noopener">{{ saved.zaloUrl }}</a></small>
      </div>
    </div>
    <div class="actions" style="margin-top:20px">
      <Button type="submit" label="Lưu" icon="pi pi-check" :loading="saving" />
      <Button v-if="saved?.zaloUrl" as="a" :href="saved.zaloUrl" target="_blank" rel="noopener" label="Mở thử Zalo" icon="pi pi-external-link" severity="secondary" outlined />
    </div>
  </form>
</template>
