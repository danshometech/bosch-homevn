<script setup>
import { ref, reactive, computed } from 'vue'
import { fmt, monthly } from '@/utils/format'
import { replay } from '@/utils/motion'
import { useCartStore } from '@/stores/cart'
import { createOrder } from '@/services/orders'
import AppIcon from '@/components/AppIcon.vue'
import TweenNumber from '@/components/TweenNumber.vue'

const props = defineProps({ inst: { type: Boolean, default: false } })
const cart = useCartStore()
// Lấy thông tin các mã trong giỏ (GET /api/products?ids=…) trước khi hiện; vào thẳng ?inst=1 thì chọn sẵn trả góp
const ready = ref(false)
cart.sync().finally(() => {
  months.value = cart.installShow
  if (props.inst && cart.installable) pay.value = 'inst'
  ready.value = true
})

const CITIES = ['TP. Hồ Chí Minh', 'Hà Nội', 'Đà Nẵng', 'Hải Phòng', 'Cần Thơ', 'Tỉnh khác']
const FIELDS = [
  { k: 'name', label: 'Họ và tên *', placeholder: 'Nguyễn Văn A', autocomplete: 'name' },
  { k: 'phone', label: 'Số điện thoại *', placeholder: '09xx xxx xxx', autocomplete: 'tel', inputmode: 'tel' },
  { k: 'email', label: 'Email (nhận hóa đơn)', placeholder: 'email@vidu.com', autocomplete: 'email', inputmode: 'email' },
  { k: 'city', label: 'Tỉnh / Thành phố', options: CITIES },
  { k: 'addr', label: 'Địa chỉ cụ thể *', placeholder: 'Số nhà, đường, phường/xã', autocomplete: 'street-address', full: true },
  { k: 'note', label: 'Ghi chú', placeholder: 'VD: giao giờ hành chính, cần lắp đặt âm tủ…', full: true, textarea: true },
]
const SHIPPING = [
  { k: 'std', title: 'Giao tiêu chuẩn + lắp đặt', desc: '2–3 ngày · Kỹ thuật viên lắp đặt tận nơi', fee: 0 },
  { k: 'fast', title: 'Giao nhanh trong 24h', desc: 'Áp dụng nội thành HN, HCM, ĐN', fee: 50000 },
  { k: 'pick', title: 'Nhận tại showroom', desc: '12 showroom toàn quốc', fee: 0 },
]

const f = reactive({ name: '', phone: '', email: '', city: CITIES[0], addr: '', note: '' })
const ship = ref('std')
const pay = ref(props.inst && cart.installable ? 'inst' : 'cod')
const months = ref(cart.installShow)
const err = ref({})
const done = ref(null)
const submitting = ref(false)
const ctrls = {}

const shipFee = computed(() => SHIPPING.find(s => s.k === ship.value).fee)
const grand = computed(() => cart.total + shipFee.value)
const payments = computed(() => [
  { k: 'cod', title: 'Thanh toán khi nhận hàng (COD)', desc: 'Kiểm tra hàng trước khi thanh toán' },
  { k: 'bank', title: 'Chuyển khoản / QR ngân hàng', desc: 'VietQR · Xác nhận tự động' },
  { k: 'wallet', title: 'Ví điện tử', desc: 'MoMo, ZaloPay, VNPay' },
  { k: 'card', title: 'Thẻ quốc tế', desc: 'Visa, Mastercard, JCB' },
  ...(cart.installable ? [{ k: 'inst', title: 'Trả góp 0% qua thẻ tín dụng', desc: '25 ngân hàng · Không cần trả trước' }] : []),
])

function clearErr(k) {
  if (!err.value[k]) return
  const { [k]: _, ...rest } = err.value
  err.value = rest
}

function validate() {
  const e = {}
  if (!f.name.trim()) e.name = 'Vui lòng nhập họ tên'
  if (!/^0\d{9}$/.test(f.phone.replace(/\s/g, ''))) e.phone = 'Số điện thoại gồm 10 số, bắt đầu bằng 0'
  if (!f.addr.trim()) e.addr = 'Vui lòng nhập địa chỉ'
  err.value = e
  Object.keys(e).forEach(k => replay(ctrls[k], 'shake'))
  return !Object.keys(e).length
}

async function submit() {
  if (submitting.value || !validate()) return
  submitting.value = true
  try {
    const order = await createOrder({
      customer: { ...f, phone: f.phone.replace(/\s/g, '') },
      items: cart.lines.map(({ p, q }) => ({ id: p.id, qty: q, price: p.price })),
      shipping: ship.value,
      payment: pay.value,
      months: pay.value === 'inst' ? months.value : null,
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

  <div v-else-if="!ready" class="wrap">
    <div class="box empty" style="margin-top:32px"><p class="muted">Đang tải đơn hàng…</p></div>
  </div>

  <div v-else-if="!cart.lines.length" class="wrap">
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
            <label v-for="x in FIELDS" :key="x.k" class="fld" :class="{ full: x.full, err: err[x.k] }">
              {{ x.label }}
              <select v-if="x.options" v-model="f[x.k]">
                <option v-for="o in x.options" :key="o">{{ o }}</option>
              </select>
              <textarea v-else-if="x.textarea" v-model="f[x.k]" :placeholder="x.placeholder" />
              <input
                v-else
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
          </div>
        </div>

        <div class="box">
          <h3>Giao hàng</h3>
          <label v-for="s in SHIPPING" :key="s.k" class="opt" :class="{ on: ship === s.k }">
            <input v-model="ship" type="radio" name="ship" :value="s.k">
            <div><b>{{ s.title }}</b><small>{{ s.desc }}</small></div>
            <span class="r">{{ s.fee ? fmt(s.fee) : 'Miễn phí' }}</span>
          </label>
        </div>

        <div class="box">
          <h3>Phương thức thanh toán</h3>
          <template v-for="m in payments" :key="m.k">
            <label class="opt" :class="{ on: pay === m.k }">
              <input v-model="pay" type="radio" name="pay" :value="m.k">
              <div><b>{{ m.title }}</b><small>{{ m.desc }}</small></div>
              <span v-if="m.k === 'inst'" class="tag" style="margin-left:auto;align-self:center">0%</span>
            </label>
            <div v-if="m.k === 'inst'" class="reveal" :class="{ open: pay === 'inst' }">
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
        <div v-for="{ p, q } in cart.lines" :key="p.id" class="mini">
          <div class="ph"><img v-if="p.img" :src="p.img" alt=""><span v-else style="font-size:9px">ảnh</span><span class="q">{{ q }}</span></div>
          <div style="flex:1;line-height:1.35">{{ p.name }}</div>
          <b style="white-space:nowrap">{{ fmt(p.price * q) }}</b>
        </div>
        <div style="border-top:1px solid var(--line);margin-top:10px;padding-top:10px">
          <div class="sum-row"><span>Tạm tính</span><span class="tn">{{ fmt(cart.subtotal) }}</span></div>
          <div class="sum-row"><span>Phí giao hàng</span><span :key="shipFee" class="swap tn">{{ shipFee ? fmt(shipFee) : 'Miễn phí' }}</span></div>
          <div class="sum-row tot"><span>Tổng cộng</span><b><TweenNumber :value="grand" /></b></div>
          <div class="reveal" :class="{ open: pay === 'inst' }">
            <div><div class="sum-row"><span>Trả góp {{ months }} tháng</span><b><TweenNumber :value="monthly(grand, months)" />/th</b></div></div>
          </div>
        </div>
        <button class="btn btn-primary btn-block" style="margin-top:16px" :disabled="submitting" @click="submit">
          <AppIcon v-if="submitting" name="spinner" :size="18" class="spin" />
          {{ submitting ? 'Đang gửi đơn…' : 'Đặt hàng' }}
        </button>
        <p class="muted" style="font-size:12px;margin-top:10px;text-align:center">Bằng việc đặt hàng, bạn đồng ý với điều khoản của bosch-homevn.com</p>
      </div>
    </div>
  </div>
</template>
