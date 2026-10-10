<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import AppIcon from '@/components/AppIcon.vue'

// Phân trang theo ?trang= trên URL của trang hiện tại; null trong `items` = dấu …
const props = defineProps({ page: { type: Number, required: true }, pages: { type: Number, required: true } })
const route = useRoute()
const to = n => ({ query: { ...route.query, trang: n > 1 ? n : undefined } })

const items = computed(() => {
  const { page: p, pages: n } = props
  const keep = new Set([1, n, p - 1, p, p + 1])
  if (p <= 3) [2, 3, 4].forEach(x => keep.add(x))
  if (p >= n - 2) [n - 1, n - 2, n - 3].forEach(x => keep.add(x))
  const list = [...keep].filter(x => x >= 1 && x <= n).sort((a, b) => a - b)
  return list.flatMap((x, i) => {
    const gap = i ? x - list[i - 1] : 1
    return gap === 1 ? [x] : gap === 2 ? [x - 1, x] : [null, x]
  })
})
</script>

<template>
  <nav v-if="pages > 1" class="pager" aria-label="Phân trang">
    <RouterLink v-if="page > 1" class="pg" :to="to(page - 1)" aria-label="Trang trước"><AppIcon name="chevron" :size="16" class="pg-prev" /></RouterLink>
    <span v-else class="pg off" aria-hidden="true"><AppIcon name="chevron" :size="16" class="pg-prev" /></span>
    <template v-for="(x, i) in items" :key="x ?? 'gap' + i">
      <span v-if="x === null" class="pg-gap" aria-hidden="true">…</span>
      <RouterLink v-else class="pg" :class="{ on: x === page }" :to="to(x)" :aria-label="`Trang ${x}`" :aria-current="x === page ? 'page' : undefined">{{ x }}</RouterLink>
    </template>
    <RouterLink v-if="page < pages" class="pg" :to="to(page + 1)" aria-label="Trang sau"><AppIcon name="chevron" :size="16" class="pg-next" /></RouterLink>
    <span v-else class="pg off" aria-hidden="true"><AppIcon name="chevron" :size="16" class="pg-next" /></span>
  </nav>
</template>
