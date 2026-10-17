<script setup>
import { ref, computed, watch } from 'vue'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Tag from 'primevue/tag'
import DateRangeField from './DateRangeField.vue'
import { get, download } from '@/services/api'
import { addDays, fmt, fmtDate, isoDate, orderStatusOf, startOfToday } from '@/utils/format'

const PAGE_SIZE = 8
const visible = defineModel('visible', { type: Boolean, default: false })
const toast = useToast()

const range = ref([addDays(startOfToday(), -29), startOfToday()])
const q = ref('')
const term = ref('')
const page = ref(1)
const data = ref({ items: [], total: 0 })
const loading = ref(false)
const selected = ref(null)
const busy = ref(false)

const ready = computed(() => !!range.value?.[0] && !!range.value?.[1])

let request = 0
async function load() {
  if (!visible.value || !ready.value) return
  const req = ++request
  loading.value = true
  try {
    const params = new URLSearchParams({ from: isoDate(range.value[0]), to: isoDate(range.value[1]), page: page.value, pageSize: PAGE_SIZE })
    if (term.value) params.set('q', term.value)
    const res = await get('/admin/orders?' + params)
    if (req === request) data.value = res
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được danh sách đơn', detail: err.message, life: 5000 })
  } finally {
    if (req === request) loading.value = false
  }
}

// Đổi khoảng ngày / từ khóa thì về trang 1 và bỏ chọn
watch([range, term], () => {
  page.value = 1
  selected.value = null
})
watch([visible, range, term, page], load, { immediate: true })
let typing = 0
watch(q, v => {
  clearTimeout(typing)
  typing = setTimeout(() => (term.value = v.trim()), 300)
})

async function run() {
  busy.value = true
  try {
    const code = selected.value.code
    await download(`/admin/orders/${encodeURIComponent(code)}/export`, `chi-tiet-don-hang-${code}.xlsx`)
    visible.value = false
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không xuất được file Excel', detail: err.message, life: 5000 })
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <Dialog v-model:visible="visible" modal header="Xuất chi tiết một đơn" :style="{ width: 'min(760px, 96vw)' }">
    <div class="dialog-body">
      <p class="dialog-intro">Chọn khoảng ngày đặt, rồi chọn một đơn để xuất toàn bộ thông tin của đơn đó (người nhận, sản phẩm, giao hàng, thanh toán, lịch sử xử lý).</p>
      <DateRangeField v-model="range" />
      <InputText v-model="q" placeholder="Lọc theo mã đơn, số điện thoại, tên khách…" fluid autofocus />
      <DataTable
        v-model:selection="selected"
        :value="data.items"
        :loading="loading"
        selection-mode="single"
        :meta-key-selection="false"
        data-key="code"
        lazy
        paginator
        :rows="PAGE_SIZE"
        :total-records="data.total"
        :first="(page - 1) * PAGE_SIZE"
        size="small"
        row-hover
        style="cursor:pointer"
        @page="e => (page = e.page + 1)"
      >
        <template #empty><p class="muted" style="margin:12px 0">{{ ready ? 'Không có đơn nào trong khoảng ngày này.' : 'Chọn khoảng ngày để xem đơn.' }}</p></template>
        <Column selection-mode="single" style="width:44px" />
        <Column header="Mã đơn">
          <template #body="{ data: o }">
            <b class="mono">{{ o.code }}</b>
            <div class="muted" style="font-size:12px">{{ fmtDate(o.createdAt) }}</div>
          </template>
        </Column>
        <Column header="Khách hàng">
          <template #body="{ data: o }">
            {{ o.customerName }}
            <div class="muted" style="font-size:12px">{{ o.customerPhone }}</div>
          </template>
        </Column>
        <Column header="Tổng tiền" style="width:130px">
          <template #body="{ data: o }"><b>{{ fmt(o.total) }}</b></template>
        </Column>
        <Column header="Trạng thái" style="width:120px">
          <template #body="{ data: o }"><Tag :value="orderStatusOf(o.status).label" :severity="orderStatusOf(o.status).severity" /></template>
        </Column>
      </DataTable>
    </div>
    <template #footer>
      <Button label="Hủy" severity="secondary" text @click="visible = false" />
      <Button
        :label="selected ? `Xuất đơn ${selected.code}` : 'Chọn một đơn'"
        icon="pi pi-file-excel"
        :loading="busy"
        :disabled="!selected"
        @click="run"
      />
    </template>
  </Dialog>
</template>
