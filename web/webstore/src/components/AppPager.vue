<script setup>
import { computed } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import AppIcon from '@/components/AppIcon.vue'

// Phân trang theo ?trang= trên URL của trang hiện tại; null trong `items` = dấu …
// local: không đụng URL, các số trang là nút bấm, đổi trang qua v-model:page (vd. đánh giá ở trang chi tiết)
const props = defineProps({
  page: { type: Number, required: true },
  pages: { type: Number, required: true },
  local: { type: Boolean, default: false },
})
const emit = defineEmits(['update:page'])
const route = useRoute()
const tag = computed(() => (props.local ? 'button' : RouterLink))
const bind = n => (props.local
  ? { type: 'button', onClick: () => emit('update:page', n) }
  : { to: { query: { ...route.query, trang: n > 1 ? n : undefined } } })

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
    <component :is="tag" v-if="page > 1" class="pg" v-bind="bind(page - 1)" aria-label="Trang trước"><AppIcon name="chevron" :size="16" class="pg-prev" /></component>
    <span v-else class="pg off" aria-hidden="true"><AppIcon name="chevron" :size="16" class="pg-prev" /></span>
    <template v-for="(x, i) in items" :key="x ?? 'gap' + i">
      <span v-if="x === null" class="pg-gap" aria-hidden="true">…</span>
      <component :is="tag" v-else class="pg" :class="{ on: x === page }" v-bind="bind(x)" :aria-label="`Trang ${x}`" :aria-current="x === page ? 'page' : undefined">{{ x }}</component>
    </template>
    <component :is="tag" v-if="page < pages" class="pg" v-bind="bind(page + 1)" aria-label="Trang sau"><AppIcon name="chevron" :size="16" class="pg-next" /></component>
    <span v-else class="pg off" aria-hidden="true"><AppIcon name="chevron" :size="16" class="pg-next" /></span>
  </nav>
</template>
