<script setup>
import { ref, reactive, computed, watch } from 'vue'
import { fmt, monthly } from '@/utils/format'
import { replay } from '@/utils/motion'
import { toDetail } from '@/router/links'
import { useCartStore } from '@/stores/cart'
import { useCatalogStore } from '@/stores/catalog'
import { useViewedStore } from '@/stores/viewed'
import ImageBox from '@/components/ImageBox.vue'
import ProductCard from '@/components/ProductCard.vue'
import TweenNumber from '@/components/TweenNumber.vue'

const cart = useCartStore()
const catalog = useCatalogStore()
const viewed = useViewedStore()
// Lấy thông tin các mã trong giỏ (GET /api/products?ids=…) trước khi hiện
const ready = ref(false)
cart.sync().finally(() => (ready.value = true))

const dirs = reactive({})
const qtyEls = {}

// Dòng phụ dưới tên: bảo hành + tồn kho, phần nào có dữ liệu mới hiện
const note = p => [p.warranty && 'Bảo hành ' + p.warranty, p.stock != null && 'Còn ' + p.stock].filter(Boolean).join(' · ')

function step(p, q, d) {
  if (p.stock != null && q > p.stock) return replay(qtyEls[p.id], 'shake')
  dirs[p.id] = d
  cart.setQty(p.id, q)
}

// "Sản phẩm đã xem" (lịch sử xem, trừ hàng trong giỏ) và "Sản phẩm liên quan": cùng danh mục với hàng trong giỏ,
// ưu tiên loại khác (giỏ có bếp từ thì gợi ý hút mùi, lò…), không lặp hàng trong giỏ / đã xem. Giỏ đổi thì tính lại
const seen = ref([])
const related = ref([])
const inCart = computed(() => cart.lines.map(l => l.p.id))
let request = 0
watch([ready, () => inCart.value.join()], async ([ok]) => {
  if (!ok) return
  const req = ++request
  try {
    const ids = inCart.value
    const recent = await viewed.recent(ids)
    if (req !== request) return
    seen.value = recent
    const cats = [...new Set(cart.lines.map(l => l.p.cat))].slice(0, 3)
    const lists = await Promise.all(cats.map(category => catalog.fetchProducts({ category, take: 60 })))
    if (req !== request) return
    const skip = new Set([...ids, ...recent.map(p => p.id)])
    const subs = new Set(cart.lines.map(l => l.p.sub))
    const bySub = new Map()
    for (const p of lists.flat()) {
      if (skip.has(p.id)) continue
      skip.add(p.id)
      if (!bySub.has(p.sub)) bySub.set(p.sub, [])
      bySub.get(p.sub).push(p)
    }
    // xoay vòng mỗi loại một sản phẩm, loại chưa có trong giỏ trước
    const groups = [...bySub].sort(([a], [b]) => subs.has(a) - subs.has(b)).map(([, list]) => list)
    const picked = []
    for (let i = 0; picked.length < 4 && groups.some(g => g[i]); i++) {
      for (const g of groups) if (g[i] && picked.length < 4) picked.push(g[i])
    }
    related.value = picked
  } catch (err) {
    console.error(err)
  }
}, { immediate: true })
</script>

<template>
  <div class="wrap">
    <div v-if="!ready" class="box empty" style="margin-top:32px"><p class="muted">Đang tải giỏ hàng…</p></div>
    <div v-else-if="!cart.lines.length" class="box empty rise" style="margin-top:32px">
      <h2>Giỏ hàng đang trống</h2>
      <p class="muted" style="margin:8px 0 24px">Khám phá các ưu đãi flash sale hôm nay.</p>
      <RouterLink class="btn btn-primary" to="/">Tiếp tục mua sắm</RouterLink>
    </div>
    <template v-else>
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
          <div class="sum-row"><span>Giao hàng &amp; lắp đặt</span><span>Miễn phí</span></div>
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
    </template>

    <section v-if="ready && related.length" class="sec" style="padding-bottom:0">
      <div class="sec-h"><h2>Sản phẩm liên quan</h2></div>
      <div class="grid">
        <ProductCard v-for="x in related" :key="x.id" :p="x" />
      </div>
    </section>
    <section v-if="ready && seen.length" class="sec">
      <div class="sec-h"><h2>Sản phẩm đã xem</h2></div>
      <div class="grid">
        <ProductCard v-for="x in seen" :key="x.id" :p="x" />
      </div>
    </section>
  </div>
</template>
