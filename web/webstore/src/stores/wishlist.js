import { defineStore } from 'pinia'
import { ref, watch } from 'vue'
import { useCatalogStore } from './catalog'
import { useToastStore } from './toast'

const KEY = 'bh_wish'

function load() {
  try {
    return JSON.parse(localStorage.getItem(KEY)) || []
  } catch {
    return []
  }
}

export const useWishlistStore = defineStore('wishlist', () => {
  const ids = ref(load())

  const has = id => ids.value.includes(id)
  function toggle(id) {
    const on = !has(id)
    ids.value = on ? [...ids.value, id] : ids.value.filter(x => x !== id)
    useToastStore().liked(useCatalogStore().prodOf(id), on)
  }
  async function sync() {
    if (!ids.value.length) return
    const alive = new Set(await useCatalogStore().ensure(ids.value))
    ids.value = ids.value.filter(id => alive.has(id))
  }

  watch(ids, v => {
    try { localStorage.setItem(KEY, JSON.stringify(v)) } catch { /* private mode / storage đầy */ }
  })

  return { ids, has, toggle, sync }
})
