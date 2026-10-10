import { defineStore } from 'pinia'
import { ref, watch } from 'vue'
import { useCatalogStore } from './catalog'

const KEY = 'bh_viewed'
const MAX = 20

function load() {
  try {
    const v = JSON.parse(localStorage.getItem(KEY))
    return Array.isArray(v) ? v : []
  } catch {
    return []
  }
}

// Sản phẩm đã xem ở trang chi tiết, mới xem nhất đứng đầu, lưu localStorage
export const useViewedStore = defineStore('viewed', () => {
  const ids = ref(load())

  function add(id) {
    ids.value = [id, ...ids.value.filter(x => x !== id)].slice(0, MAX)
  }
  function forget(dead) {
    if (dead.length) ids.value = ids.value.filter(x => !dead.includes(x))
  }
  // Các sản phẩm xem gần nhất còn bán (trừ exclude): lấy thông tin còn thiếu qua GET /api/products?ids=…,
  // mã không còn bán thì bỏ khỏi lịch sử
  async function recent(exclude = [], limit = 4) {
    const catalog = useCatalogStore()
    const cand = ids.value.filter(x => !exclude.includes(x)).slice(0, limit * 2)
    if (!cand.length) return []
    const alive = new Set(await catalog.ensure(cand))
    forget(cand.filter(x => !alive.has(x)))
    return cand.filter(x => alive.has(x)).slice(0, limit).map(x => catalog.prodOf(x))
  }

  watch(ids, v => {
    try { localStorage.setItem(KEY, JSON.stringify(v)) } catch { /* private mode / storage đầy */ }
  })

  return { ids, add, forget, recent }
})
