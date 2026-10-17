<script setup>
import { ref, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import { get, put } from '@/services/api'

const LINKS = [
  {
    key: 'zaloUrl',
    label: 'Zalo',
    placeholder: 'vd. 0912345678 hoặc https://zalo.me/0912345678',
    hint: 'Số điện thoại Zalo hoặc link trang Zalo / Zalo OA. Khách bấm nút Zalo (nút liên hệ nổi, hộp tư vấn trang chủ, chân trang) sẽ mở trang này. Để trống thì ẩn nút Zalo.',
  },
  {
    key: 'facebookUrl',
    label: 'Chat tư vấn Facebook',
    placeholder: 'vd. https://www.facebook.com/boschhomevn hoặc boschhomevn',
    hint: 'Link fanpage hoặc tên trang. Hiện icon Facebook ở nút liên hệ nổi và chân trang. Để trống thì ẩn.',
  },
  {
    key: 'tikTokUrl',
    label: 'TikTok',
    placeholder: 'vd. https://www.tiktok.com/@boschhomevn hoặc @boschhomevn',
    hint: 'Link kênh hoặc tên kênh. Hiện icon TikTok ở nút liên hệ nổi và chân trang. Để trống thì ẩn.',
  },
  {
    key: 'youTubeUrl',
    label: 'YouTube',
    placeholder: 'vd. https://www.youtube.com/@boschhomevn hoặc @boschhomevn',
    hint: 'Link kênh hoặc tên kênh. Hiện icon YouTube ở nút liên hệ nổi và chân trang. Để trống thì ẩn.',
  },
]

const toast = useToast()
const form = ref({ hotline: '', ...Object.fromEntries(LINKS.map(l => [l.key, ''])) })
const saved = ref(null)
const loading = ref(true)
const saving = ref(false)

const apply = s => {
  saved.value = s
  form.value = { hotline: s.hotline, ...Object.fromEntries(LINKS.map(l => [l.key, s[l.key] ?? ''])) }
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
  const f = form.value
  try {
    apply(await put('/admin/settings/contact', {
      hotline: f.hotline.trim(),
      ...Object.fromEntries(LINKS.map(l => [l.key, f[l.key].trim() || null])),
    }))
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
      <div v-for="l in LINKS" :key="l.key" class="field">
        <label :for="l.key">{{ l.label }}</label>
        <div class="row">
          <InputText :id="l.key" v-model="form[l.key]" maxlength="300" :placeholder="l.placeholder" />
          <Button v-if="saved?.[l.key]" as="a" :href="saved[l.key]" target="_blank" rel="noopener" label="Mở thử" icon="pi pi-external-link" severity="secondary" outlined />
        </div>
        <small>{{ l.hint }}</small>
        <small v-if="saved?.[l.key]">Đang dùng: <a :href="saved[l.key]" target="_blank" rel="noopener">{{ saved[l.key] }}</a></small>
      </div>
    </div>
    <div class="actions" style="margin-top:20px">
      <Button type="submit" label="Lưu" icon="pi pi-check" :loading="saving" />
    </div>
  </form>
</template>
