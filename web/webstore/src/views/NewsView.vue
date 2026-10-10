<script setup>
import { ref, computed, watch } from 'vue'
import { setTitle } from '@/router'
import { toNews } from '@/router/links'
import { useNewsStore } from '@/stores/news'
import NewsBento from '@/components/NewsBento.vue'
import NewsCard from '@/components/NewsCard.vue'
import NewsRow from '@/components/NewsRow.vue'

// "Tất cả": bento 4 bài mới nhất, rồi mỗi chuyên mục một hàng cuộn ngang (không lặp bài đã ở bento).
// Một chuyên mục: bento 4 bài mới nhất của chuyên mục, lưới bài còn lại, "Xem thêm" tải trang kế
const props = defineProps({ cat: { type: String, default: null } })
const news = useNewsStore()
const BENTO = 4
const PAGE = 10
const ROW = 10

const bento = ref([])
const posts = ref([])
const rows = ref([])
const total = ref(0)
const page = ref(1)
const loading = ref(true)
const loadingMore = ref(false)
const failed = ref(false)
let request = 0

const tabs = computed(() => news.categories.filter(c => c.postCount > 0 || c.id === props.cat))
const current = computed(() => (props.cat ? news.catOf(props.cat) : null))
const missing = computed(() => !!props.cat && !current.value)

watch(() => props.cat, async cat => {
  const req = ++request
  loading.value = true
  failed.value = false
  bento.value = []
  posts.value = []
  rows.value = []
  total.value = 0
  page.value = 1
  try {
    await news.loadCategories()
    if (req !== request) return
    setTitle(current.value?.name || 'Tin tức')
    if (missing.value) return
    const res = await news.fetchPosts(cat, 1, cat ? PAGE : BENTO)
    if (req !== request) return
    bento.value = res.items.slice(0, BENTO)
    posts.value = res.items.slice(BENTO)
    total.value = res.total
    loading.value = false
    if (!cat) loadRows(req)
  } catch (err) {
    console.error(err)
    if (req === request) failed.value = true
  } finally {
    if (req === request) loading.value = false
  }
}, { immediate: true })

async function loadRows(req) {
  const shown = new Set(bento.value.map(p => p.slug))
  try {
    const list = await Promise.all(tabs.value.map(async category => {
      const res = await news.fetchPosts(category.id, 1, ROW + BENTO)
      const items = res.items.filter(p => !shown.has(p.slug)).slice(0, ROW)
      const hidden = res.items.length - res.items.filter(p => !shown.has(p.slug)).length
      return { category, items, more: res.total - hidden > items.length }
    }))
    if (req === request) rows.value = list.filter(r => r.items.length)
  } catch (err) {
    console.error(err)
  }
}

async function loadMore() {
  const req = request
  loadingMore.value = true
  try {
    const res = await news.fetchPosts(props.cat, page.value + 1, PAGE)
    if (req !== request) return
    page.value++
    const have = new Set([...bento.value, ...posts.value].map(p => p.slug))
    posts.value.push(...res.items.filter(p => !have.has(p.slug)))
    total.value = res.total
  } catch (err) {
    console.error(err)
  } finally {
    loadingMore.value = false
  }
}
</script>

<template>
  <div class="wrap news">
    <div class="crumb">
      <RouterLink to="/">Trang chủ</RouterLink>/
      <template v-if="current"><RouterLink :to="toNews()">Tin tức</RouterLink>/<span>{{ current.name }}</span></template>
      <span v-else>Tin tức</span>
    </div>
    <nav v-if="tabs.length" class="pills" aria-label="Chuyên mục tin">
      <RouterLink class="pill" :class="{ on: !cat }" :to="toNews()">Tất cả</RouterLink>
      <RouterLink v-for="c in tabs" :key="c.id" class="pill" :class="{ on: cat === c.id }" :to="toNews(c.id)">{{ c.name }}</RouterLink>
    </nav>
    <header class="news-h">
      <h1>{{ current ? current.name : 'Tin tức' }}</h1>
      <p>{{ current?.description || 'Chia sẻ kinh nghiệm, mẹo hay và thông tin mới về thiết bị Bosch cho gia đình.' }}</p>
    </header>

    <div v-if="loading" class="bento n4" aria-busy="true" aria-label="Đang tải bài viết">
      <span v-for="i in 4" :key="i" class="sk" />
    </div>
    <div v-else-if="failed" class="box empty">
      <h3>Không tải được bài viết</h3>
      <p class="muted" style="margin-top:8px">Vui lòng thử lại sau ít phút.</p>
    </div>
    <div v-else-if="missing" class="box empty">
      <h3>Không tìm thấy chuyên mục</h3>
      <p class="muted" style="margin:8px 0 20px">Chuyên mục có thể đã được đổi tên hoặc gỡ bỏ.</p>
      <RouterLink class="btn btn-primary" :to="toNews()">Xem tất cả tin tức</RouterLink>
    </div>
    <div v-else-if="!bento.length" class="box empty">
      <h3>Chưa có bài viết</h3>
      <p class="muted" style="margin:8px 0 20px">Các bài chia sẻ sẽ sớm được cập nhật tại đây.</p>
      <RouterLink class="btn btn-primary" :to="{ name: 'list' }">Xem sản phẩm</RouterLink>
    </div>
    <template v-else>
      <NewsBento :posts="bento" />
      <template v-if="cat">
        <div v-if="posts.length || loadingMore" class="news-grid">
          <NewsCard v-for="p in posts" :key="p.slug" :post="p" />
          <template v-if="loadingMore">
            <div v-for="i in 3" :key="'sk' + i" class="nc" aria-hidden="true">
              <span class="sk" style="aspect-ratio:16/9;border-radius:0" />
              <div class="nc-body"><span class="sk sk-line sk-sm" style="width:40%" /><span class="sk sk-line" /><span class="sk sk-line" style="width:70%" /></div>
            </div>
          </template>
        </div>
        <div v-if="bento.length + posts.length < total" class="news-more">
          <button class="btn btn-ghost" type="button" :disabled="loadingMore" @click="loadMore">{{ loadingMore ? 'Đang tải…' : 'Xem thêm bài viết' }}</button>
        </div>
      </template>
      <NewsRow v-for="r in rows" :key="r.category.id" :category="r.category" :posts="r.items" :more="r.more" />
    </template>
  </div>
</template>
