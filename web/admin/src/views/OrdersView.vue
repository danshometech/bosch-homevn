<script setup>
import { ref, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import InputText from 'primevue/inputtext'
import Tag from 'primevue/tag'
import OrderDetailExportDialog from '@/components/OrderDetailExportDialog.vue'
import OrderExportDialog from '@/components/OrderExportDialog.vue'
import { get } from '@/services/api'
import { fmt, fmtDate, ORDER_STATUS, orderStatusOf } from '@/utils/format'
import { useOrderBadgeStore } from '@/stores/orders'

// Lọc trạng thái / từ khóa / trang giữ trên URL (?status=New&q=…&page=2): mở chi tiết rồi quay lại vẫn đúng chỗ
const PAGE_SIZE = 20
const route = useRoute()
const router = useRouter()
const toast = useToast()
const badge = useOrderBadgeStore()

const status = computed(() => String(route.query.status || ''))
const page = computed(() => Math.max(1, Number(route.query.page) || 1))
const q = ref(String(route.query.q || ''))
const data = ref({ items: [], total: 0, counts: {} })
const loading = ref(true)

let request = 0
async function load() {
  const req = ++request
  loading.value = true
  try {
    const params = new URLSearchParams({ page: page.value, pageSize: PAGE_SIZE })
    if (status.value) params.set('status', status.value)
    if (route.query.q) params.set('q', route.query.q)
    const res = await get('/admin/orders?' + params)
    if (req === request) data.value = res
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được đơn hàng', detail: err.message, life: 5000 })
  } finally {
    if (req === request) loading.value = false
  }
  badge.refresh()
}
watch(() => route.query, load, { immediate: true })

// Bỏ tham số rỗng; đổi bộ lọc thì về trang 1
function setQuery(patch) {
  const query = Object.fromEntries(Object.entries({ ...route.query, page: undefined, ...patch }).filter(([, v]) => v))
  router.replace({ query })
}
let typing = 0
watch(q, v => {
  clearTimeout(typing)
  typing = setTimeout(() => setQuery({ q: v.trim() }), 300)
})

// Xuất Excel: các đơn trong khoảng ngày / chi tiết một đơn — mỗi loại một hộp thoại
const exportOpen = ref(false)
const detailOpen = ref(false)

const tabs = computed(() => {
  const counts = data.value.counts || {}
  const all = Object.values(counts).reduce((s, n) => s + n, 0)
  return [{ value: '', label: 'Tất cả', count: all }, ...ORDER_STATUS.map(s => ({ ...s, count: counts[s.value] || 0 }))]
})
</script>

<template>
  <div class="page-h">
    <div>
      <h1>Đơn hàng</h1>
      <p class="muted" style="margin:4px 0 0">Đơn khách đặt trên site, mới nhất ở trên. Bấm một đơn để xem và cập nhật trạng thái.</p>
    </div>
    <div class="orders-tools">
      <InputText v-model="q" placeholder="Tìm mã đơn, số điện thoại, tên khách…" fluid />
      <div class="orders-export">
        <Button label="Xuất Excel" icon="pi pi-file-excel" severity="secondary" outlined title="Xuất các đơn trong một khoảng ngày" @click="exportOpen = true" />
        <Button label="Xuất chi tiết đơn" icon="pi pi-file-export" severity="secondary" outlined title="Xuất toàn bộ thông tin một đơn" @click="detailOpen = true" />
      </div>
    </div>
    <OrderExportDialog v-model:visible="exportOpen" />
    <OrderDetailExportDialog v-model:visible="detailOpen" />
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
      data-key="code"
      size="small"
      row-hover
      style="cursor:pointer"
      @page="e => setQuery({ page: e.page > 0 ? e.page + 1 : undefined })"
      @row-click="e => router.push({ name: 'order-detail', params: { code: e.data.code } })"
    >
      <template #empty><p class="muted" style="margin:12px 0">{{ route.query.q ? 'Không có đơn nào khớp từ khóa.' : 'Chưa có đơn hàng.' }}</p></template>
      <Column header="Mã đơn" style="width:170px">
        <template #body="{ data: o }">
          <b class="mono">{{ o.code }}</b>
          <div class="muted" style="font-size:12px">{{ fmtDate(o.createdAt) }}</div>
        </template>
      </Column>
      <Column header="Khách hàng">
        <template #body="{ data: o }">
          <b>{{ o.customerName }}</b>
          <div class="muted" style="font-size:13px">{{ o.customerPhone }}</div>
        </template>
      </Column>
      <Column header="SL" style="width:60px">
        <template #body="{ data: o }">{{ o.itemCount }}</template>
      </Column>
      <Column header="Tổng tiền" style="width:140px">
        <template #body="{ data: o }"><b>{{ fmt(o.total) }}</b></template>
      </Column>
      <Column header="Thanh toán" style="width:200px">
        <template #body="{ data: o }">
          <div style="font-size:13px">{{ o.paymentMethodName }}</div>
          <Tag v-if="o.isPaid" value="Đã thu tiền" severity="success" style="margin-top:4px" />
        </template>
      </Column>
      <Column header="Trạng thái" style="width:130px">
        <template #body="{ data: o }"><Tag :value="orderStatusOf(o.status).label" :severity="orderStatusOf(o.status).severity" /></template>
      </Column>
    </DataTable>
  </div>
</template>
