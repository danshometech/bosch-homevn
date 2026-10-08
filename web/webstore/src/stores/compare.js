import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { useCatalogStore } from './catalog'
import { useToastStore } from './toast'

export const MAX_COMPARE = 4

export const useCompareStore = defineStore('compare', () => {
  const { prodOf } = useCatalogStore()
  const ids = ref([])
  const open = ref(false)

  const products = computed(() => ids.value.map(prodOf).filter(Boolean))

  const has = id => ids.value.includes(id)
  function toggle(id) {
    if (has(id)) ids.value = ids.value.filter(x => x !== id)
    else if (ids.value.length < MAX_COMPARE) ids.value = [...ids.value, id]
    else useToastStore().show(`Chỉ so sánh tối đa ${MAX_COMPARE} sản phẩm`, 'info')
    if (!ids.value.length) open.value = false
  }
  function clear() {
    ids.value = []
    open.value = false
  }

  return { ids, open, products, has, toggle, clear }
})
