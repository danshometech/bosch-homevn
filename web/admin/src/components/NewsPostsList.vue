<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import { get } from '@/services/api'
import { fold, fmtDate } from '@/utils/format'

const route = useRoute()
const router = useRouter()
const toast = useToast()

const STATUS = [
  { value: 'published', label: 'Đã đăng' },
  { value: 'draft', label: 'Bản nháp' },
]

const posts = ref([])
const categories = ref([])
const loading = ref(true)
const q = ref(String(route.query.q || ''))
const cat = ref(route.query.cat ? String(route.query.cat) : null)
const status = ref(route.query.status ? String(route.query.status) : null)
watch([q, cat, status], () => {
  const query = Object.fromEntries(Object.entries({ q: q.value, cat: cat.value, status: status.value }).filter(([, v]) => v))
  router.replace({ query })
})

onMounted(async () => {
  try {
    ;[posts.value, categories.value] = await Promise.all([get('/admin/news/posts'), get('/admin/news/categories')])
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được bài viết', detail: err.message, life: 5000 })
  } finally {
    loading.value = false
  }
})

const rows = computed(() => {
  const term = fold(q.value.trim())
  return posts.value.filter(p => (!cat.value || p.categoryId === cat.value)
    && (!status.value || (status.value === 'published') === p.isPublished)
    && (!term || fold(p.title).includes(term) || p.slug.includes(term)))
})
</script>

<template>
  <div v-if="!loading && !categories.length" class="box" style="margin-bottom:16px">
    Chưa có chuyên mục nào — <RouterLink :to="{ name: 'news', query: { tab: 'chuyen-muc' } }" style="font-weight:600">tạo chuyên mục</RouterLink> trước rồi mới viết bài.
  </div>
  <div class="box">
    <div class="actions" style="margin-bottom:16px;align-items:center">
      <IconField style="flex:1;min-width:240px">
        <InputIcon class="pi pi-search" />
        <InputText v-model="q" placeholder="Tìm theo tiêu đề…" fluid />
      </IconField>
      <Select v-model="cat" :options="categories" option-label="name" option-value="id" placeholder="Mọi chuyên mục" show-clear style="width:220px" />
      <Select v-model="status" :options="STATUS" option-label="label" option-value="value" placeholder="Mọi trạng thái" show-clear style="width:180px" />
      <span class="muted" style="font-size:13px">{{ rows.length }}/{{ posts.length }} bài</span>
      <Button label="Viết bài" icon="pi pi-plus" :disabled="!loading && !categories.length" @click="router.push({ name: 'news-new', query: cat ? { cat } : {} })" />
    </div>
    <DataTable :value="rows" :loading="loading" data-key="id" paginator :rows="20" :rows-per-page-options="[20, 50, 100]" size="small" striped-rows>
      <template #empty><span class="muted">Chưa có bài viết.</span></template>
      <Column header="" style="width:88px">
        <template #body="{ data }">
          <img v-if="data.coverImageUrl" class="thumb wide" :src="data.coverImageUrl" alt="" loading="lazy">
          <span v-else class="thumb wide empty"><i class="pi pi-image" /></span>
        </template>
      </Column>
      <Column field="title" header="Bài viết" sortable>
        <template #body="{ data }">
          <RouterLink :to="{ name: 'news-edit', params: { id: data.id } }" style="font-weight:600">{{ data.title }}</RouterLink>
          <div class="muted" style="font-size:12px">/tin-tuc/bai-viet/{{ data.slug }}</div>
        </template>
      </Column>
      <Column field="categoryName" header="Chuyên mục" sortable style="width:180px" />
      <Column field="isPublished" header="Trạng thái" sortable style="width:120px">
        <template #body="{ data }">
          <Tag :value="data.isPublished ? 'Đã đăng' : 'Bản nháp'" :severity="data.isPublished ? 'success' : 'secondary'" />
        </template>
      </Column>
      <Column field="publishedAt" header="Ngày đăng" sortable style="width:150px">
        <template #body="{ data }"><span class="muted">{{ fmtDate(data.publishedAt) }}</span></template>
      </Column>
      <Column field="updatedAt" header="Sửa lần cuối" sortable style="width:150px">
        <template #body="{ data }"><span class="muted">{{ fmtDate(data.updatedAt) }}</span></template>
      </Column>
    </DataTable>
  </div>
</template>
