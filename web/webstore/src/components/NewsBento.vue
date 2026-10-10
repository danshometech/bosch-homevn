<script setup>
import { fmtDate } from '@/utils/format'
import { toNews, toNewsPost } from '@/router/links'

// 1–4 bài mới nhất: ô đầu to, các ô còn lại xếp theo số bài (class n1…n4). Bấm cả ô để đọc bài
defineProps({ posts: { type: Array, required: true } })
</script>

<template>
  <div class="bento" :class="'n' + posts.length">
    <article v-for="(p, i) in posts" :key="p.slug" class="bt" :class="{ lead: i === 0, 'no-img': !p.coverImageUrl }">
      <img v-if="p.coverImageUrl" :src="p.coverImageUrl" alt="" :loading="i === 0 ? 'eager' : 'lazy'">
      <div class="bt-body">
        <p class="bt-meta">
          <RouterLink class="bt-cat" :to="toNews(p.categoryId)">{{ p.categoryName }}</RouterLink>
          <time :datetime="p.publishedAt">{{ fmtDate(p.publishedAt) }}</time>
        </p>
        <component :is="i === 0 ? 'h2' : 'h3'" class="bt-title"><RouterLink class="bt-link" :to="toNewsPost(p.slug)">{{ p.title }}</RouterLink></component>
        <p v-if="i === 0 && p.summary" class="bt-sum">{{ p.summary }}</p>
      </div>
    </article>
  </div>
</template>
