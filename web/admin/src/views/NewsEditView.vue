<script setup>
import { ref, computed, watch, defineAsyncComponent } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import Textarea from 'primevue/textarea'
import ToggleSwitch from 'primevue/toggleswitch'
import ImageField from '@/components/ImageField.vue'
import { get, post, put, del } from '@/services/api'
import { fold, fmtDate } from '@/utils/format'

const RichEditor = defineAsyncComponent(() => import('@/components/RichEditor.vue'))

const props = defineProps({ id: { type: String, default: null } })
const route = useRoute()
const router = useRouter()
const toast = useToast()
const confirm = useConfirm()

const blank = () => ({ title: '', slug: '', categoryId: null, summary: '', coverImageUrl: null, html: '', isPublished: false })
const form = ref(blank())
const meta = ref(null)
const categories = ref([])
const loading = ref(true)
const saving = ref(false)

const toForm = p => {
  meta.value = { slug: p.slug, publishedAt: p.publishedAt, updatedAt: p.updatedAt, isPublished: p.isPublished }
  return { title: p.title, slug: p.slug, categoryId: p.categoryId, summary: p.summary ?? '', coverImageUrl: p.coverImageUrl, html: p.html, isPublished: p.isPublished }
}

watch(() => props.id, async id => {
  loading.value = true
  try {
    const [cats, p] = await Promise.all([get('/admin/news/categories'), id ? get(`/admin/news/posts/${id}`) : null])
    categories.value = cats
    if (p) {
      form.value = toForm(p)
    } else {
      meta.value = null
      form.value = { ...blank(), categoryId: cats.some(c => c.id === route.query.cat) ? route.query.cat : cats[0]?.id ?? null }
    }
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được bài viết', detail: err.message, life: 5000 })
    if (err.status === 404) router.replace({ name: 'news' })
  } finally {
    loading.value = false
  }
}, { immediate: true })

const autoSlug = computed(() => fold(form.value.title).replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, ''))
const noContent = computed(() => !form.value.html.trim())

async function save() {
  saving.value = true
  try {
    const body = { ...form.value, slug: form.value.slug.trim() || null, summary: form.value.summary.trim() || null }
    if (props.id) {
      form.value = toForm(await put(`/admin/news/posts/${props.id}`, body))
      toast.add({ severity: 'success', summary: form.value.isPublished ? 'Đã lưu, bài đang hiện trên site' : 'Đã lưu bản nháp', life: 2500 })
    } else {
      const created = await post('/admin/news/posts', body)
      toast.add({ severity: 'success', summary: created.isPublished ? 'Đã đăng bài' : 'Đã lưu bản nháp', life: 2500 })
      router.replace({ name: 'news-edit', params: { id: created.id } })
    }
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không lưu được', detail: err.message, life: 6000 })
  } finally {
    saving.value = false
  }
}

function remove() {
  confirm.require({
    header: 'Xóa bài viết',
    message: `Xóa hẳn "${form.value.title}"?`,
    icon: 'pi pi-exclamation-triangle',
    acceptProps: { label: 'Xóa', severity: 'danger' },
    rejectProps: { label: 'Hủy', severity: 'secondary', outlined: true },
    accept: async () => {
      try {
        await del(`/admin/news/posts/${props.id}`)
        toast.add({ severity: 'success', summary: 'Đã xóa bài viết', life: 2500 })
        router.replace({ name: 'news' })
      } catch (err) {
        toast.add({ severity: 'error', summary: 'Không xóa được', detail: err.message, life: 5000 })
      }
    },
  })
}
</script>

<template>
  <form v-if="!loading" @submit.prevent="save">
    <div class="page-h">
      <div>
        <RouterLink class="crumb" :to="{ name: 'news' }">← Tin tức</RouterLink>
        <h1>{{ id ? 'Sửa bài viết' : 'Viết bài mới' }}</h1>
      </div>
      <div class="actions">
        <Tag v-if="meta" :value="meta.isPublished ? 'Đang hiện trên site' : 'Bản nháp'" :severity="meta.isPublished ? 'success' : 'secondary'" />
        <Button v-if="id" label="Xóa" icon="pi pi-trash" severity="danger" outlined @click="remove" />
        <Button type="submit" :label="form.isPublished ? 'Lưu & đăng' : 'Lưu nháp'" icon="pi pi-check" :loading="saving"
                :disabled="!form.categoryId || (form.isPublished && noContent)" />
      </div>
    </div>

    <div class="edit-layout">
      <div>
        <section class="box">
          <div class="rows" style="gap:16px">
            <div class="field">
              <label for="title">Tiêu đề</label>
              <InputText id="title" v-model="form.title" required maxlength="300" placeholder="vd. 5 mẹo dùng bếp từ tiết kiệm điện" />
            </div>
            <div class="field">
              <label for="summary">Mô tả ngắn</label>
              <Textarea id="summary" v-model="form.summary" rows="3" maxlength="500" auto-resize />
              <small>Hiện ở danh sách bài và đầu bài viết. {{ form.summary.length }}/500</small>
            </div>
          </div>
        </section>
        <section class="box">
          <h2>Nội dung</h2>
          <RichEditor v-model="form.html" folder="news" />
          <small v-if="form.isPublished && noContent" style="color:#b91c1c">Bài đăng lên site phải có nội dung.</small>
        </section>
      </div>

      <aside>
        <section class="box">
          <h2>Đăng bài</h2>
          <div class="rows" style="gap:14px">
            <label class="check"><ToggleSwitch v-model="form.isPublished" />Hiện trên site</label>
            <div class="field">
              <label>Chuyên mục</label>
              <Select v-model="form.categoryId" :options="categories" option-label="name" option-value="id" placeholder="Chọn chuyên mục" />
            </div>
            <div class="field">
              <label for="slug">Đường dẫn</label>
              <InputText id="slug" v-model="form.slug" maxlength="200" :placeholder="autoSlug || 'tu-dong-theo-tieu-de'" />
              <small>/tin-tuc/bai-viet/{{ form.slug.trim() || autoSlug || '…' }}<template v-if="!form.slug.trim()"> · để trống thì tự sinh từ tiêu đề</template></small>
            </div>
            <small v-if="meta" class="muted">Đăng lúc {{ fmtDate(meta.publishedAt) }} · sửa lần cuối {{ fmtDate(meta.updatedAt) }}</small>
          </div>
        </section>
        <section class="box">
          <h2>Ảnh bìa</h2>
          <ImageField v-model="form.coverImageUrl" folder="news" wide />
          <small class="muted">Ảnh ngang 16:9 hiện ở danh sách bài và đầu bài viết.</small>
        </section>
      </aside>
    </div>
  </form>
</template>
