<script setup>
import { ref, watch } from 'vue'
import { setTitle } from '@/router'
import { fmtDate } from '@/utils/format'
import { toNews } from '@/router/links'
import { useNewsStore } from '@/stores/news'
import NewsCard from '@/components/NewsCard.vue'

const props = defineProps({ slug: { type: String, required: true } })
const news = useNewsStore()

const post = ref(null)
const loading = ref(true)
const failed = ref(false)
let request = 0

watch(() => props.slug, async slug => {
  const req = ++request
  loading.value = true
  failed.value = false
  try {
    const res = await news.fetchPost(slug)
    if (req !== request) return
    post.value = res
    if (res) setTitle(res.title)
  } catch (err) {
    console.error(err)
    if (req === request) failed.value = true
  } finally {
    if (req === request) loading.value = false
  }
}, { immediate: true })
</script>

<template>
  <div class="wrap">
    <div v-if="loading" class="np" aria-busy="true" aria-label="Đang tải bài viết">
      <div class="crumb"><span class="sk sk-line" style="width:220px" /></div>
      <span class="sk sk-line" style="width:20%;margin-bottom:14px" />
      <span class="sk sk-line" style="height:34px;margin-bottom:10px" />
      <span class="sk sk-line" style="height:34px;width:60%;margin-bottom:28px" />
      <span class="sk np-cover" />
    </div>
    <div v-else-if="failed || !post" class="box empty" style="margin-top:32px">
      <h3>{{ failed ? 'Không tải được bài viết' : 'Không tìm thấy bài viết' }}</h3>
      <p class="muted" style="margin:8px 0 20px">{{ failed ? 'Vui lòng thử lại sau ít phút.' : 'Bài viết có thể đã được gỡ hoặc đổi đường dẫn.' }}</p>
      <RouterLink class="btn btn-primary" :to="toNews()">Xem tất cả tin tức</RouterLink>
    </div>
    <template v-else>
      <article class="np">
        <div class="crumb">
          <RouterLink to="/">Trang chủ</RouterLink>/<RouterLink :to="toNews()">Tin tức</RouterLink>/<RouterLink :to="toNews(post.categoryId)">{{ post.categoryName }}</RouterLink>
        </div>
        <header class="np-h">
          <RouterLink class="nc-cat" :to="toNews(post.categoryId)">{{ post.categoryName }}</RouterLink>
          <h1>{{ post.title }}</h1>
          <p class="np-date"><time :datetime="post.publishedAt">{{ fmtDate(post.publishedAt) }}</time></p>
          <p v-if="post.summary" class="np-lead">{{ post.summary }}</p>
        </header>
        <img v-if="post.coverImageUrl" class="np-cover" :src="post.coverImageUrl" :alt="post.title">
        <!-- HTML đã lọc ở Api (HtmlSanitizer) khi admin lưu -->
        <div class="article np-body" v-html="post.html" />
      </article>

      <section v-if="post.related.length" class="sec np-related">
        <div class="sec-h">
          <h2>Bài viết liên quan</h2>
          <RouterLink class="link" :to="toNews(post.categoryId)">Xem tất cả</RouterLink>
        </div>
        <div class="news-grid">
          <NewsCard v-for="p in post.related" :key="p.slug" :post="p" />
        </div>
      </section>
    </template>
  </div>
</template>
