<script setup>
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import Button from 'primevue/button'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import { get, post, put, del } from '@/services/api'

const toast = useToast()
const confirm = useConfirm()
const categories = ref([])
const loading = ref(true)

async function load() {
  try {
    categories.value = await get('/admin/categories')
  } catch (err) {
    fail('Không tải được danh mục', err)
  } finally {
    loading.value = false
  }
}
onMounted(load)

const fail = (summary, err) => toast.add({ severity: 'error', summary, detail: err.message, life: 5000 })
const ok = summary => toast.add({ severity: 'success', summary, life: 2500 })

// Hộp thoại thêm/sửa: kind 'category' | 'type'; id null = thêm mới
const dialog = ref(null)
const groupsOf = c => [...new Set(c.types.map(t => t.group).filter(Boolean))]
const dialogGroups = computed(() => (dialog.value?.category ? groupsOf(dialog.value.category) : []))
const openCategory = c => (dialog.value = { kind: 'category', id: c?.id ?? null, name: c?.name ?? '', shortName: c?.shortName ?? '' })
const openType = (c, t) => (dialog.value = { kind: 'type', category: c, id: t?.id ?? null, name: t?.name ?? '', group: t?.group ?? null })

async function saveDialog() {
  const d = dialog.value
  try {
    if (d.kind === 'category') {
      const body = { name: d.name, shortName: d.shortName }
      d.id ? await put(`/admin/categories/${d.id}`, body) : await post('/admin/categories', body)
    } else {
      const body = { name: d.name, group: d.group || null }
      d.id ? await put(`/admin/types/${d.id}`, body) : await post(`/admin/categories/${d.category.id}/types`, body)
    }
    ok(d.id ? 'Đã lưu' : 'Đã thêm')
    dialog.value = null
    await load()
  } catch (err) {
    fail('Không lưu được', err)
  }
}

function remove(kind, item) {
  confirm.require({
    header: kind === 'category' ? 'Xóa danh mục' : 'Xóa loại sản phẩm',
    message: `Xóa "${item.name}"?`,
    icon: 'pi pi-exclamation-triangle',
    acceptProps: { label: 'Xóa', severity: 'danger' },
    rejectProps: { label: 'Hủy', severity: 'secondary', outlined: true },
    accept: async () => {
      try {
        await del(kind === 'category' ? `/admin/categories/${item.id}` : `/admin/types/${item.id}`)
        ok('Đã xóa')
        await load()
      } catch (err) {
        fail('Không xóa được', err)
      }
    },
  })
}

// Đổi chỗ với mục liền trước/sau rồi lưu thứ tự mới
async function move(list, i, d, url) {
  if (i + d < 0 || i + d >= list.length) return
  const ids = list.map(x => x.id)
  ;[ids[i], ids[i + d]] = [ids[i + d], ids[i]]
  try {
    await put(url, { ids })
    await load()
  } catch (err) {
    fail('Không đổi được thứ tự', err)
  }
}
</script>

<template>
  <div class="page-h">
    <div>
      <h1>Danh mục</h1>
      <p class="muted" style="margin:4px 0 0">Thứ tự ở đây là thứ tự trên menu site. Mã (slug) sinh từ tên lúc tạo và không đổi.</p>
    </div>
    <Button label="Thêm danh mục" icon="pi pi-plus" @click="openCategory(null)" />
  </div>
  <p v-if="loading" class="muted">Đang tải…</p>

  <section v-for="(c, ci) in categories" :key="c.id" class="box">
    <div class="cat-h">
      <h2>{{ c.name }}<small>{{ c.shortName }} · {{ c.id }}</small></h2>
      <div class="actions">
        <Button icon="pi pi-arrow-up" text rounded aria-label="Lên" :disabled="ci === 0" @click="move(categories, ci, -1, '/admin/categories/order')" />
        <Button icon="pi pi-arrow-down" text rounded aria-label="Xuống" :disabled="ci === categories.length - 1" @click="move(categories, ci, 1, '/admin/categories/order')" />
        <Button icon="pi pi-pencil" text rounded aria-label="Sửa danh mục" @click="openCategory(c)" />
        <Button icon="pi pi-trash" text rounded severity="danger" aria-label="Xóa danh mục" :disabled="c.types.length > 0"
                :title="c.types.length ? 'Còn loại sản phẩm — xóa hết loại trước' : 'Xóa danh mục'" @click="remove('category', c)" />
      </div>
    </div>
    <DataTable :value="c.types" size="small" data-key="id">
      <Column header="Loại sản phẩm">
        <template #body="{ data }">{{ data.name }} <span class="muted" style="font-size:12px">· {{ data.id }}</span></template>
      </Column>
      <Column field="group" header="Nhóm" style="width:220px">
        <template #body="{ data }">{{ data.group || '—' }}</template>
      </Column>
      <Column field="productCount" header="Sản phẩm" style="width:110px" />
      <Column header="" style="width:200px">
        <template #body="{ data, index }">
          <Button icon="pi pi-arrow-up" text rounded aria-label="Lên" :disabled="index === 0" @click="move(c.types, index, -1, `/admin/categories/${c.id}/types/order`)" />
          <Button icon="pi pi-arrow-down" text rounded aria-label="Xuống" :disabled="index === c.types.length - 1" @click="move(c.types, index, 1, `/admin/categories/${c.id}/types/order`)" />
          <Button icon="pi pi-pencil" text rounded aria-label="Sửa loại" @click="openType(c, data)" />
          <Button icon="pi pi-trash" text rounded severity="danger" aria-label="Xóa loại" :disabled="data.productCount > 0" @click="remove('type', data)" />
        </template>
      </Column>
    </DataTable>
    <Button label="Thêm loại" icon="pi pi-plus" text size="small" style="margin-top:8px" @click="openType(c, null)" />
  </section>

  <Dialog :visible="!!dialog" modal :header="dialog ? (dialog.id ? 'Sửa ' : 'Thêm ') + (dialog.kind === 'category' ? 'danh mục' : 'loại sản phẩm') : ''"
          style="width:min(440px, 92vw)" @update:visible="v => !v && (dialog = null)">
    <form v-if="dialog" class="rows" style="gap:14px" @submit.prevent="saveDialog">
      <div class="field">
        <label for="d-name">Tên</label>
        <InputText id="d-name" v-model="dialog.name" required maxlength="200" autofocus />
      </div>
      <div v-if="dialog.kind === 'category'" class="field">
        <label for="d-short">Tên ngắn (nút lọc)</label>
        <InputText id="d-short" v-model="dialog.shortName" required maxlength="100" />
      </div>
      <div v-else class="field">
        <label>Nhóm (không bắt buộc)</label>
        <Select v-model="dialog.group" :options="dialogGroups" editable show-clear placeholder="vd. Thiết bị đun nấu" />
        <small>Chỉ dùng khi danh mục chia nhóm trên menu (như Thiết bị bếp).</small>
      </div>
      <div class="actions" style="justify-content:flex-end">
        <Button label="Hủy" severity="secondary" text @click="dialog = null" />
        <Button type="submit" label="Lưu" icon="pi pi-check" />
      </div>
    </form>
  </Dialog>
</template>
