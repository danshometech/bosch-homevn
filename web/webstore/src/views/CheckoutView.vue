<script setup>
import { ref, reactive, computed, onMounted, onBeforeUnmount } from 'vue'
import Select from 'primevue/select'
import { fmt, monthly } from '@/utils/format'
import { replay } from '@/utils/motion'
import { useCartStore } from '@/stores/cart'
import { createOrder } from '@/services/orders'
import { getProvinces, getWards } from '@/services/address'
import { getCheckoutOptions, savedCheckoutOptions } from '@/services/checkout'
import AppIcon from '@/components/AppIcon.vue'
import TweenNumber from '@/components/TweenNumber.vue'

const props = defineProps({ inst: { type: Boolean, default: false } })
const cart = useCartStore()
// Trang hiện ngay: form không phụ thuộc giỏ; tóm tắt đơn chờ thông tin sản phẩm (GET /api/products?ids=…),
// cách giao / thanh toán hiện bản lần trước rồi thay bằng bản mới. Vào thẳng ?inst=1 thì chọn sẵn trả góp
const ready = ref(false)
const opts = ref(savedCheckoutOptions() ?? { shipping: [], payment: [] })
const optsFailed = ref(false)
const optsLoading = ref(true)
async function loadOptions() {
  optsLoading.value = true
  try {
    opts.value = await getCheckoutOptions()
    optsFailed.value = false
  } catch (e) {
    console.error(e)
    optsFailed.value = !opts.value.shipping.length
  } finally {
    optsLoading.value = false
    pickDefaults()
  }
}
// Cách thanh toán mặc định cần biết giỏ có trả góp không → chờ giỏ sync xong
function pickDefaults() {
  if (!opts.value.shipping.some(s => s.id === ship.value)) ship.value = opts.value.shipping[0]?.id ?? null
  if (!ready.value || payments.value.some(m => m.id === pay.value)) return
  const inst = props.inst && payments.value.find(m => m.isInstallment)
  pay.value = (inst || payments.value.find(m => !m.isInstallment))?.id ?? null
}
let freshOpts = loadOptions()
const retryOptions = () => (freshOpts = loadOptions())
cart.sync().finally(() => {
  months.value = cart.installShow
  ready.value = true
  pickDefaults()
})
const lineCount = computed(() => Object.keys(cart.items).length)

const CONTACT = [
  { k: 'name', label: 'Họ và tên *', placeholder: 'Nguyễn Văn A', autocomplete: 'name' },
  { k: 'phone', label: 'Số điện thoại *', placeholder: '09xx xxx xxx', autocomplete: 'tel', inputmode: 'tel' },
  { k: 'email', label: 'Email (nhận hóa đơn)', placeholder: 'email@vidu.com', autocomplete: 'email', inputmode: 'email', full: true },
]
const DETAIL = [
  { k: 'addr', label: 'Số nhà, tên đường *', placeholder: 'VD: 12 Nguyễn Huệ, tòa nhà ABC', autocomplete: 'address-line1', full: true },
  { k: 'note', label: 'Ghi chú', placeholder: 'VD: giao giờ hành chính, cần lắp đặt âm tủ…', full: true, textarea: true },
]
// Ô chọn tỉnh / phường có tìm kiếm (PrimeVue Select unstyled, CSS .asel trong styles.css)
const SELECT_PT = {
  root: ({ state, props }) => ({ class: ['asel', { open: state.overlayVisible, disabled: props.disabled }] }),
  label: ({ props }) => ({ class: ['asel-label', { 'is-ph': props.modelValue == null }] }),
  dropdown: { class: 'asel-icon' },
  overlay: { class: 'sel-panel asel-panel' },
  header: { class: 'asel-head' },
  pcFilterContainer: { root: { class: 'asel-search' } },
  pcFilter: { root: { class: 'asel-filter' } },
  pcFilterIconContainer: { root: { class: 'asel-search-ic' } },
  listContainer: { class: 'asel-scroll' },
  list: { class: 'sel-list' },
  option: ({ context }) => ({ class: ['sel-opt', { on: context.selected, focus: context.focused }] }),
  emptyMessage: { class: 'asel-empty' },
  transition: { name: 'menu' },
}
const f = reactive({ name: '', phone: '', email: '', province: null, ward: null, area: '', addr: '', note: '' })

// Tỉnh / thành → phường / xã (danh mục 2025) lấy từ API; không tải được thì nhập tay vào ô `area`
const provinces = ref([])
const wards = ref([])
const provLoading = ref(true)
const wardsLoading = ref(false)
const addrFailed = ref(false)
onMounted(() => {
  getProvinces()
    .then(list => (provinces.value = list))
    .catch(e => {
      console.error(e)
      addrFailed.value = true
    })
    .finally(() => (provLoading.value = false))
})
async function onProvince(code) {
  clearErr('province')
  f.ward = null
  wards.value = []
  if (!code) return
  wardsLoading.value = true
  try {
    const list = await getWards(code)
    if (f.province === code) wards.value = list
  } catch (e) {
    console.error(e)
    addrFailed.value = true
  } finally {
    wardsLoading.value = false
  }
}
const nameOf = (list, code) => list.find(x => x.code === code)?.name ?? ''
const area = computed(() => (addrFailed.value ? f.area.trim() : [nameOf(wards.value, f.ward), nameOf(provinces.value, f.province)].filter(Boolean).join(', ')))
const ship = ref(opts.value.shipping[0]?.id ?? null)
const pay = ref(null)
const months = ref(cart.installShow)
const err = ref({})
const done = ref(null)
const submitting = ref(false)
const ctrls = {}
const sels = {}

const shipFee = computed(() => opts.value.shipping.find(s => s.id === ship.value)?.fee ?? 0)
const grand = computed(() => cart.total + shipFee.value)
// Cách trả góp chỉ hiện khi giỏ có sản phẩm cho trả góp
const payments = computed(() => opts.value.payment.filter(m => !m.isInstallment || cart.installable))
const isInst = computed(() => !!payments.value.find(m => m.id === pay.value)?.isInstallment)

// Panel ô chọn gắn vào body, vị trí tính lúc mở; PrimeVue chỉ tự đóng khi resize trên máy không cảm ứng → tự căn lại
let alignFrame = 0
function realign() {
  cancelAnimationFrame(alignFrame)
  alignFrame = requestAnimationFrame(() => Object.values(sels).forEach(s => s?.overlayVisible && s.alignOverlay()))
}
onMounted(() => window.addEventListener('resize', realign))
onBeforeUnmount(() => {
  window.removeEventListener('resize', realign)
  cancelAnimationFrame(alignFrame)
})

function clearErr(k) {
  if (!err.value[k]) return
  const { [k]: _, ...rest } = err.value
  err.value = rest
}

function validate() {
  const e = {}
  if (!f.name.trim()) e.name = 'Vui lòng nhập họ tên'
  if (!/^0\d{9}$/.test(f.phone.replace(/\s/g, ''))) e.phone = 'Số điện thoại gồm 10 số, bắt đầu bằng 0'
  if (addrFailed.value) {
    if (!f.area.trim()) e.area = 'Vui lòng nhập phường / xã và tỉnh / thành phố'
  } else {
    if (!f.province) e.province = 'Vui lòng chọn tỉnh / thành phố'
    else if (!f.ward) e.ward = 'Vui lòng chọn phường / xã'
  }
  if (!f.addr.trim()) e.addr = 'Vui lòng nhập số nhà, tên đường'
  if (!ship.value) e.ship = 'Vui lòng chọn cách giao hàng'
  if (!pay.value) e.pay = 'Vui lòng chọn cách thanh toán'
  err.value = e
  Object.keys(e).forEach(k => replay(ctrls[k], 'shake'))
  return !Object.keys(e).length
}

async function submit() {
  if (submitting.value || !ready.value) return
  submitting.value = true
  // Đặt theo phí mới nhất, không theo bản lưu lần trước
  await freshOpts
  if (!validate()) {
    submitting.value = false
    return
  }
  try {
    const order = await createOrder({
      customer: {
        name: f.name.trim(),
        phone: f.phone.replace(/\s/g, ''),
        email: f.email.trim(),
        province: addrFailed.value ? null : nameOf(provinces.value, f.province),
        ward: addrFailed.value ? null : nameOf(wards.value, f.ward),
        address: [f.addr.trim(), area.value].filter(Boolean).join(', '),
        note: f.note.trim(),
      },
      items: cart.lines.map(({ p, q }) => ({ id: p.id, qty: q, price: p.price })),
      shipping: ship.value,
      payment: pay.value,
      months: isInst.value ? months.value : null,
      total: grand.value,
    })
    done.value = { id: order.id, total: grand.value, name: f.name, phone: f.phone }
    cart.clear()
    window.scrollTo(0, 0)
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div v-if="done" class="wrap">
    <div class="success">
      <div class="ok"><AppIcon name="check" :size="34" class="draw" /></div>
      <h1 class="rise" style="font-size:30px;animation-delay:220ms">Đặt hàng thành công</h1>
      <p class="muted rise" style="margin:10px 0 24px;animation-delay:300ms">Cảm ơn {{ done.name }}! Nhân viên sẽ gọi xác nhận qua số {{ done.phone }} trong 15 phút.</p>
      <div class="box rise" style="text-align:left;margin-bottom:24px;animation-delay:380ms">
        <div class="sum-row"><span>Mã đơn hàng</span><b class="mono">{{ done.id }}</b></div>
        <div class="sum-row"><span>Tổng thanh toán</span><b class="tn">{{ fmt(done.total) }}</b></div>
        <div class="sum-row"><span>Bảo hành</span><span>Kích hoạt tự động khi giao hàng</span></div>
      </div>
      <RouterLink class="btn btn-primary rise" style="animation-delay:460ms" to="/">Tiếp tục mua sắm</RouterLink>
    </div>
  </div>

  <div v-else-if="ready ? !cart.lines.length : !lineCount" class="wrap">
    <div class="box empty rise" style="margin-top:32px">
      <h2>Chưa có sản phẩm để thanh toán</h2>
      <RouterLink class="btn btn-primary" style="margin-top:20px" to="/">Về trang chủ</RouterLink>
    </div>
  </div>

  <div v-else class="wrap">
    <div class="crumb">
      <RouterLink to="/">Trang chủ</RouterLink>/<RouterLink :to="{ name: 'cart' }">Giỏ hàng</RouterLink>/<span>Thanh toán</span>
    </div>
    <div class="steps">
      <span class="st"><i>1</i>Giỏ hàng</span><span class="ln" />
      <span class="st on"><i>2</i>Thông tin &amp; thanh toán</span><span class="ln" />
      <span class="st"><i>3</i>Hoàn tất</span>
    </div>
    <div class="cart">
      <div style="display:flex;flex-direction:column;gap:16px">
        <div class="box">
          <h3>Thông tin người nhận</h3>
          <div class="form-g">
            <label v-for="x in CONTACT" :key="x.k" class="fld" :class="{ full: x.full, err: err[x.k] }">
              {{ x.label }}
              <input
                :ref="el => (ctrls[x.k] = el)"
                v-model="f[x.k]"
                :placeholder="x.placeholder"
                :autocomplete="x.autocomplete"
                :inputmode="x.inputmode"
                :aria-invalid="!!err[x.k]"
                @input="clearErr(x.k)"
              >
              <Transition name="err">
                <span v-if="err[x.k]" class="e">{{ err[x.k] }}</span>
              </Transition>
            </label>

            <template v-if="!addrFailed">
              <div class="fld" :class="{ err: err.province }">
                <span id="lbl-province">Tỉnh / Thành phố *</span>
                <Select
                  :ref="el => { sels.province = el; ctrls.province = el?.$el }"
                  v-model="f.province"
                  :options="provinces"
                  option-label="name"
                  option-value="code"
                  filter
                  :filter-fields="['name', 'search']"
                  filter-placeholder="Tìm tỉnh / thành phố…"
                  :placeholder="provLoading ? 'Đang tải…' : 'Chọn tỉnh / thành phố'"
                  :disabled="provLoading"
                  empty-filter-message="Không tìm thấy"
                  aria-labelledby="lbl-province"
                  :pt="SELECT_PT"
                  @update:model-value="onProvince"
                >
                  <template #dropdownicon><AppIcon name="chevron" :size="16" /></template>
                  <template #option="{ option, selected }">{{ option.name }}<AppIcon v-if="selected" name="check" :size="16" /></template>
                </Select>
                <Transition name="err">
                  <span v-if="err.province" class="e">{{ err.province }}</span>
                </Transition>
              </div>
              <div class="fld" :class="{ err: err.ward }">
                <span id="lbl-ward">Phường / Xã *</span>
                <Select
                  :ref="el => { sels.ward = el; ctrls.ward = el?.$el }"
                  v-model="f.ward"
                  :options="wards"
                  option-label="name"
                  option-value="code"
                  filter
                  :filter-fields="['name', 'search']"
                  filter-placeholder="Tìm phường / xã…"
                  :placeholder="wardsLoading ? 'Đang tải…' : f.province ? 'Chọn phường / xã' : 'Chọn tỉnh / thành phố trước'"
                  :disabled="!f.province || wardsLoading"
                  empty-filter-message="Không tìm thấy"
                  aria-labelledby="lbl-ward"
                  :pt="SELECT_PT"
                  @update:model-value="clearErr('ward')"
                >
                  <template #dropdownicon><AppIcon name="chevron" :size="16" /></template>
                  <template #option="{ option, selected }">{{ option.name }}<AppIcon v-if="selected" name="check" :size="16" /></template>
                </Select>
                <Transition name="err">
                  <span v-if="err.ward" class="e">{{ err.ward }}</span>
                </Transition>
              </div>
            </template>
            <label v-else class="fld full" :class="{ err: err.area }">
              Phường / Xã, Tỉnh / Thành phố *
              <input
                :ref="el => (ctrls.area = el)"
                v-model="f.area"
                placeholder="VD: Phường Bến Nghé, TP. Hồ Chí Minh"
                autocomplete="address-level1"
                :aria-invalid="!!err.area"
                @input="clearErr('area')"
              >
              <small class="muted" style="font-weight:400">Chưa tải được danh sách địa chỉ, vui lòng nhập tay.</small>
              <Transition name="err">
                <span v-if="err.area" class="e">{{ err.area }}</span>
              </Transition>
            </label>

            <label v-for="x in DETAIL" :key="x.k" class="fld" :class="{ full: x.full, err: err[x.k] }">
              {{ x.label }}
              <textarea v-if="x.textarea" v-model="f[x.k]" :placeholder="x.placeholder" />
              <input
                v-else
                :ref="el => (ctrls[x.k] = el)"
                v-model="f[x.k]"
                :placeholder="x.placeholder"
                :autocomplete="x.autocomplete"
                :aria-invalid="!!err[x.k]"
                @input="clearErr(x.k)"
              >
              <Transition name="err">
                <span v-if="err[x.k]" class="e">{{ err[x.k] }}</span>
              </Transition>
            </label>
          </div>
        </div>

        <div v-if="optsFailed" class="box">
          <h3>Giao hàng &amp; thanh toán</h3>
          <p class="muted" style="margin:0 0 12px">Chưa tải được cách giao hàng và thanh toán.</p>
          <button type="button" class="btn btn-ghost" :disabled="optsLoading" @click="retryOptions">
            <AppIcon v-if="optsLoading" name="spinner" :size="16" class="spin" />Thử lại
          </button>
        </div>

        <template v-if="!opts.shipping.length && optsLoading">
          <div v-for="[title, n] in [['Giao hàng', 3], ['Phương thức thanh toán', 4]]" :key="title" class="box" aria-busy="true">
            <h3>{{ title }}</h3>
            <div v-for="i in n" :key="i" class="opt sk-opt">
              <span class="sk sk-dot" />
              <span class="sk-stack"><span class="sk sk-line" style="width:42%" /><span class="sk sk-line sk-sm" style="width:64%" /></span>
            </div>
          </div>
        </template>

        <div v-if="opts.shipping.length" :ref="el => (ctrls.ship = el)" class="box">
          <h3>Giao hàng</h3>
          <label v-for="s in opts.shipping" :key="s.id" class="opt" :class="{ on: ship === s.id }">
            <input v-model="ship" type="radio" name="ship" :value="s.id" @change="clearErr('ship')">
            <div><b>{{ s.name }}</b><small v-if="s.description">{{ s.description }}</small></div>
            <span class="r">{{ s.fee ? fmt(s.fee) : 'Miễn phí' }}</span>
          </label>
        </div>

        <div v-if="payments.length" :ref="el => (ctrls.pay = el)" class="box">
          <h3>Phương thức thanh toán</h3>
          <template v-for="m in payments" :key="m.id">
            <label class="opt" :class="{ on: pay === m.id }">
              <input v-model="pay" type="radio" name="pay" :value="m.id" @change="clearErr('pay')">
              <div><b>{{ m.name }}</b><small v-if="m.description">{{ m.description }}</small></div>
              <span v-if="m.isInstallment" class="tag" style="margin-left:auto;align-self:center">0%</span>
            </label>
            <div v-if="m.isInstallment" class="reveal" :class="{ open: pay === m.id }">
              <div>
                <div class="inst-plans">
                  <button
                    v-for="(n, i) in cart.installMonths"
                    :key="n"
                    type="button"
                    :class="{ on: months === n }"
                    :aria-pressed="months === n"
                    :style="{ animationDelay: i * 45 + 'ms' }"
                    @click="months = n"
                  ><b>{{ n }} tháng</b><span class="tn">{{ fmt(monthly(grand, n)) }}</span>/th</button>
                </div>
              </div>
            </div>
          </template>
        </div>
      </div>

      <div class="box" style="position:sticky;top:140px">
        <h3>Đơn hàng ({{ cart.count }})</h3>
        <template v-if="ready">
          <div v-for="{ p, q } in cart.lines" :key="p.id" class="mini">
            <div class="ph"><img v-if="p.img" :src="p.img" alt=""><span v-else style="font-size:9px">ảnh</span><span class="q">{{ q }}</span></div>
            <div style="flex:1;line-height:1.35">{{ p.name }}</div>
            <b style="white-space:nowrap">{{ fmt(p.price * q) }}</b>
          </div>
          <div style="border-top:1px solid var(--line);margin-top:10px;padding-top:10px">
            <div class="sum-row"><span>Tạm tính</span><span class="tn">{{ fmt(cart.subtotal) }}</span></div>
            <div class="sum-row"><span>Phí giao hàng</span><span :key="shipFee" class="swap tn">{{ shipFee ? fmt(shipFee) : 'Miễn phí' }}</span></div>
            <div class="sum-row tot"><span>Tổng cộng</span><b><TweenNumber :value="grand" /></b></div>
            <div class="reveal" :class="{ open: isInst }">
              <div><div class="sum-row"><span>Trả góp {{ months }} tháng</span><b><TweenNumber :value="monthly(grand, months)" />/th</b></div></div>
            </div>
          </div>
        </template>
        <div v-else aria-busy="true">
          <div v-for="i in Math.min(lineCount, 3)" :key="i" class="mini">
            <span class="sk sk-mini" />
            <span class="sk-stack"><span class="sk sk-line" /><span class="sk sk-line" style="width:55%" /></span>
          </div>
          <div style="border-top:1px solid var(--line);margin-top:10px;padding-top:10px">
            <div v-for="w in [30, 40, 50]" :key="w" class="sum-row"><span class="sk sk-line" :style="{ width: w + '%' }" /><span class="sk sk-line" style="width:28%" /></div>
          </div>
        </div>
        <button class="btn btn-primary btn-block" style="margin-top:16px" :disabled="!ready || submitting || optsFailed" @click="submit">
          <AppIcon v-if="submitting" name="spinner" :size="18" class="spin" />
          {{ submitting ? 'Đang gửi đơn…' : 'Đặt hàng' }}
        </button>
        <p class="muted" style="font-size:12px;margin-top:10px;text-align:center">Bằng việc đặt hàng, bạn đồng ý với điều khoản của bosch-homevn.com</p>
      </div>
    </div>
  </div>
</template>
