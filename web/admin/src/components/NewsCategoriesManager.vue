<script setup>
import { ref, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import Button from 'primevue/button'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import { get, post, put, del } from '@/services/api'

const toast = useToast()
const confirm = useConfirm()
const categories = ref([])
const loading = ref(true)

const fail = (summary, err) => toast.add({ severity: 'error', summary, detail: err.message, life: 5000 })
const ok = summary => toast.add({ severity: 'success', summary, life: 2500 })

async function load() {
  try {
    categories.value = await get('/admin/news/categories')
  } catch (err) {
    fail('Không tải được chuyên mục', err)
  } finally {
    loading.value = false
  }
}
onMounted(load)

const dialog = ref(null)
const open = c => (dialog.value = { id: c?.id ?? null, name: c?.name ?? '', description: c?.description ?? '' })

async function save() {
  const d = dialog.value
  const body = { name: d.name, description: d.description || null }
  try {
    d.id ? await put(`/admin/news/categories/${d.id}`, body) : await post('/admin/news/categories', body)
    ok(d.id ? 'Đã lưu' : 'Đã thêm chuyên mục')
    dialog.value = null
    await load()
  } catch (err) {
    fail('Không lưu được', err)
  }
}

function remove(c) {
  confirm.require({
    header: 'Xóa chuyên mục',
    message: `Xóa "${c.name}"?`,
    icon: 'pi pi-exclamation-triangle',
    acceptProps: { label: 'Xóa', severity: 'danger' },
    rejectProps: { label: 'Hủy', severity: 'secondary', outlined: true },
    accept: async () => {
      try {
        await del(`/admin/news/categories/${c.id}`)
        ok('Đã xóa')
        await load()
      } catch (err) {
        fail('Không xóa được', err)
      }
    },
  })
}

async function move(i, d) {
  const list = categories.value
  if (i + d < 0 || i + d >= list.length) return
  const ids = list.map(x => x.id)
  ;[ids[i], ids[i + d]] = [ids[i + d], ids[i]]
  try {
    await put('/admin/news/categories/order', { ids })
    await load()
  } catch (err) {
    fail('Không đổi được thứ tự', err)
  }
}
</script>

<template>
  <div class="box">
    <div class="actions" style="margin-bottom:16px;align-items:center;justify-content:space-between;flex-wrap:nowrap">
      <p class="muted" style="margin:0;font-size:13px">Thứ tự ở đây là thứ tự tab trên trang Tin tức. Mã (slug) sinh từ tên lúc tạo và không đổi. Chuyên mục chưa có bài đăng thì không hiện trên site.</p>
      <Button label="Thêm chuyên mục" icon="pi pi-plus" style="flex-shrink:0" @click="open(null)" />
    </div>
    <DataTable :value="categories" :loading="loading" data-key="id" size="small">
      <template #empty><span class="muted">Chưa có chuyên mục nào.</span></template>
      <Column header="Chuyên mục">
        <template #body="{ data }">
          <b>{{ data.name }}</b> <span class="muted" style="font-size:12px">· /tin-tuc/{{ data.id }}</span>
          <div v-if="data.description" class="muted" style="font-size:13px">{{ data.description }}</div>
        </template>
      </Column>
      <Column header="Bài viết" style="width:110px">
        <template #body="{ data }">
          <RouterLink :to="{ name: 'news', query: { cat: data.id } }">{{ data.postCount }}</RouterLink>
        </template>
      </Column>
      <Column header="" style="width:200px">
        <template #body="{ data, index }">
          <Button icon="pi pi-arrow-up" text rounded aria-label="Lên" :disabled="index === 0" @click="move(index, -1)" />
          <Button icon="pi pi-arrow-down" text rounded aria-label="Xuống" :disabled="index === categories.length - 1" @click="move(index, 1)" />
          <Button icon="pi pi-pencil" text rounded aria-label="Sửa chuyên mục" @click="open(data)" />
          <Button icon="pi pi-trash" text rounded severity="danger" aria-label="Xóa chuyên mục" :disabled="data.postCount > 0"
                  :title="data.postCount ? 'Còn bài viết — chuyển hoặc xóa hết bài trước' : 'Xóa chuyên mục'" @click="remove(data)" />
        </template>
      </Column>
    </DataTable>
  </div>

  <Dialog :visible="!!dialog" modal :header="dialog?.id ? 'Sửa chuyên mục' : 'Thêm chuyên mục'" style="width:min(460px, 92vw)"
          @update:visible="v => !v && (dialog = null)">
    <form v-if="dialog" class="rows" style="gap:14px" @submit.prevent="save">
      <div class="field">
        <label for="nc-name">Tên</label>
        <InputText id="nc-name" v-model="dialog.name" required maxlength="200" autofocus placeholder="vd. Mẹo hay nhà bếp" />
      </div>
      <div class="field">
        <label for="nc-desc">Mô tả ngắn (không bắt buộc)</label>
        <Textarea id="nc-desc" v-model="dialog.description" rows="3" maxlength="300" auto-resize />
        <small>Hiện dưới tiêu đề khi khách xem chuyên mục.</small>
      </div>
      <div class="actions" style="justify-content:flex-end">
        <Button label="Hủy" severity="secondary" text @click="dialog = null" />
        <Button type="submit" label="Lưu" icon="pi pi-check" />
      </div>
    </form>
  </Dialog>
</template>
