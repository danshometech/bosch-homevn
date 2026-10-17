<script setup>
import { ref, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import { get, put } from '@/services/api'
import { fmtDate, REVIEW_STATUS, reviewStatusOf } from '@/utils/format'
import { useReviewBadgeStore } from '@/stores/reviews'

const PAGE_SIZE = 20
const STARS = [{ value: null, label: 'Mọi số sao' }, ...[5, 4, 3, 2, 1].map(n => ({ value: n, label: `${n} sao` }))]
const route = useRoute()
const router = useRouter()
const toast = useToast()
const badge = useReviewBadgeStore()

const status = computed(() => String(route.query.status || ''))
const page = computed(() => Math.max(1, Number(route.query.page) || 1))
const stars = computed(() => Number(route.query.stars) || null)
const q = ref(String(route.query.q || ''))
const data = ref({ items: [], total: 0, counts: {} })
const loading = ref(true)
const busy = ref(null)

let request = 0
async function load() {
  const req = ++request
  loading.value = true
  try {
    const params = new URLSearchParams({ page: page.value, pageSize: PAGE_SIZE })
    for (const k of ['status', 'q', 'product', 'stars']) if (route.query[k]) params.set(k, route.query[k])
    const res = await get('/admin/reviews?' + params)
    if (req === request) data.value = res
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được đánh giá', detail: err.message, life: 5000 })
  } finally {
    if (req === request) loading.value = false
  }
  badge.refresh()
}
watch(() => route.query, load, { immediate: true })

function setQuery(patch) {
  const query = Object.fromEntries(Object.entries({ ...route.query, page: undefined, ...patch }).filter(([, v]) => v))
  router.replace({ query })
}
let typing = 0
watch(q, v => {
  clearTimeout(typing)
  typing = setTimeout(() => setQuery({ q: v.trim() }), 300)
})

const tabs = computed(() => {
  const counts = data.value.counts || {}
  const all = Object.values(counts).reduce((s, n) => s + n, 0)
  return [{ value: '', label: 'Tất cả', count: all }, ...REVIEW_STATUS.map(s => ({ ...s, count: counts[s.value] || 0 }))]
})
// Đang lọc theo một sản phẩm (?product=, vd. bấm từ trang sửa sản phẩm)
const product = computed(() => route.query.product && data.value.items.find(r => r.productId === route.query.product))

async function setStatus(r, value) {
  busy.value = r.id
  try {
    await put(`/admin/reviews/${r.id}/status`, { status: value })
    toast.add({ severity: 'success', summary: value === 'Approved' ? 'Đã duyệt, đánh giá đang hiện trên site' : value === 'Hidden' ? 'Đã ẩn đánh giá' : 'Đã chuyển về chờ duyệt', life: 2500 })
    await load()
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không lưu được', detail: err.message, life: 5000 })
  } finally {
    busy.value = null
  }
}
</script>

<template>
  <div class="page-h">
    <div>
      <h1>Đánh giá</h1>
      <p class="muted" style="margin:4px 0 0">Đánh giá khách gửi trên site phải duyệt mới hiện. Bấm một đánh giá để sửa hoặc trả lời.</p>
    </div>
    <div class="orders-tools">
      <InputText v-model="q" placeholder="Tìm tên khách, số điện thoại, nội dung, sản phẩm…" fluid />
      <div class="orders-export">
        <Select :model-value="stars" :options="STARS" option-label="label" option-value="value" placeholder="Mọi số sao" @update:model-value="v => setQuery({ stars: v })" />
        <Button label="Thêm đánh giá" icon="pi pi-plus" @click="router.push({ name: 'review-new', query: route.query.product ? { product: route.query.product } : {} })" />
      </div>
    </div>
  </div>

  <div class="status-tabs" role="tablist" aria-label="Lọc theo trạng thái">
    <button
      v-for="t in tabs"
      :key="t.value"
      type="button"
      role="tab"
      class="status-tab"
      :class="{ on: status === t.value }"
      :aria-selected="status === t.value"
      @click="setQuery({ status: t.value })"
    >{{ t.label }}<span class="n">{{ t.count }}</span></button>
    <button v-if="route.query.product" type="button" class="status-tab" title="Bỏ lọc sản phẩm" @click="setQuery({ product: undefined })">
      Sản phẩm: {{ product?.productModel || route.query.product }}<i class="pi pi-times" style="font-size:11px" />
    </button>
  </div>

  <div class="box">
    <DataTable
      :value="data.items"
      :loading="loading"
      lazy
      paginator
      :rows="PAGE_SIZE"
      :total-records="data.total"
      :first="(page - 1) * PAGE_SIZE"
      data-key="id"
      size="small"
      row-hover
      style="cursor:pointer"
      @page="e => setQuery({ page: e.page > 0 ? e.page + 1 : undefined })"
      @row-click="e => router.push({ name: 'review-edit', params: { id: e.data.id } })"
    >
      <template #empty><p class="muted" style="margin:12px 0">{{ route.query.q ? 'Không có đánh giá nào khớp từ khóa.' : 'Chưa có đánh giá.' }}</p></template>
      <Column header="Sản phẩm" style="width:260px">
        <template #body="{ data: r }">
          <div class="rv-product">
            <img v-if="r.productImageUrl" class="thumb" :src="r.productImageUrl" alt="">
            <span v-else class="thumb empty"><i class="pi pi-image" /></span>
            <div><div class="rv-name">{{ r.productName }}</div><small class="muted mono">{{ r.productModel }}</small></div>
          </div>
        </template>
      </Column>
      <Column header="Đánh giá">
        <template #body="{ data: r }">
          <div class="rv-stars" :aria-label="`${r.rating} sao`">{{ '★'.repeat(r.rating) }}<span>{{ '★'.repeat(5 - r.rating) }}</span></div>
          <div class="rv-text">{{ r.content }}</div>
          <div class="rv-meta muted">
            <span v-if="r.photos.length"><i class="pi pi-image" /> {{ r.photos.length }} ảnh</span>
            <span v-if="r.reply"><i class="pi pi-reply" /> Đã trả lời</span>
            <span v-if="r.helpfulCount"><i class="pi pi-thumbs-up" /> {{ r.helpfulCount }}</span>
          </div>
        </template>
      </Column>
      <Column header="Khách hàng" style="width:190px">
        <template #body="{ data: r }">
          <b>{{ r.authorName }}</b>
          <div v-if="r.authorPhone" class="muted" style="font-size:13px">{{ r.authorPhone }}</div>
          <Tag v-if="r.isVerifiedPurchase" value="Đã mua hàng" severity="success" style="margin-top:4px" />
          <Tag v-if="r.createdBy === 'seed'" value="Dữ liệu mẫu" severity="secondary" style="margin-top:4px" />
        </template>
      </Column>
      <Column header="Ngày" style="width:120px">
        <template #body="{ data: r }"><span style="font-size:13px">{{ fmtDate(r.createdAt) }}</span></template>
      </Column>
      <Column header="Trạng thái" style="width:200px">
        <template #body="{ data: r }">
          <div class="rv-status">
            <Tag :value="reviewStatusOf(r.status).label" :severity="reviewStatusOf(r.status).severity" />
            <Button v-if="r.status !== 'Approved'" label="Duyệt" icon="pi pi-check" size="small" severity="success" text
                    :loading="busy === r.id" @click.stop="setStatus(r, 'Approved')" />
            <Button v-if="r.status !== 'Hidden'" label="Ẩn" icon="pi pi-eye-slash" size="small" severity="secondary" text
                    :disabled="busy === r.id" @click.stop="setStatus(r, 'Hidden')" />
          </div>
        </template>
      </Column>
    </DataTable>
  </div>
</template>
