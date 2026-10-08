import { defineStore } from 'pinia'
import { ref, watch } from 'vue'

const KEY = 'bh_wish'

function load() {
  try {
    return JSON.parse(localStorage.getItem(KEY)) || []
  } catch {
    return []
  }
}

// Sản phẩm yêu thích (nút tim trên thẻ), lưu localStorage
export const useWishlistStore = defineStore('wishlist', () => {
  const ids = ref(load())

  const has = id => ids.value.includes(id)
  function toggle(id) {
    ids.value = has(id) ? ids.value.filter(x => x !== id) : [...ids.value, id]
  }

  watch(ids, v => {
    try { localStorage.setItem(KEY, JSON.stringify(v)) } catch { /* private mode / storage đầy */ }
  })

  return { ids, has, toggle }
})
