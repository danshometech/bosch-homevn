<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import { get } from '@/services/api'
import { TONES, onSale } from '@/utils/format'

const router = useRouter()
const toast = useToast()
const moments = ref([])
const products = ref(new Map())
const loading = ref(true)

onMounted(async () => {
  try {
    const [m, p] = await Promise.all([get('/admin/moments'), get('/admin/products')])
    moments.value = m
    products.value = new Map(p.map(x => [x.id, x]))
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được khoảnh khắc', detail: err.message, life: 5000 })
  } finally {
    loading.value = false
  }
})

// Trang chủ chỉ hiện khoảnh khắc có ít nhất một sản phẩm hàng chính đang bán
const shown = m => m.productIds.some(id => products.value.get(id) && onSale(products.value.get(id)))
</script>

<template>
  <div class="page-h">
    <div>
      <h1>Khoảnh khắc trang chủ</h1>
      <p class="muted" style="margin:4px 0 0">"A Day with Bosch" — mỗi khoảnh khắc một ảnh bối cảnh, lời dẫn và một hàng sản phẩm.</p>
    </div>
  </div>
  <div class="box">
    <DataTable :value="moments" :loading="loading" data-key="id" size="small" row-hover
               @row-click="e => router.push({ name: 'moment-edit', params: { id: e.data.id } })" style="cursor:pointer">
      <Column header="Giờ" style="width:90px">
        <template #body="{ data }"><b style="font-size:18px">{{ data.time }}</b></template>
      </Column>
      <Column header="Khoảnh khắc">
        <template #body="{ data }">
          <span class="tone-dot" :style="{ background: TONES[data.tone]?.color }" />
          <b>{{ data.name }}</b>
          <div class="muted" style="font-size:13px">{{ data.title }}</div>
        </template>
      </Column>
      <Column header="Sản phẩm" style="width:110px">
        <template #body="{ data }">{{ data.productIds.length }}</template>
      </Column>
      <Column header="Mua kèm" style="width:110px">
        <template #body="{ data }">{{ data.extraProductIds.length }}</template>
      </Column>
      <Column header="Trên trang chủ" style="width:170px">
        <template #body="{ data }">
          <Tag :value="shown(data) ? 'Đang hiện' : 'Đang ẩn — chưa có sản phẩm'" :severity="shown(data) ? 'success' : 'secondary'" />
        </template>
      </Column>
    </DataTable>
  </div>
</template>
