import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import { useCatalogStore } from './catalog'
import { useToastStore } from './toast'

const KEY = 'bh_cart'

function load() {
  try {
    const saved = JSON.parse(localStorage.getItem(KEY)) || {}
    return Object.fromEntries(Object.entries(saved).filter(([, q]) => q > 0))
  } catch {
    return {}
  }
}

export const useCartStore = defineStore('cart', () => {
  const catalog = useCatalogStore()
  const { prodOf } = catalog
  const items = ref(load())

  // Dòng nào chưa có dữ liệu sản phẩm (chưa sync) thì chưa hiện; số lượng trên header đếm thẳng từ giỏ
  const lines = computed(() => Object.entries(items.value).map(([id, q]) => ({ p: prodOf(id), q })).filter(l => l.p))
  const count = computed(() => Object.values(items.value).reduce((s, q) => s + q, 0))
  const subtotal = computed(() => lines.value.reduce((s, l) => s + l.p.price * l.q, 0))
  const savings = computed(() => lines.value.reduce((s, l) => s + (l.p.old ? l.p.old - l.p.price : 0) * l.q, 0))
  const total = subtotal
  // Trả góp cả giỏ: mọi sản phẩm đều có trả góp, kỳ hạn = các kỳ hạn chung của mọi sản phẩm. Kỳ hạn tính
  // "X/tháng": kỳ hạn hiển thị nếu mọi sản phẩm cùng một kỳ hạn hiển thị, không thì kỳ hạn chung dài nhất
  const installMonths = computed(() => {
    const ps = lines.value.map(l => l.p)
    if (!ps.length || ps.some(p => !p.inst)) return []
    return ps.reduce((acc, p) => acc.filter(m => p.inst.months.includes(m)), ps[0].inst.months)
  })
  const installable = computed(() => installMonths.value.length > 0)
  const installShow = computed(() => {
    const shows = new Set(lines.value.map(l => l.p.inst?.displayMonths))
    const [only] = shows
    return shows.size === 1 && installMonths.value.includes(only) ? only : installMonths.value.at(-1)
  })

  function add(id, q = 1, { silent = false } = {}) {
    items.value[id] = (items.value[id] || 0) + q
    if (!silent) useToastStore().added(prodOf(id), q)
  }
  function setQty(id, q) {
    if (q <= 0) delete items.value[id]
    else items.value[id] = q
  }
  function remove(id) {
    setQty(id, 0)
  }
  function clear() {
    items.value = {}
  }
  // Trang giỏ hàng / thanh toán gọi: tải thông tin các mã trong giỏ (GET /api/products?ids=…),
  // mã không còn bán thì bỏ khỏi giỏ. Lỗi mạng thì giữ nguyên giỏ.
  async function sync() {
    const ids = Object.keys(items.value)
    if (!ids.length) return
    try {
      const alive = new Set(await catalog.ensure(ids))
      for (const id of ids) if (!alive.has(id)) delete items.value[id]
    } catch (err) {
      console.error(err)
    }
  }

  watch(items, v => {
    try { localStorage.setItem(KEY, JSON.stringify(v)) } catch { /* private mode / storage đầy */ }
  }, { deep: true })

  return { items, lines, count, subtotal, savings, total, installMonths, installable, installShow, add, setQty, remove, clear, sync }
})
