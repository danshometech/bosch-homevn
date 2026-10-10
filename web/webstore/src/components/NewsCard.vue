<script setup>
import { fmtDate } from '@/utils/format'
import { toNews, toNewsPost } from '@/router/links'

defineProps({
  post: { type: Object, required: true },
  compact: { type: Boolean, default: false },
})
</script>

<template>
  <article class="nc" :class="{ compact, 'no-img': !post.coverImageUrl }">
    <RouterLink v-if="post.coverImageUrl" class="nc-img" :to="toNewsPost(post.slug)" tabindex="-1" aria-hidden="true">
      <img :src="post.coverImageUrl" alt="" loading="lazy">
    </RouterLink>
    <div class="nc-body">
      <p class="nc-meta">
        <RouterLink class="nc-cat" :to="toNews(post.categoryId)">{{ post.categoryName }}</RouterLink>
        <time :datetime="post.publishedAt">{{ fmtDate(post.publishedAt) }}</time>
      </p>
      <h3 class="nc-title"><RouterLink :to="toNewsPost(post.slug)">{{ post.title }}</RouterLink></h3>
      <p v-if="post.summary && !compact" class="nc-sum">{{ post.summary }}</p>
    </div>
  </article>
</template>
