<script setup>
import { ref, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import Button from 'primevue/button'
import DatePicker from 'primevue/datepicker'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Rating from 'primevue/rating'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import Textarea from 'primevue/textarea'
import ToggleSwitch from 'primevue/toggleswitch'
import GalleryField from '@/components/GalleryField.vue'
import { get, post, put, del } from '@/services/api'
import { fmtDate, REVIEW_STATUS, reviewStatusOf, reviewSourceOf } from '@/utils/format'
import { useReviewBadgeStore } from '@/stores/reviews'

const LABELS = ['', 'Rất tệ', 'Tệ', 'Bình thường', 'Tốt', 'Tuyệt vời']
const props = defineProps({ id: { type: String, default: null } })
const route = useRoute()
const router = useRouter()
const toast = useToast()
const confirm = useConfirm()
const badge = useReviewBadgeStore()

const blank = () => ({
  productId: null, rating: 5, content: '', authorName: '', authorPhone: '', photos: [],
  isVerifiedPurchase: false, status: 'Approved', reply: '', createdAt: new Date(), helpfulCount: 0,
})
const form = ref(blank())
const meta = ref(null)
const products = ref([])
const loading = ref(true)
const saving = ref(false)

const toForm = r => {
  meta.value = r
  return {
    productId: r.productId, rating: r.rating, content: r.content, authorName: r.authorName, authorPhone: r.authorPhone ?? '',
    photos: [...r.photos], isVerifiedPurchase: r.isVerifiedPurchase, status: r.status, reply: r.reply ?? '', createdAt: new Date(r.createdAt),
    helpfulCount: r.helpfulCount,
  }
}

watch(() => props.id, async id => {
  loading.value = true
  try {
    const [list, r] = await Promise.all([get('/admin/products'), id ? get(`/admin/reviews/${id}`) : null])
    products.value = list
    if (r) {
      form.value = toForm(r)
    } else {
      meta.value = null
      form.value = { ...blank(), productId: list.some(p => p.id === route.query.product) ? route.query.product : null }
    }
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được đánh giá', detail: err.message, life: 5000 })
    if (err.status === 404) router.replace({ name: 'reviews' })
  } finally {
    loading.value = false
  }
}, { immediate: true })

const options = computed(() => products.value.map(p => ({ value: p.id, label: `${p.model} — ${p.name}`, img: p.imageUrl })))
const valid = computed(() => form.value.productId && form.value.rating && form.value.content.trim() && form.value.authorName.trim() && form.value.createdAt)

async function save() {
  saving.value = true
  try {
    const body = {
      ...form.value,
      content: form.value.content.trim(),
      authorName: form.value.authorName.trim(),
      authorPhone: form.value.authorPhone.trim() || null,
      reply: form.value.reply.trim() || null,
      createdAt: form.value.createdAt.toISOString(),
      helpfulCount: form.value.helpfulCount ?? 0,
    }
    if (props.id) {
      form.value = toForm(await put(`/admin/reviews/${props.id}`, body))
      toast.add({ severity: 'success', summary: 'Đã lưu đánh giá', detail: reviewStatusOf(form.value.status).label, life: 2500 })
    } else {
      const created = await post('/admin/reviews', body)
      toast.add({ severity: 'success', summary: 'Đã thêm đánh giá', detail: reviewStatusOf(created.status).label, life: 2500 })
      router.replace({ name: 'review-edit', params: { id: created.id } })
    }
    badge.refresh()
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không lưu được', detail: err.message, life: 6000 })
  } finally {
    saving.value = false
  }
}

function remove() {
  confirm.require({
    header: 'Xóa đánh giá',
    message: `Xóa hẳn đánh giá của "${meta.value.authorName}"?`,
    icon: 'pi pi-exclamation-triangle',
    acceptProps: { label: 'Xóa', severity: 'danger' },
    rejectProps: { label: 'Hủy', severity: 'secondary', outlined: true },
    accept: async () => {
      try {
        await del(`/admin/reviews/${props.id}`)
        toast.add({ severity: 'success', summary: 'Đã xóa đánh giá', life: 2500 })
        badge.refresh()
        router.replace({ name: 'reviews' })
      } catch (err) {
        toast.add({ severity: 'error', summary: 'Không xóa được', detail: err.message, life: 5000 })
      }
    },
  })
}
</script>

<template>
  <form v-if="!loading" @submit.prevent="save">
    <div class="page-h">
      <div>
        <RouterLink class="crumb" :to="{ name: 'reviews' }">← Đánh giá</RouterLink>
        <h1>{{ id ? 'Sửa đánh giá' : 'Thêm đánh giá' }}</h1>
        <p v-if="meta" class="muted" style="margin:4px 0 0">{{ reviewSourceOf(meta) }} · sửa lần cuối {{ fmtDate(meta.updatedAt) }}</p>
      </div>
      <div class="actions">
        <Tag v-if="meta" :value="reviewStatusOf(meta.status).label" :severity="reviewStatusOf(meta.status).severity" />
        <Button v-if="id" label="Xóa" icon="pi pi-trash" severity="danger" outlined @click="remove" />
        <Button type="submit" label="Lưu" icon="pi pi-check" :loading="saving" :disabled="!valid" />
      </div>
    </div>

    <div class="edit-layout">
      <div>
        <section class="box">
          <div class="rows" style="gap:16px">
            <div class="field">
              <label>Sản phẩm</label>
              <Select v-model="form.productId" :options="options" option-label="label" option-value="value" filter
                      placeholder="Chọn sản phẩm…" :disabled="!!id" fluid>
                <template #option="{ option }">
                  <div class="rv-pick"><img v-if="option.img" :src="option.img" alt=""><span>{{ option.label }}</span></div>
                </template>
              </Select>
              <small v-if="id">Không đổi được sản phẩm của đánh giá đã có.</small>
            </div>
            <div class="field">
              <label>Số sao</label>
              <div class="rv-rating">
                <Rating v-model="form.rating" />
                <span class="muted">{{ LABELS[form.rating] }}</span>
              </div>
            </div>
            <div class="field">
              <label for="content">Nội dung</label>
              <Textarea id="content" v-model="form.content" rows="5" maxlength="1000" auto-resize />
              <small>{{ form.content.length }}/1000</small>
            </div>
          </div>
        </section>
        <section class="box">
          <h2>Phản hồi của cửa hàng</h2>
          <Textarea v-model="form.reply" rows="3" maxlength="1000" auto-resize fluid
                    placeholder="vd. Cảm ơn anh/chị đã tin chọn Bosch…" />
          <small class="muted">Hiện ngay dưới đánh giá trên site. Để trống nếu chưa trả lời.</small>
        </section>
      </div>

      <aside>
        <section class="box">
          <h2>Hiển thị</h2>
          <div class="rows" style="gap:14px">
            <div class="field">
              <label>Trạng thái</label>
              <Select v-model="form.status" :options="REVIEW_STATUS" option-label="label" option-value="value">
                <template #option="{ option }"><Tag :value="option.label" :severity="option.severity" /></template>
              </Select>
              <small>Chỉ đánh giá "Đang hiện" mới hiện trên site và được tính vào điểm sản phẩm.</small>
            </div>
            <div class="field">
              <label for="author">Họ tên khách</label>
              <InputText id="author" v-model="form.authorName" maxlength="100" />
            </div>
            <div class="field">
              <label for="phone">Số điện thoại</label>
              <InputText id="phone" v-model="form.authorPhone" maxlength="20" inputmode="tel" />
              <small>Không hiện trên site.</small>
            </div>
            <label class="check"><ToggleSwitch v-model="form.isVerifiedPurchase" />Đã mua hàng</label>
            <div class="field">
              <label>Ngày đánh giá</label>
              <DatePicker v-model="form.createdAt" show-time hour-format="24" date-format="dd/mm/yy" :max-date="new Date()" show-icon />
            </div>
            <div class="field">
              <label for="helpful">Lượt "Hữu ích"</label>
              <InputNumber v-model="form.helpfulCount" input-id="helpful" :min="0" :max="1000000" :use-grouping="false" show-buttons />
              <small>Khách bấm trên site thì tự cộng / trừ; sửa ở đây để đặt lại số.</small>
            </div>
            <small v-if="meta?.repliedAt" class="muted">Trả lời lúc {{ fmtDate(meta.repliedAt) }}</small>
          </div>
        </section>
        <section class="box">
          <h2>Ảnh của khách</h2>
          <GalleryField v-model="form.photos" folder="reviews" :max="5" />
        </section>
      </aside>
    </div>
  </form>
</template>
