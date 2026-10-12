<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import ImageField from '@/components/ImageField.vue'
import { get, put } from '@/services/api'

const router = useRouter()
const toast = useToast()
const form = ref(null)
const saving = ref(false)

onMounted(async () => {
  try {
    form.value = await get('/admin/hero')
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được banner', detail: err.message, life: 5000 })
    router.replace({ name: 'moments' })
  }
})

async function save() {
  saving.value = true
  try {
    form.value = await put('/admin/hero', form.value)
    toast.add({ severity: 'success', summary: 'Đã lưu banner', life: 2500 })
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không lưu được', detail: err.message, life: 6000 })
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <form v-if="form" @submit.prevent="save">
    <div class="page-h">
      <div>
        <RouterLink class="crumb" :to="{ name: 'moments' }">← Khoảnh khắc trang chủ</RouterLink>
        <h1>Banner đầu trang</h1>
      </div>
      <Button type="submit" label="Lưu" icon="pi pi-check" :loading="saving" />
    </div>

    <div class="edit-layout">
      <div>
        <section class="box">
          <h2>Nội dung</h2>
          <div class="form-grid">
            <div class="field">
              <label for="time">Giờ</label>
              <InputText id="time" v-model="form.time" required pattern="[0-2][0-9]:[0-5][0-9]" placeholder="06:32" maxlength="5" />
            </div>
            <div class="field">
              <label for="kicker">Dòng chữ nhỏ cạnh giờ</label>
              <InputText id="kicker" v-model="form.kicker" placeholder="vd. A Day with Bosch" maxlength="60" />
            </div>
            <div class="field wide">
              <label for="title">Tiêu đề</label>
              <Textarea id="title" v-model="form.title" required rows="2" auto-resize maxlength="120" />
              <small>Mỗi dòng là một dòng chữ lớn trên banner.</small>
            </div>
            <div class="field wide">
              <label for="lead">Lời dẫn</label>
              <Textarea id="lead" v-model="form.lead" rows="3" auto-resize maxlength="400" />
            </div>
          </div>
        </section>

        <section class="box">
          <h2>Nút</h2>
          <div class="form-grid">
            <div class="field">
              <label for="cta-label">Chữ trên nút</label>
              <InputText id="cta-label" v-model="form.ctaLabel" placeholder="vd. Khám phá căn bếp" maxlength="60" />
            </div>
            <div class="field">
              <label for="cta-url">Đường dẫn</label>
              <InputText id="cta-url" v-model="form.ctaUrl" placeholder="/san-pham/thiet-bi-bep" maxlength="200" />
            </div>
          </div>
          <small class="muted">Để trống cả hai ô thì banner không có nút.</small>
        </section>
      </div>

      <aside>
        <section class="box">
          <h2>Ảnh nền</h2>
          <ImageField v-model="form.imageUrl" folder="moments" wide />
          <div class="field" style="margin-top:12px">
            <label for="pos">Vị trí khung ảnh</label>
            <InputText id="pos" v-model="form.imagePosition" placeholder="vd. 50% 50%" maxlength="50" />
            <small>object-position: ngang dọc, phần ảnh giữ lại khi cắt. Để trống thì tự canh theo màn hình.</small>
          </div>
          <div class="field" style="margin-top:12px">
            <label for="alt">Mô tả ảnh (alt)</label>
            <InputText id="alt" v-model="form.imageAlt" maxlength="200" />
          </div>
        </section>
      </aside>
    </div>
  </form>
</template>
