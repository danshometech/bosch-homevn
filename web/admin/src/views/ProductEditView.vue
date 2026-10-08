<script setup>
import { ref, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import Button from 'primevue/button'
import Checkbox from 'primevue/checkbox'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import ToggleSwitch from 'primevue/toggleswitch'
import ImageField from '@/components/ImageField.vue'
import GalleryField from '@/components/GalleryField.vue'
import VideoField from '@/components/VideoField.vue'
import { get, post, put, del } from '@/services/api'
import { fmt, STOCK, onSale, discountOf, INSTALLMENT_MONTHS, monthly } from '@/utils/format'

// Không có id = thêm mới
const props = defineProps({ id: { type: String, default: null } })
const router = useRouter()
const toast = useToast()
const confirm = useConfirm()

const blank = () => ({
  model: '', name: '', typeId: null, series: null,
  price: null, oldPrice: null, dealerPrice: null,
  stockStatus: 'InStock', stockQuantity: null, stockNote: null,
  color: null, origin: null, warranty: 'BSH - 3 năm',
  isNew: false, isFlashSale: false, isPublished: true,
  imageUrl: null, galleryImages: [], videoUrl: null, videoPosterUrl: null, highlights: [], specs: [],
  allowInstallment: false, installmentMonths: [3, 6, 9, 12], installmentDisplayMonths: 12,
})
// Sản phẩm chưa từng cấu hình trả góp thì điền sẵn kỳ hạn mặc định để bật là dùng được ngay
const toForm = p => {
  const f = { ...blank(), ...p }
  f.galleryImages ??= []
  if (!f.installmentMonths.length) Object.assign(f, { installmentMonths: [3, 6, 9, 12], installmentDisplayMonths: 12 })
  return f
}
const form = ref(blank())
const categories = ref([])
const loading = ref(true)
const saving = ref(false)

watch(() => props.id, async id => {
  loading.value = true
  try {
    const [cats, product] = await Promise.all([get('/admin/categories'), id ? get(`/admin/products/${id}`) : null])
    categories.value = cats
    form.value = product ? toForm(product) : blank()
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được sản phẩm', detail: err.message, life: 5000 })
    if (err.status === 404) router.replace({ name: 'products' })
  } finally {
    loading.value = false
  }
}, { immediate: true })

// Loại sản phẩm gom theo danh mục cho ô chọn
const typeGroups = computed(() => categories.value.map(c => ({
  label: c.name,
  items: c.types.map(t => ({ value: t.id, label: t.group ? `${t.name} · ${t.group}` : t.name })),
})))
// Nhãn "Giảm X%" trên site tính từ giá gạch và giá bán. Nhập % thì giữ giá bán, tự tính giá gạch (làm tròn nghìn);
// xóa ô % thì bỏ giá gạch (hết nhãn giảm giá)
const discount = computed({
  get: () => discountOf(form.value),
  set: pct => {
    const f = form.value
    if (!pct) f.oldPrice = null
    else if (f.price) f.oldPrice = Math.round(f.price / (1 - pct / 100) / 1000) * 1000
  },
})
// Bỏ tick kỳ hạn đang dùng để hiển thị thì chuyển sang kỳ hạn dài nhất còn lại
const instMonths = computed(() => [...form.value.installmentMonths].sort((a, b) => a - b))
const instOptions = computed(() => instMonths.value.map(n => ({ value: n, label: `${n} tháng` })))
watch(instMonths, list => {
  if (list.length && !list.includes(form.value.installmentDisplayMonths)) form.value.installmentDisplayMonths = list.at(-1)
})
const instMissing = computed(() => form.value.allowInstallment && !instMonths.value.length)
const oldPriceTooLow = computed(() => form.value.oldPrice != null && form.value.price != null && form.value.oldPrice < form.value.price)
const margin = computed(() => {
  const { price, dealerPrice } = form.value
  return price != null && dealerPrice ? Math.round(((price - dealerPrice) / dealerPrice) * 1000) / 10 : null
})

// Dòng trang chi tiết tự thêm vào cuối bảng thông số (giống DetailView của site): màu / xuất xứ / bảo hành ở mục
// Thông tin nếu bảng chưa có dòng cùng tên, và thương hiệu
const autoSpecs = computed(() => {
  const names = new Set(form.value.specs.map(s => s.name.trim()))
  const f = form.value
  const rows = [['Màu sắc', f.color, 'mục Thông tin'], ['Xuất xứ', f.origin, 'mục Thông tin'], ['Bảo hành', f.warranty, 'mục Thông tin']]
  return [...rows.filter(([k, v]) => v && !names.has(k)), ['Thương hiệu', 'Bosch', 'luôn có']]
})

const addSpec = () => form.value.specs.push({ name: '', value: '' })
function moveSpec(i, d) {
  const s = form.value.specs
  if (i + d < 0 || i + d >= s.length) return
  ;[s[i], s[i + d]] = [s[i + d], s[i]]
}

async function save() {
  saving.value = true
  try {
    const body = { ...form.value, highlights: form.value.highlights.filter(h => h && h.trim()), installmentMonths: instMonths.value }
    if (props.id) {
      form.value = toForm(await put(`/admin/products/${props.id}`, body))
      toast.add({ severity: 'success', summary: 'Đã lưu', detail: form.value.model, life: 2500 })
    } else {
      const created = await post('/admin/products', body)
      toast.add({ severity: 'success', summary: 'Đã thêm sản phẩm', detail: created.model, life: 2500 })
      router.replace({ name: 'product-edit', params: { id: created.id } })
    }
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không lưu được', detail: err.message, life: 6000 })
  } finally {
    saving.value = false
  }
}

function remove() {
  confirm.require({
    header: 'Xóa sản phẩm',
    message: `Xóa hẳn ${form.value.model}? Sản phẩm cũng bị gỡ khỏi các khoảnh khắc trang chủ.`,
    icon: 'pi pi-exclamation-triangle',
    acceptProps: { label: 'Xóa', severity: 'danger' },
    rejectProps: { label: 'Hủy', severity: 'secondary', outlined: true },
    accept: async () => {
      try {
        await del(`/admin/products/${props.id}`)
        toast.add({ severity: 'success', summary: 'Đã xóa', detail: form.value.model, life: 2500 })
        router.replace({ name: 'products' })
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
        <RouterLink class="crumb" :to="{ name: 'products' }">← Sản phẩm</RouterLink>
        <h1>{{ id ? form.model : 'Thêm sản phẩm' }}</h1>
      </div>
      <div class="actions">
        <Tag v-if="id" :value="onSale(form) ? 'Đang bán trên site' : 'Không hiện trên site'" :severity="onSale(form) ? 'success' : 'secondary'" />
        <Button v-if="id" label="Xóa" icon="pi pi-trash" severity="danger" outlined @click="remove" />
        <Button type="submit" label="Lưu" icon="pi pi-check" :loading="saving" :disabled="instMissing" />
      </div>
    </div>

    <div class="edit-layout">
      <div>
        <section class="box">
          <h2>Thông tin</h2>
          <div class="form-grid">
            <div class="field">
              <label for="model">Mã model</label>
              <InputText id="model" v-model="form.model" :disabled="!!id" required maxlength="32" />
              <small v-if="!id">Không đổi được sau khi tạo.</small>
            </div>
            <div class="field wide">
              <label for="name">Tên sản phẩm</label>
              <InputText id="name" v-model="form.name" required maxlength="300" />
            </div>
            <div class="field">
              <label>Loại sản phẩm</label>
              <Select v-model="form.typeId" :options="typeGroups" option-group-label="label" option-group-children="items"
                      option-label="label" option-value="value" filter placeholder="Chọn loại" />
            </div>
            <div class="field">
              <label for="series">Series</label>
              <InputNumber v-model="form.series" input-id="series" :min="1" :max="9" placeholder="2 / 4 / 6 / 8" />
            </div>
            <div class="field"><label for="color">Màu sắc</label><InputText id="color" v-model="form.color" maxlength="100" /></div>
            <div class="field"><label for="origin">Xuất xứ</label><InputText id="origin" v-model="form.origin" maxlength="100" /></div>
            <div class="field"><label for="warranty">Bảo hành</label><InputText id="warranty" v-model="form.warranty" maxlength="100" /></div>
          </div>
        </section>

        <section class="box">
          <h2>Giá</h2>
          <div class="form-grid">
            <div class="field">
              <label for="price">Giá bán</label>
              <InputNumber v-model="form.price" input-id="price" mode="currency" currency="VND" locale="vi-VN" :min="0" />
              <small>Để trống = chưa bán trên site.</small>
            </div>
            <div class="field">
              <label for="discount">Giảm (%)</label>
              <InputNumber v-model="discount" input-id="discount" :min="0" :max="90" suffix="%" :disabled="form.price == null" placeholder="Không giảm" />
              <small>{{ form.price == null ? 'Nhập giá bán trước.' : 'Nhập % để tự tính giá gạch.' }}</small>
            </div>
            <div class="field">
              <label for="oldPrice">Giá gạch</label>
              <InputNumber v-model="form.oldPrice" input-id="oldPrice" mode="currency" currency="VND" locale="vi-VN" :min="0" :invalid="oldPriceTooLow" />
              <small v-if="oldPriceTooLow" style="color:#b91c1c">Thấp hơn giá bán — sẽ không lưu được.</small>
              <small v-else>Giá trước giảm, hiện gạch ngang trên site.</small>
            </div>
            <div class="field">
              <label for="dealerPrice">Giá nhập đại lý</label>
              <InputNumber v-model="form.dealerPrice" input-id="dealerPrice" mode="currency" currency="VND" locale="vi-VN" :min="0" />
              <small>Chỉ hiện trong trang quản trị.<template v-if="margin != null"> Lãi {{ margin }}% ({{ fmt(form.price - form.dealerPrice) }}).</template></small>
            </div>
          </div>
          <p class="muted" style="margin:12px 0 0;font-size:13px">
            Trên site: <b v-if="discount" style="color:#b91c1c">nhãn "Giảm {{ discount }}%"</b><template v-else>không có nhãn giảm giá</template>
            <template v-if="form.isNew"> · nhãn "Mới"</template><template v-if="form.isFlashSale"> · Flash sale</template>
            <template v-if="form.allowInstallment"> · Trả góp 0%</template>
          </p>
        </section>

        <section class="box">
          <h2>Trả góp 0%</h2>
          <label class="check"><ToggleSwitch v-model="form.allowInstallment" />Cho phép trả góp</label>
          <template v-if="form.allowInstallment">
            <div class="form-grid" style="margin-top:16px">
              <div class="field wide">
                <label>Kỳ hạn khách chọn khi thanh toán</label>
                <div class="checks">
                  <label v-for="n in INSTALLMENT_MONTHS" :key="n" class="check"><Checkbox v-model="form.installmentMonths" :value="n" />{{ n }} tháng</label>
                </div>
                <small v-if="instMissing" style="color:#b91c1c">Chọn ít nhất một kỳ hạn.</small>
              </div>
              <div class="field">
                <label>Kỳ hạn hiển thị</label>
                <Select v-model="form.installmentDisplayMonths" :options="instOptions" option-label="label" option-value="value" :disabled="instMissing" />
                <small>Dùng tính "chỉ X/tháng" trên thẻ và trang chi tiết.</small>
              </div>
            </div>
            <p class="muted" style="margin:12px 0 0;font-size:13px">
              <template v-if="form.price && !instMissing">
                Trên site: "Trả góp 0% - 0đ trả trước - chỉ <b>{{ fmt(monthly(form.price, form.installmentDisplayMonths)) }}</b>/tháng"
              </template>
              <template v-else-if="!form.price">Nhập giá bán để xem số tiền mỗi tháng.</template>
            </p>
          </template>
        </section>

        <section class="box">
          <h2>Kho hàng</h2>
          <div class="form-grid">
            <div class="field">
              <label>Tình trạng</label>
              <Select v-model="form.stockStatus" :options="STOCK" option-label="label" option-value="value" />
            </div>
            <div class="field">
              <label for="qty">Số lượng tồn</label>
              <InputNumber v-model="form.stockQuantity" input-id="qty" :min="0" placeholder="Không theo dõi" />
            </div>
            <div class="field wide">
              <label for="note">Ghi chú hàng về</label>
              <InputText id="note" v-model="form.stockNote" placeholder="vd. Cuối tháng 09 có hàng" maxlength="200" />
            </div>
          </div>
        </section>

        <section class="box">
          <h2>Thông số kỹ thuật</h2>
          <div class="rows">
            <div v-for="(s, i) in form.specs" :key="i" class="row">
              <InputText v-model="s.name" placeholder="Tên (vd. Dung tích)" style="max-width:260px" maxlength="100" />
              <InputText v-model="s.value" placeholder="Giá trị (vd. 71 L)" maxlength="300" />
              <Button icon="pi pi-arrow-up" text rounded aria-label="Lên" :disabled="i === 0" @click="moveSpec(i, -1)" />
              <Button icon="pi pi-arrow-down" text rounded aria-label="Xuống" :disabled="i === form.specs.length - 1" @click="moveSpec(i, 1)" />
              <Button icon="pi pi-times" text rounded severity="danger" aria-label="Bỏ dòng" @click="form.specs.splice(i, 1)" />
            </div>
          </div>
          <Button label="Thêm dòng" icon="pi pi-plus" text size="small" style="margin-top:8px" @click="addSpec" />
          <div class="auto-specs">
            <small class="muted">Trang chi tiết tự thêm vào cuối bảng:</small>
            <div v-for="[k, v, from] in autoSpecs" :key="k" class="row">
              <InputText :model-value="k" disabled style="max-width:260px" />
              <InputText :model-value="v" disabled />
              <small class="muted auto-from">{{ from }}</small>
            </div>
          </div>
        </section>
      </div>

      <aside>
        <section class="box">
          <h2>Hiển thị</h2>
          <div class="rows" style="gap:12px">
            <label class="check"><ToggleSwitch v-model="form.isPublished" />Hiện trên site</label>
            <label class="check"><Checkbox v-model="form.isNew" binary />Gắn nhãn "Mới"</label>
            <label class="check"><Checkbox v-model="form.isFlashSale" binary />Flash sale</label>
          </div>
        </section>
        <section class="box">
          <h2>Ảnh &amp; video</h2>
          <ImageField v-model="form.imageUrl" folder="products" />
          <h3 class="sub-h">Ảnh thêm ({{ form.galleryImages.length }}/4)
            <small class="muted">Trang chi tiết hiện ảnh chính + các ảnh này thành tối đa 5 ảnh nhỏ dưới ảnh lớn.</small>
          </h3>
          <GalleryField v-model="form.galleryImages" :max="4" />
          <h3 class="sub-h">Video</h3>
          <VideoField v-model="form.videoUrl" v-model:poster="form.videoPosterUrl" />
        </section>
        <section class="box">
          <h2>3 điểm nổi bật</h2>
          <div class="rows">
            <InputText v-for="i in 3" :key="i" v-model="form.highlights[i - 1]" :placeholder="['vd. 13 bộ', 'vd. 44 dB', 'vd. Hạng D'][i - 1]" maxlength="40" />
          </div>
          <small class="muted">Hiện thành chip trên thẻ sản phẩm và trang chi tiết.</small>
        </section>
      </aside>
    </div>
  </form>
</template>
