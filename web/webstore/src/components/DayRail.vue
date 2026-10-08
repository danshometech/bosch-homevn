<script setup>
import { ref, watch } from 'vue'
import { reducedMotion } from '@/utils/motion'

// Thanh đồng hồ: các mốc giờ, vạch tiến trình tới mốc đang xem (`now`).
// Màn ≥1200 là cột dọc; nhỏ hơn là thanh ngang cuộn được, mốc đang xem tự trượt vào giữa.
const props = defineProps({
  moments: { type: Array, required: true },
  now: { type: Number, required: true },
})
const emit = defineEmits(['go'])

const list = ref(null)
watch(() => props.now, i => {
  const el = list.value
  const link = el?.children[i]?.querySelector('a') // căn theo chữ của mốc, không tính đoạn kẻ nối phía trước
  if (!link || el.scrollWidth <= el.clientWidth) return
  const r = link.getBoundingClientRect(), box = el.getBoundingClientRect()
  el.scrollBy({ left: r.left + r.width / 2 - (box.left + box.width / 2), behavior: reducedMotion() ? 'auto' : 'smooth' })
})
</script>

<template>
  <nav class="rail" aria-label="Các thời điểm trong ngày">
    <p class="rail-h">A Day <br>with Bosch</p>
    <ol ref="list" class="rail-list" :style="{ '--p': now / (moments.length - 1) }">
      <li v-for="(m, i) in moments" :key="m.id" :class="{ on: i === now, past: i < now }">
        <a :href="'#' + m.id" :aria-current="i === now ? 'step' : undefined" @click.prevent="emit('go', m.id)">
          <time>{{ m.time }}</time><span>{{ m.name }}</span>
        </a>
      </li>
    </ol>
  </nav>
</template>
