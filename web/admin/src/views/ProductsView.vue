<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import Button from 'primevue/button'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import ToggleSwitch from 'primevue/toggleswitch'
import { get, put, del } from '@/services/api'
import { fmt, fold, stockOf, onSale, discountOf, toProductRequest } from '@/utils/format'

const route = useRoute()
const router = useRouter()
const toast = useToast()
const confirm = useConfirm()

const STATUS = [
  { value: 'onsale', label: 'Đang bán trên site' },
  { value: 'noprice', label: 'Chưa có giá bán' },
  { value: 'hidden', label: 'Đang ẩn' },
  { value: 'LowStock', label: 'Sắp hết hàng' },
  { value: 'OutOfStock', label: 'Hết hàng' },
]
const MATCH = {
  onsale: onSale,
  noprice: p => p.price == null,
  hidden: p => !p.isPublished,
  LowStock: p => p.stockStatus === 'LowStock',
  OutOfStock: p => p.stockStatus === 'OutOfStock',
}

const products = ref([])
const categories = ref([])
const loading = ref(true)
// bộ lọc giữ trên URL (?q=&cat=&status=) để quay lại từ trang sửa vẫn còn
const q = ref(String(route.query.q || ''))
const cat = ref(route.query.cat ? String(route.query.cat) : null)
const status = ref(route.query.status ? String(route.query.status) : null)
watch([q, cat, status], () => {
  const query = Object.fromEntries(Object.entries({ q: q.value, cat: cat.value, status: status.value }).filter(([, v]) => v))
  router.replace({ query })
})

onMounted(async () => {
  try {
    ;[products.value, categories.value] = await Promise.all([get('/admin/products'), get('/admin/categories')])
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được sản phẩm', detail: err.message, life: 5000 })
  } finally {
    loading.value = false
  }
})

const rows = computed(() => {
  const term = fold(q.value.trim())
  return products.value.filter(p => (!cat.value || p.categoryId === cat.value)
    && (!status.value || MATCH[status.value](p))
    && (!term || fold(p.name).includes(term) || fold(p.model).includes(term) || fold(p.typeName).includes(term)))
})

// Bật/tắt hiển thị, trả góp ngay trên bảng
const TOGGLED = {
  isPublished: ['Đã hiện trên site', 'Đã ẩn khỏi site'],
  allowInstallment: ['Đã bật trả góp', 'Đã tắt trả góp'],
}
async function toggle(p, field, value) {
  const before = p[field]
  p[field] = value
  try {
    Object.assign(p, await put(`/admin/products/${p.id}`, toProductRequest(p)))
    toast.add({ severity: 'success', summary: TOGGLED[field][value ? 0 : 1], detail: p.model, life: 2500 })
  } catch (err) {
    p[field] = before
    toast.add({ severity: 'error', summary: 'Không lưu được', detail: err.message, life: 5000 })
  }
}

function remove(p) {
  confirm.require({
    header: 'Xóa sản phẩm',
    message: `Xóa hẳn ${p.model} — ${p.name}? Sản phẩm cũng bị gỡ khỏi các khoảnh khắc trang chủ.`,
    icon: 'pi pi-exclamation-triangle',
    acceptProps: { label: 'Xóa', severity: 'danger' },
    rejectProps: { label: 'Hủy', severity: 'secondary', outlined: true },
    accept: async () => {
      try {
        await del(`/admin/products/${p.id}`)
        products.value = products.value.filter(x => x.id !== p.id)
        toast.add({ severity: 'success', summary: 'Đã xóa', detail: p.model, life: 2500 })
      } catch (err) {
        toast.add({ severity: 'error', summary: 'Không xóa được', detail: err.message, life: 5000 })
      }
    },
  })
}
</script>

<template>
  <div class="page-h">
    <h1>Sản phẩm <span class="muted" style="font-size:16px;font-weight:400">({{ rows.length }}/{{ products.length }})</span></h1>
    <Button label="Thêm sản phẩm" icon="pi pi-plus" @click="router.push({ name: 'product-new' })" />
  </div>
  <div class="box">
    <div class="actions" style="margin-bottom:16px">
      <IconField style="flex:1;min-width:240px">
        <InputIcon class="pi pi-search" />
        <InputText v-model="q" placeholder="Tìm theo tên, mã, loại…" fluid />
      </IconField>
      <Select v-model="cat" :options="categories" option-label="name" option-value="id" placeholder="Mọi danh mục" show-clear style="width:220px" />
      <Select v-model="status" :options="STATUS" option-label="label" option-value="value" placeholder="Mọi trạng thái" show-clear style="width:220px" />
    </div>
    <DataTable
      :value="rows"
      :loading="loading"
      data-key="id"
      paginator
      :rows="20"
      :rows-per-page-options="[20, 50, 100]"
      size="small"
      sort-mode="single"
      striped-rows
    >
      <Column header="" style="width:64px">
        <template #body="{ data }">
          <img v-if="data.imageUrl" class="thumb" :src="data.imageUrl" alt="" loading="lazy">
          <span v-else class="thumb empty"><i class="pi pi-image" /></span>
        </template>
      </Column>
      <Column field="name" header="Sản phẩm" sortable>
        <template #body="{ data }">
          <RouterLink :to="{ name: 'product-edit', params: { id: data.id } }" style="font-weight:600">{{ data.name }}</RouterLink>
          <div class="muted" style="font-size:12px">{{ data.model }} · {{ data.typeName }}</div>
        </template>
      </Column>
      <Column field="price" header="Giá bán" sortable style="width:130px">
        <template #body="{ data }">
          <template v-if="data.price != null">
            {{ fmt(data.price) }}
            <Tag v-if="discountOf(data)" :value="`-${discountOf(data)}%`" severity="danger" style="font-size:11px;padding:1px 6px" />
          </template>
          <Tag v-else value="Chưa có giá" severity="warn" />
        </template>
      </Column>
      <Column field="dealerPrice" header="Giá nhập" sortable style="width:130px">
        <template #body="{ data }"><span class="muted">{{ fmt(data.dealerPrice) }}</span></template>
      </Column>
      <Column field="stockStatus" header="Kho" sortable style="width:120px">
        <template #body="{ data }">
          <Tag :value="stockOf(data.stockStatus).label" :severity="stockOf(data.stockStatus).severity" />
        </template>
      </Column>
      <Column header="Hiển thị" style="width:90px">
        <template #body="{ data }">
          <ToggleSwitch :model-value="data.isPublished" @update:model-value="v => toggle(data, 'isPublished', v)" />
        </template>
      </Column>
      <Column header="Trả góp" style="width:90px">
        <template #body="{ data }">
          <ToggleSwitch :model-value="data.allowInstallment" @update:model-value="v => toggle(data, 'allowInstallment', v)" />
        </template>
      </Column>
      <Column header="" style="width:100px">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" text rounded aria-label="Sửa" @click="router.push({ name: 'product-edit', params: { id: data.id } })" />
          <Button icon="pi pi-trash" text rounded severity="danger" aria-label="Xóa" @click="remove(data)" />
        </template>
      </Column>
    </DataTable>
  </div>
</template>
