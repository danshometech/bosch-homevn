<script setup>
import { ref, reactive } from 'vue'
import { fmt, monthly } from '@/utils/format'
import { replay } from '@/utils/motion'
import { toDetail } from '@/router/links'
import { useCartStore } from '@/stores/cart'
import ImageBox from '@/components/ImageBox.vue'
import TweenNumber from '@/components/TweenNumber.vue'

const cart = useCartStore()
// Lấy thông tin các mã trong giỏ (GET /api/products?ids=…) trước khi hiện
const ready = ref(false)
cart.sync().finally(() => (ready.value = true))
const code = ref(cart.coupon)
const msg = ref('')
const msgOk = ref(false)
const msgN = ref(0)
const couponForm = ref(null)

const dirs = reactive({})
const qtyEls = {}

// Dòng phụ dưới tên: bảo hành + tồn kho, phần nào có dữ liệu mới hiện
const note = p => [p.warranty && 'Bảo hành ' + p.warranty, p.stock != null && 'Còn ' + p.stock].filter(Boolean).join(' · ')

function step(p, q, d) {
  if (p.stock != null && q > p.stock) return replay(qtyEls[p.id], 'shake')
  dirs[p.id] = d
  cart.setQty(p.id, q)
}

function apply() {
  msgOk.value = cart.applyCoupon(code.value)
  msg.value = msgOk.value ? 'Đã áp dụng mã giảm 10% (tối đa 500.000₫)' : 'Mã không hợp lệ. Thử BOSCH10'
  msgN.value++
  if (!msgOk.value) replay(couponForm.value, 'shake')
}
</script>

<template>
  <div v-if="!ready" class="wrap">
    <div class="box empty" style="margin-top:32px"><p class="muted">Đang tải giỏ hàng…</p></div>
  </div>
  <div v-else-if="!cart.lines.length" class="wrap">
    <div class="box empty rise" style="margin-top:32px">
      <h2>Giỏ hàng đang trống</h2>
      <p class="muted" style="margin:8px 0 24px">Khám phá các ưu đãi flash sale hôm nay.</p>
      <RouterLink class="btn btn-primary" to="/">Tiếp tục mua sắm</RouterLink>
    </div>
  </div>
  <div v-else class="wrap">
    <div class="crumb"><RouterLink to="/">Trang chủ</RouterLink>/<span>Giỏ hàng</span></div>
    <h1 style="font-size:28px;margin-bottom:20px">Giỏ hàng <span class="muted" style="font-weight:400;font-size:18px">({{ cart.count }})</span></h1>
    <div class="cart">
      <TransitionGroup name="row" tag="div" class="box" style="padding:6px 24px">
        <div v-for="{ p, q } in cart.lines" :key="p.id" class="ci">
          <ImageBox :to="toDetail(p.id)" :src="p.img" :alt="p.name" :label="p.sub.toLowerCase()" />
          <div>
            <h4><RouterLink :to="toDetail(p.id)">{{ p.name }}</RouterLink></h4>
            <div v-if="note(p)" class="muted" style="font-size:13px">{{ note(p) }}</div>
            <button class="rm" @click="cart.remove(p.id)">Xóa</button>
          </div>
          <div style="text-align:right;display:flex;flex-direction:column;align-items:flex-end;gap:10px">
            <div>
              <b style="color:var(--accent)"><TweenNumber :value="p.price * q" /></b>
              <div v-if="p.old"><s class="muted tn" style="font-size:12px">{{ fmt(p.old * q) }}</s></div>
            </div>
            <div :ref="el => (qtyEls[p.id] = el)" class="qty">
              <button aria-label="Giảm" @click="step(p, q - 1, -1)">−</button>
              <span class="qty-n" aria-live="polite"><span :key="q" :class="{ 'roll-up': dirs[p.id] > 0, 'roll-down': dirs[p.id] < 0 }">{{ q }}</span></span>
              <button aria-label="Tăng" @click="step(p, q + 1, 1)">+</button>
            </div>
          </div>
        </div>
      </TransitionGroup>
      <div class="box" style="position:sticky;top:140px">
        <h3>Tóm tắt đơn hàng</h3>
        <div class="sum-row"><span>Tạm tính</span><TweenNumber :value="cart.subtotal" /></div>
        <div class="sum-row"><span>Tiết kiệm</span><span style="color:var(--ok-ink)">−<TweenNumber :value="cart.savings" /></span></div>
        <div class="reveal" :class="{ open: cart.discount > 0 }">
          <div><div class="sum-row"><span>Mã giảm giá</span><span style="color:var(--ok-ink)">−<TweenNumber :value="cart.discount" /></span></div></div>
        </div>
        <div class="sum-row"><span>Giao hàng &amp; lắp đặt</span><span>Miễn phí</span></div>
        <form ref="couponForm" class="coupon" @submit.prevent="apply">
          <input v-model="code" :class="{ good: msgN && msgOk, bad: msgN && !msgOk }" placeholder="Mã giảm giá" aria-label="Mã giảm giá">
          <button type="submit" class="btn btn-ghost btn-sm" style="height:40px">Áp dụng</button>
        </form>
        <div v-if="msg" :key="msgN" class="coupon-msg" role="status" :style="{ color: msgOk ? 'var(--ok-ink)' : 'var(--accent)' }">{{ msg }}</div>
        <div class="sum-row tot"><span>Tổng cộng</span><b><TweenNumber :value="cart.total" /></b></div>
        <div class="muted" style="font-size:12px;text-align:right;margin-bottom:16px">Đã bao gồm VAT</div>
        <RouterLink class="btn btn-primary btn-block" :to="{ name: 'checkout' }">Tiến hành thanh toán</RouterLink>
        <div class="reveal" :class="{ open: cart.installable }">
          <div>
            <RouterLink class="btn btn-ghost btn-block" style="margin-top:8px" :to="{ name: 'checkout', query: { inst: '1' } }">
              <span>Trả góp 0% · <TweenNumber :value="monthly(cart.total, cart.installShow)" />/tháng</span>
            </RouterLink>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
