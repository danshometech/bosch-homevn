<script setup>
import { ref, reactive, computed, watch, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { fmt, pct, monthly, warrantyOf } from '@/utils/format'
import { replay } from '@/utils/motion'
import { toList } from '@/router/links'
import { setTitle } from '@/router'
import { useCartStore } from '@/stores/cart'
import { useCatalogStore } from '@/stores/catalog'
import { useCompareStore } from '@/stores/compare'
import AppIcon from '@/components/AppIcon.vue'
import ImageBox from '@/components/ImageBox.vue'
import StarRating from '@/components/StarRating.vue'
import ProductCard from '@/components/ProductCard.vue'
import ProductCardSkeleton from '@/components/ProductCardSkeleton.vue'
import ProductDetailSkeleton from '@/components/ProductDetailSkeleton.vue'

const props = defineProps({ id: { type: String, required: true } })
const router = useRouter()
const catalog = useCatalogStore()
const cart = useCartStore()
const compare = useCompareStore()

const PERKS = [
  ['GH', 'Giao hàng & lắp đặt miễn phí', 'Nội thành Hà Nội, TP. HCM, Đà Nẵng trong 24h'],
  ['ĐT', 'Đổi trả trong 30 ngày', 'Đổi mới nếu lỗi do nhà sản xuất'],
  ['VAT', 'Xuất hóa đơn VAT', 'Theo yêu cầu cho cá nhân & doanh nghiệp'],
]
const STOCK = {
  InStock: ['Còn hàng', 'var(--ok-ink)'],
  LowStock: ['Sắp hết hàng', 'var(--accent)'],
  OutOfStock: ['Hết hàng', 'var(--ink-3)'],
}

const qty = ref(1)
const qtyDir = ref(0)
const qtyBox = ref(null)
const added = ref(false)
let timer

// Hiện ngay bản đã có trong store (vd. vừa bấm từ thẻ sản phẩm), rồi lấy bản mới nhất qua GET /api/products/{id};
// không có / chưa bán thì về trang chủ. "Sản phẩm tương tự": GET /api/products?category=…&take=5
const p = ref(catalog.prodOf(props.id) || null)
// Thư viện: ảnh chính + ảnh thêm (tối đa 5) và video (nếu có) ở cuối, thành các ô nhỏ dưới khung lớn; `shot` = mục đang xem
const shot = ref(0)
const media = computed(() => {
  if (!p.value) return []
  const list = p.value.images.slice(0, 5).map(src => ({ src }))
  return p.value.video ? [...list, { src: p.value.video, video: true, poster: p.value.videoPoster }] : list
})
const current = computed(() => media.value[Math.min(shot.value, media.value.length - 1)] ?? null)
// Hướng chuyển (1 = sang phải / tới, -1 = lùi) quyết định ảnh lớn trượt vào từ bên nào; 0 = không hiệu ứng (đổi sản phẩm)
const dir = ref(0)
function show(i) {
  if (i === shot.value) return
  dir.value = i > shot.value ? 1 : -1
  shot.value = i
}
function slide(d) {
  dir.value = d
  shot.value = (shot.value + d + media.value.length) % media.value.length
}
// Vuốt ngang trên ảnh lớn (màn cảm ứng) để chuyển ảnh
let touchX = null
const onTouchStart = e => (touchX = e.touches[0].clientX)
function onTouchEnd(e) {
  if (touchX === null) return
  const dx = e.changedTouches[0].clientX - touchX
  touchX = null
  if (Math.abs(dx) > 40 && media.value.length > 1) slide(dx < 0 ? 1 : -1)
}
// Ô nhỏ chưa có hình (ảnh / ảnh bìa / khung hình video chưa tải xong) hiện khung skeleton
const loadedThumbs = reactive(new Set())
const related = ref([])
const relLoading = ref(true)
const failed = ref(false)
let request = 0
watch(() => props.id, async id => {
  const req = ++request
  qty.value = 1
  qtyDir.value = 0
  added.value = false
  failed.value = false
  dir.value = 0
  shot.value = 0
  p.value = catalog.prodOf(id) || null
  related.value = []
  relLoading.value = true
  try {
    const fresh = await catalog.fetchProduct(id)
    if (req !== request) return
    if (!fresh) return router.replace('/')
    p.value = fresh
    setTitle(fresh.name)
    const list = await catalog.fetchProducts({ category: fresh.cat, take: 5 })
    if (req === request) related.value = list.filter(x => x.id !== id).slice(0, 4)
  } catch (err) {
    console.error(err)
    if (req === request && !p.value) failed.value = true
  } finally {
    if (req === request) relLoading.value = false
  }
}, { immediate: true })
onUnmounted(() => clearTimeout(timer))

const perks = computed(() => [['BH', `Bảo hành chính hãng ${warrantyOf(p.value)}`, 'Bảo hành điện tử, kích hoạt khi giao hàng'], ...PERKS])
const c = computed(() => catalog.catOf(p.value.cat))
const off = computed(() => pct(p.value))
const soldOut = computed(() => p.value.stockStatus === 'OutOfStock')
const stock = computed(() => {
  const [label, color] = STOCK[p.value.stockStatus] || STOCK.InStock
  const qty = p.value.stock != null ? ` (${p.value.stock})` : ''
  const note = p.value.stockNote ? ` · ${p.value.stockNote}` : ''
  return { text: label + qty + note, color }
})
// Bảng thông số: specs của sản phẩm, thêm màu / xuất xứ / bảo hành nếu có mà specs chưa ghi
const specRows = computed(() => {
  const rows = Object.entries(p.value.specs)
  for (const [k, v] of [['Màu sắc', p.value.color], ['Xuất xứ', p.value.origin], ['Bảo hành', p.value.warranty]]) {
    if (v && !(k in p.value.specs)) rows.push([k, v])
  }
  return [...rows, ['Thương hiệu', 'Bosch']]
})


function step(d) {
  const next = qty.value + d
  if (next < 1 || (p.value.stock != null && next > p.value.stock)) return replay(qtyBox.value, 'shake')
  qtyDir.value = d
  qty.value = next
}

function addToCart() {
  cart.add(p.value.id, qty.value)
  added.value = true
  clearTimeout(timer)
  timer = setTimeout(() => (added.value = false), 1600)
}
function buyNow() {
  cart.add(p.value.id, qty.value, { silent: true })
  router.push({ name: 'cart' })
}
function buyInstallment() {
  cart.add(p.value.id, qty.value, { silent: true })
  router.push({ name: 'checkout', query: { inst: '1' } })
}
</script>

<template>
  <div v-if="!p && failed" class="wrap">
    <div class="box empty" style="margin-top:32px">
      <h3>Không tải được sản phẩm</h3>
      <p class="muted" style="margin-top:8px">Vui lòng thử lại sau ít phút hoặc gọi hotline 1900 6868.</p>
    </div>
  </div>
  <ProductDetailSkeleton v-else-if="!p" />
  <div v-else class="wrap">
    <div class="crumb">
      <RouterLink to="/">Trang chủ</RouterLink>/<template v-if="c"><RouterLink :to="toList(c.id)">{{ c.name }}</RouterLink>/</template><span>{{ p.sub }}</span>
    </div>
    <div class="pd">
      <div class="gal">
        <div class="gal-stage" @touchstart.passive="onTouchStart" @touchend="onTouchEnd">
          <Transition :name="dir > 0 ? 'gal-next' : dir < 0 ? 'gal-prev' : 'gal-none'">
            <div v-if="current?.video" :key="current.src" class="gal-main gal-video">
              <video :src="current.src" :poster="current.poster || undefined" controls autoplay playsinline preload="metadata" />
            </div>
            <ImageBox v-else :key="current?.src" class="gal-main" :src="current?.src" :alt="p.name" :label="'ảnh ' + p.sub.toLowerCase()" />
          </Transition>
          <template v-if="media.length > 1">
            <button class="gal-nav prev" type="button" aria-label="Trước" @click="slide(-1)"><AppIcon name="chevron" :size="20" /></button>
            <button class="gal-nav next" type="button" aria-label="Sau" @click="slide(1)"><AppIcon name="chevron" :size="20" /></button>
          </template>
        </div>
        <div v-if="media.length > 1" class="gal-thumbs" :style="{ '--cols': Math.max(5, media.length) }" role="group" aria-label="Ảnh và video sản phẩm">
          <button
            v-for="(m, i) in media"
            :key="m.src"
            type="button"
            class="gal-thumb"
            :class="{ on: i === shot, video: m.video, wait: !loadedThumbs.has(m.poster || m.src) }"
            :aria-label="m.video ? 'Video sản phẩm' : `Ảnh ${i + 1}`"
            :aria-pressed="i === shot"
            @click="show(i)"
          >
            <!-- video: ảnh bìa nếu có, không thì khung hình ở giây thứ 1 của video -->
            <template v-if="m.video">
              <img v-if="m.poster" :src="m.poster" alt="" loading="lazy" @load="loadedThumbs.add(m.poster)">
              <video v-else :src="m.src + '#t=1'" muted playsinline preload="metadata" tabindex="-1" @loadeddata="loadedThumbs.add(m.src)" />
              <span class="gal-play"><AppIcon name="play" :size="16" /></span>
            </template>
            <img v-else :src="m.src" alt="" loading="lazy" @load="loadedThumbs.add(m.src)">
          </button>
        </div>
      </div>
      <div>
        <div style="display:flex;gap:6px">
          <span v-if="off" class="tag">-{{ off }}%</span>
          <span v-if="p.isNew" class="tag new">Mới</span>
          <span class="tag ok">Chính hãng</span>
        </div>
        <h1>{{ p.name }}</h1>
        <div class="pd-meta">
          <StarRating v-if="p.rating != null" :rating="p.rating" :count="p.reviews" />
          <span>Mã: <span class="mono">{{ p.model }}</span></span>
          <span :style="{ color: stock.color, fontWeight: 600 }">● {{ stock.text }}</span>
        </div>
        <ul v-if="p.chips.length" class="pd-chips"><li v-for="c in p.chips" :key="c">{{ c }}</li></ul>
        <div class="pd-price">
          <div><b>{{ fmt(p.price) }}</b><s v-if="p.old">{{ fmt(p.old) }}</s></div>
          <div class="muted" style="font-size:13px;margin-top:4px"><template v-if="p.old">Tiết kiệm {{ fmt(p.old - p.price) }} · </template>Đã gồm VAT</div>
          <div v-if="p.inst && !soldOut" class="inst-box">
            <span>Trả góp 0% chỉ từ <b>{{ fmt(monthly(p.price, p.inst.displayMonths)) }}</b>/tháng</span>
            <button class="link" @click="buyInstallment">Mua trả góp</button>
          </div>
        </div>
        <div style="display:flex;align-items:center;gap:14px">
          <span style="font-size:14px;font-weight:500">Số lượng</span>
          <div ref="qtyBox" class="qty">
            <button aria-label="Giảm" @click="step(-1)">−</button>
            <span class="qty-n" aria-live="polite"><span :key="qty" :class="{ 'roll-up': qtyDir > 0, 'roll-down': qtyDir < 0 }">{{ qty }}</span></span>
            <button aria-label="Tăng" @click="step(1)">+</button>
          </div>
          <button
            class="btn btn-ghost btn-sm cmp-toggle"
            :class="{ on: compare.has(p.id) }"
            :aria-pressed="compare.has(p.id)"
            style="margin-left:auto"
            @click="compare.toggle(p.id)"
          >
            <AppIcon name="cmp" :size="16" />{{ compare.has(p.id) ? 'Đã thêm so sánh' : 'So sánh' }}
          </button>
        </div>
        <div class="pd-buy">
          <button class="btn btn-ghost" :class="{ 'is-done': added }" :disabled="soldOut" @click="addToCart">
            <AppIcon v-if="added" name="check" :size="18" class="draw" />
            {{ added ? 'Đã thêm vào giỏ' : 'Thêm vào giỏ' }}
          </button>
          <button class="btn btn-primary" :disabled="soldOut" @click="buyNow">Mua ngay</button>
        </div>
        <div class="perks">
          <div v-for="[k, b, s] in perks" :key="k">
            <span class="k" :style="{ fontSize: k.length > 2 ? '9px' : '11px' }">{{ k }}</span>
            <div><b>{{ b }}</b><small>{{ s }}</small></div>
          </div>
        </div>
      </div>
    </div>

    <section class="sec">
      <div class="sec-h"><h2>Thông số kỹ thuật</h2></div>
      <table class="spec">
        <tbody>
          <tr v-for="[k, v] in specRows" :key="k"><td>{{ k }}</td><td>{{ v }}</td></tr>
        </tbody>
      </table>
    </section>

    <section v-if="relLoading || related.length" style="padding-bottom:24px">
      <div class="sec-h"><h2>Sản phẩm tương tự</h2></div>
      <div v-if="relLoading" class="grid" aria-busy="true" aria-label="Đang tải sản phẩm tương tự">
        <ProductCardSkeleton v-for="i in 4" :key="i" />
      </div>
      <div v-else class="grid">
        <ProductCard v-for="x in related" :key="x.id" :p="x" />
      </div>
    </section>
  </div>
</template>
