<script setup>
import { toFlash } from '@/router/links'
import { useCountdown } from '@/composables/useCountdown'

// Dải "Ưu đãi chỉ trong hôm nay" giữa ngày: số sản phẩm flash, mức giảm cao nhất, đồng hồ đếm ngược
defineProps({
  tone: { type: String, required: true },
  count: { type: Number, required: true },
  maxOff: { type: Number, required: true },
  subs: { type: String, required: true },
})
const countdown = useCountdown()
</script>

<template>
  <section id="flash" class="day-flash" :data-tone="tone" aria-labelledby="flash-h">
    <div class="day-flash-t">
      <h2 id="flash-h"><span class="dot" />Ưu đãi chỉ trong hôm nay</h2>
      <p>{{ count }} sản phẩm giảm đến {{ maxOff }}% · {{ subs }}</p>
    </div>
    <div class="cd">
      <span class="cd-l">Kết thúc sau</span>
      <span>{{ countdown[0] }}</span><i>:</i><span>{{ countdown[1] }}</span><i>:</i><span>{{ countdown[2] }}</span>
    </div>
    <RouterLink class="btn btn-primary" :to="toFlash">Xem flash sale</RouterLink>
  </section>
</template>
