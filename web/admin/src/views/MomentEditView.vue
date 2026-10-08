<script setup>
import { ref, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import Textarea from 'primevue/textarea'
import ImageField from '@/components/ImageField.vue'
import ProductPicker from '@/components/ProductPicker.vue'
import { get, put } from '@/services/api'
import { TONES } from '@/utils/format'

const props = defineProps({ id: { type: String, required: true } })
const router = useRouter()
const toast = useToast()

const form = ref(null)
const products = ref([])
const saving = ref(false)
// Ảnh bối cảnh (ảnh chụp căn phòng) hoặc ảnh tĩnh sản phẩm đặt trên mảng tường
const sceneKind = ref('image')

watch(() => props.id, async id => {
  try {
    const [m, p] = await Promise.all([get(`/admin/moments/${id}`), get('/admin/products')])
    form.value = m
    products.value = p
    sceneKind.value = m.stillImageUrl && !m.imageUrl ? 'still' : 'image'
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được khoảnh khắc', detail: err.message, life: 5000 })
    router.replace({ name: 'moments' })
  }
}, { immediate: true })

const toneOptions = computed(() => (form.value?.tones || []).map(t => ({ value: t, label: TONES[t]?.label ?? t })))
// Số liệu trên ảnh lấy từ thông số của một sản phẩm trong khoảnh khắc
const factProducts = computed(() => products.value
  .filter(p => form.value.productIds.includes(p.id) || form.value.extraProductIds.includes(p.id))
  .map(p => ({ value: p.id, label: `${p.model} — ${p.name}` })))
const factKeys = computed(() => products.value.find(p => p.id === form.value.factProductId)?.specs.map(s => s.name) ?? [])
const factValue = computed(() => products.value.find(p => p.id === form.value.factProductId)?.specs.find(s => s.name === form.value.factSpecKey)?.value)

async function save() {
  saving.value = true
  try {
    const f = form.value
    const body = {
      ...f,
      imageUrl: sceneKind.value === 'image' ? f.imageUrl : null,
      imagePosition: sceneKind.value === 'image' ? f.imagePosition : null,
      stillImageUrl: sceneKind.value === 'still' ? f.stillImageUrl : null,
    }
    form.value = await put(`/admin/moments/${props.id}`, body)
    toast.add({ severity: 'success', summary: 'Đã lưu', detail: `${form.value.time} ${form.value.name}`, life: 2500 })
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
        <h1>{{ form.time }} · {{ form.name }}</h1>
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
              <InputText id="time" v-model="form.time" required pattern="[0-2][0-9]:[0-5][0-9]" placeholder="06:45" maxlength="5" />
            </div>
            <div class="field">
              <label for="name">Tên</label>
              <InputText id="name" v-model="form.name" required maxlength="100" />
            </div>
            <div class="field">
              <label>Tông màu nền</label>
              <Select v-model="form.tone" :options="toneOptions" option-label="label" option-value="value">
                <template #option="{ option }"><span class="tone-dot" :style="{ background: TONES[option.value]?.color }" />{{ option.label }}</template>
              </Select>
            </div>
            <div class="field wide">
              <label for="title">Tiêu đề</label>
              <InputText id="title" v-model="form.title" required maxlength="200" />
            </div>
            <div class="field wide">
              <label for="lead">Lời dẫn</label>
              <Textarea id="lead" v-model="form.lead" rows="3" auto-resize maxlength="1000" />
            </div>
          </div>
        </section>

        <section class="box">
          <h2>Sản phẩm hàng chính</h2>
          <ProductPicker v-model="form.productIds" :products="products" :exclude="form.extraProductIds" />
          <small class="muted">Không còn sản phẩm nào đang bán thì khoảnh khắc tự ẩn khỏi trang chủ.</small>
        </section>

        <section class="box">
          <h2>Dải "Mua kèm"</h2>
          <div class="form-grid" style="margin-bottom:12px">
            <div class="field wide">
              <label for="extras-title">Tiêu đề dải</label>
              <InputText id="extras-title" v-model="form.extrasTitle" placeholder="vd. Mua kèm cho bữa sáng" maxlength="200" />
            </div>
          </div>
          <ProductPicker v-model="form.extraProductIds" :products="products" :exclude="form.productIds" />
          <div class="form-grid" style="margin-top:12px">
            <div class="field">
              <label for="more-label">Link cuối dải — chữ</label>
              <InputText id="more-label" v-model="form.extrasMoreLabel" placeholder="vd. Xem tất cả gia dụng" maxlength="200" />
            </div>
            <div class="field">
              <label for="more-url">Link cuối dải — đường dẫn</label>
              <InputText id="more-url" v-model="form.extrasMoreUrl" placeholder="/san-pham/gia-dung" maxlength="500" />
            </div>
          </div>
        </section>

        <section class="box">
          <h2>Link dưới lời dẫn</h2>
          <div class="rows">
            <div v-for="(l, i) in form.links" :key="i" class="row">
              <InputText v-model="l.label" placeholder="Chữ, vd. Xem tất cả máy rửa bát" maxlength="200" />
              <InputText v-model="l.url" placeholder="/san-pham/thiet-bi-bep?loai=may-rua-bat" maxlength="500" />
              <Button icon="pi pi-times" text rounded severity="danger" aria-label="Bỏ link" @click="form.links.splice(i, 1)" />
            </div>
          </div>
          <Button label="Thêm link" icon="pi pi-plus" text size="small" style="margin-top:8px" @click="form.links.push({ label: '', url: '' })" />
        </section>
      </div>

      <aside>
        <section class="box">
          <h2>Ảnh</h2>
          <div class="actions" style="margin-bottom:12px">
            <Button label="Ảnh bối cảnh" size="small" :outlined="sceneKind !== 'image'" @click="sceneKind = 'image'" />
            <Button label="Ảnh tĩnh sản phẩm" size="small" :outlined="sceneKind !== 'still'" @click="sceneKind = 'still'" />
          </div>
          <template v-if="sceneKind === 'image'">
            <ImageField v-model="form.imageUrl" folder="moments" wide />
            <div class="field" style="margin-top:12px">
              <label for="pos">Vị trí khung ảnh</label>
              <InputText id="pos" v-model="form.imagePosition" placeholder="vd. 50% 55%" maxlength="50" />
              <small>object-position: ngang dọc, phần ảnh giữ lại khi cắt.</small>
            </div>
          </template>
          <ImageField v-else v-model="form.stillImageUrl" folder="moments" />
          <div class="field" style="margin-top:12px">
            <label for="alt">Mô tả ảnh (alt)</label>
            <InputText id="alt" v-model="form.imageAlt" maxlength="300" />
          </div>
        </section>
        <section class="box">
          <h2>Số liệu trên ảnh</h2>
          <div class="rows" style="gap:12px">
            <div class="field">
              <label>Sản phẩm</label>
              <Select v-model="form.factProductId" :options="factProducts" option-label="label" option-value="value" show-clear placeholder="Không hiện số liệu" />
            </div>
            <div class="field">
              <label>Thông số</label>
              <Select v-model="form.factSpecKey" :options="factKeys" :disabled="!factKeys.length" show-clear
                      :placeholder="form.factProductId && !factKeys.length ? 'Sản phẩm chưa có thông số' : 'Chọn thông số'" />
            </div>
            <div class="field">
              <label for="fact-note">Chú thích</label>
              <InputText id="fact-note" v-model="form.factNote" maxlength="300" />
            </div>
            <small class="muted">Hiện: <b>{{ factValue ?? '— (đang ẩn)' }}</b></small>
          </div>
        </section>
      </aside>
    </div>
  </form>
</template>
