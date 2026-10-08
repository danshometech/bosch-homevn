<script setup>
import { ref, onMounted } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import { get } from '@/services/api'
import { fmt } from '@/utils/format'

const d = ref(null)
const error = ref('')
onMounted(async () => {
  try {
    d.value = await get('/admin/dashboard')
  } catch (err) {
    error.value = err.message
  }
})
</script>

<template>
  <div class="page-h"><h1>Tổng quan</h1></div>
  <p v-if="error" class="muted">{{ error }}</p>
  <template v-if="d">
    <div class="stats">
      <RouterLink class="stat" :to="{ name: 'products' }"><b>{{ d.total }}</b><span>Sản phẩm</span></RouterLink>
      <RouterLink class="stat" :to="{ name: 'products', query: { status: 'onsale' } }"><b>{{ d.onSale }}</b><span>Đang bán trên site</span></RouterLink>
      <RouterLink class="stat warn" :to="{ name: 'products', query: { status: 'noprice' } }"><b>{{ d.noPrice }}</b><span>Chưa có giá bán</span></RouterLink>
      <RouterLink class="stat" :to="{ name: 'products', query: { status: 'hidden' } }"><b>{{ d.hidden }}</b><span>Đang ẩn</span></RouterLink>
      <RouterLink class="stat warn" :to="{ name: 'products', query: { status: 'LowStock' } }"><b>{{ d.lowStock }}</b><span>Sắp hết hàng</span></RouterLink>
      <RouterLink class="stat danger" :to="{ name: 'products', query: { status: 'OutOfStock' } }"><b>{{ d.outOfStock }}</b><span>Hết hàng</span></RouterLink>
    </div>

    <div class="box">
      <h2>Theo danh mục</h2>
      <DataTable :value="d.categories" size="small">
        <Column field="name" header="Danh mục" />
        <Column field="total" header="Sản phẩm" style="width:140px" />
        <Column field="onSale" header="Đang bán" style="width:140px" />
      </DataTable>
    </div>

    <div class="box">
      <h2>Hết hàng ({{ d.outOfStockProducts.length }})</h2>
      <DataTable :value="d.outOfStockProducts" size="small">
        <Column header="Sản phẩm">
          <template #body="{ data }">
            <RouterLink :to="{ name: 'product-edit', params: { id: data.id } }">{{ data.name }}</RouterLink>
            <div class="muted" style="font-size:12px">{{ data.model }}</div>
          </template>
        </Column>
        <Column header="Ghi chú hàng về" style="width:220px">
          <template #body="{ data }">{{ data.stockNote || '—' }}</template>
        </Column>
        <Column header="Giá bán" style="width:140px">
          <template #body="{ data }">{{ fmt(data.price) }}</template>
        </Column>
        <Column header="" style="width:110px">
          <template #body="{ data }"><Tag v-if="!data.isPublished" value="Đang ẩn" severity="secondary" /></template>
        </Column>
      </DataTable>
    </div>
  </template>
</template>
