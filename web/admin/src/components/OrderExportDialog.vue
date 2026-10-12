<script setup>
import { ref, computed, watch } from 'vue'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import DateRangeField from './DateRangeField.vue'
import { get, download } from '@/services/api'
import { addDays, isoDate, startOfToday } from '@/utils/format'

// Xuất file Excel các đơn đặt trong một khoảng ngày (giờ Việt Nam, mọi trạng thái), mỗi dòng sản phẩm một hàng.
// Đếm trước số đơn trong khoảng đang chọn: không có đơn nào thì không cho xuất file rỗng
const visible = defineModel('visible', { type: Boolean, default: false })
const toast = useToast()
const range = ref([addDays(startOfToday(), -29), startOfToday()])
const busy = ref(false)
// Số đơn trong khoảng: null = đang đếm, -1 = không đếm được (vẫn cho xuất)
const count = ref(null)

const ready = computed(() => !!range.value?.[0] && !!range.value?.[1])
const label = computed(() => (ready.value ? `${range.value[0].toLocaleDateString('vi-VN')} – ${range.value[1].toLocaleDateString('vi-VN')}` : ''))
const params = () => range.value.map(isoDate)

let request = 0
watch([visible, range], async () => {
  if (!visible.value || !ready.value) return
  const req = ++request
  count.value = null
  try {
    const [from, to] = params()
    const res = await get(`/admin/orders?from=${from}&to=${to}&pageSize=1`)
    if (req === request) count.value = res.total
  } catch {
    if (req === request) count.value = -1
  }
}, { immediate: true })

async function run() {
  busy.value = true
  try {
    const [from, to] = params()
    await download(`/admin/orders/export?from=${from}&to=${to}`, `danh-sach-don-hang-${from.replaceAll('-', '')}-${to.replaceAll('-', '')}.xlsx`)
    visible.value = false
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không xuất được file Excel', detail: err.message, life: 5000 })
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <Dialog v-model:visible="visible" modal header="Xuất Excel đơn hàng" :style="{ width: 'min(480px, 94vw)' }">
    <div class="dialog-body">
      <!-- autofocus vào đoạn mô tả: Dialog không tự focus nút X (vòng focus to) -->
      <p class="dialog-intro" tabindex="-1" autofocus>Xuất mọi đơn đặt trong khoảng ngày (theo giờ Việt Nam, tất cả trạng thái) ra file Excel (.xlsx), mỗi dòng sản phẩm một hàng.</p>
      <DateRangeField v-model="range" />
      <p v-if="ready && count !== -1" class="export-note" :class="{ empty: count === 0 }">
        <template v-if="count == null"><i class="pi pi-spin pi-spinner" style="font-size:13px" />Đang đếm đơn…</template>
        <template v-else-if="count"><i class="pi pi-file-excel" />Có <b>{{ count }}</b> đơn trong khoảng này.</template>
        <template v-else><i class="pi pi-info-circle" />Không có đơn nào được đặt trong khoảng này.</template>
      </p>
    </div>
    <template #footer>
      <Button label="Hủy" severity="secondary" text @click="visible = false" />
      <Button :label="ready ? `Xuất ${label}` : 'Xuất Excel'" icon="pi pi-file-excel" :loading="busy" :disabled="!ready || count === 0 || count == null" @click="run" />
    </template>
  </Dialog>
</template>
