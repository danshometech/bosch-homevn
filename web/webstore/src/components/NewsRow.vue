<script setup>
import { useRowNav } from '@/composables/useRowNav'
import { toNews } from '@/router/links'
import AppIcon from './AppIcon.vue'
import NewsCard from './NewsCard.vue'

// Một chuyên mục: hàng bài cuộn ngang (nút ‹ › + thanh vị trí như hàng sản phẩm trang chủ); more = còn bài chưa hiện
defineProps({
  category: { type: Object, required: true },
  posts: { type: Array, required: true },
  more: { type: Boolean, default: false },
})
const nav = useRowNav(316)
const bar = () => ({ '--w': nav.ratio, '--x': nav.progress })
</script>

<template>
  <section class="nr" :aria-labelledby="'nr-' + category.id">
    <div class="nr-h">
      <h2 :id="'nr-' + category.id"><RouterLink :to="toNews(category.id)">{{ category.name }}</RouterLink></h2>
      <RouterLink class="link" :to="toNews(category.id)">Xem tất cả</RouterLink>
      <div v-show="!(nav.atStart && nav.atEnd)" class="nr-nav">
        <button type="button" aria-label="Bài trước" :disabled="nav.atStart" @click="nav.scroll(-1)"><AppIcon name="left" :size="18" /></button>
        <button type="button" aria-label="Bài sau" :disabled="nav.atEnd" @click="nav.scroll(1)"><AppIcon name="right" :size="18" /></button>
      </div>
    </div>
    <div :ref="nav.bind" class="nr-track" @scroll.passive="nav.sync">
      <NewsCard v-for="p in posts" :key="p.slug" :post="p" compact />
      <RouterLink v-if="more" class="nr-more" :to="toNews(category.id)">Xem tất cả bài {{ category.name }}<span aria-hidden="true">→</span></RouterLink>
    </div>
    <div v-show="!(nav.atStart && nav.atEnd)" class="row-bar" :style="bar()" aria-hidden="true"><i /></div>
  </section>
</template>
