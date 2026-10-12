<script setup>
import { ref, computed, watch, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { fmt, fold } from '@/utils/format'
import { toDetail } from '@/router/links'
import { useCatalogStore } from '@/stores/catalog'
import AppIcon from './AppIcon.vue'

// Ô tìm kiếm trên header: gõ từ MIN ký tự thì gợi ý tối đa TAKE sản phẩm (GET /api/products?q=…, tìm không dấu).
// ↑ ↓ chọn, Enter mở sản phẩm đang chọn hoặc trang kết quả, Esc / bấm ra ngoài thì đóng
const MIN = 2
const TAKE = 6
const DELAY = 250

const route = useRoute()
const router = useRouter()
const catalog = useCatalogStore()

const root = ref(null)
const q = ref('')
const items = ref([])
const loading = ref(false)
const open = ref(false)
// -1: chưa chọn dòng nào; items.length: dòng "Xem tất cả"
const active = ref(-1)
let timer = 0
let request = 0

const term = computed(() => q.value.trim())
const ready = computed(() => term.value.length >= MIN)
// Tên sản phẩm tách 3 phần để tô đậm đoạn khớp từ khóa (so không dấu)
const rows = computed(() => items.value.map(p => {
  const i = fold(p.name).indexOf(fold(term.value))
  const n = term.value.length
  return { p, parts: i < 0 ? [p.name, '', ''] : [p.name.slice(0, i), p.name.slice(i, i + n), p.name.slice(i + n)] }
}))

watch(() => route.query.q, v => (q.value = v || ''), { immediate: true })
watch(() => route.fullPath, close)

function onInput() {
  clearTimeout(timer)
  active.value = -1
  if (!ready.value) {
    request++
    items.value = []
    loading.value = false
    open.value = false
    return
  }
  open.value = true
  loading.value = true
  timer = setTimeout(search, DELAY)
}

async function search() {
  const req = ++request
  try {
    const list = await catalog.fetchProducts({ q: term.value, take: TAKE })
    if (req === request) items.value = list
  } catch (err) {
    console.error(err)
    if (req === request) items.value = []
  } finally {
    if (req === request) loading.value = false
  }
}

function close() {
  open.value = false
  active.value = -1
}
function reopen() {
  if (ready.value && (items.value.length || loading.value)) open.value = true
}
function move(d) {
  if (!open.value) return reopen()
  const last = items.value.length
  const next = active.value + d
  active.value = next > last ? -1 : next < -1 ? last : next
}
function seeAll() {
  close()
  router.push({ name: 'list', query: term.value ? { q: term.value } : {} })
}
function submit() {
  const p = items.value[active.value]
  if (open.value && p) {
    close()
    router.push(toDetail(p.id))
  } else {
    seeAll()
  }
}

const onDoc = e => !root.value?.contains(e.target) && close()
watch(open, v => document[v ? 'addEventListener' : 'removeEventListener']('pointerdown', onDoc))
onUnmounted(() => {
  clearTimeout(timer)
  document.removeEventListener('pointerdown', onDoc)
})
</script>

<template>
  <div ref="root" class="search-wrap">
    <form class="search" role="search" @submit.prevent="submit">
      <input
        v-model="q"
        placeholder="Tìm bếp từ, máy rửa bát, máy giặt…"
        aria-label="Tìm sản phẩm"
        role="combobox"
        aria-autocomplete="list"
        aria-controls="search-pop"
        :aria-expanded="open"
        :aria-activedescendant="open && active >= 0 ? 'search-opt-' + active : undefined"
        autocomplete="off"
        @input="onInput"
        @focus="reopen"
        @keydown.down.prevent="move(1)"
        @keydown.up.prevent="move(-1)"
        @keydown.esc="close"
      >
      <button type="submit">Tìm</button>
    </form>
    <Transition name="sp">
      <div v-if="open" id="search-pop" class="search-pop" :class="{ busy: loading && items.length }" role="listbox" aria-label="Gợi ý sản phẩm">
        <template v-if="items.length">
          <RouterLink
            v-for="({ p, parts }, i) in rows"
            :id="'search-opt-' + i"
            :key="p.id"
            class="sp-item"
            :class="{ on: active === i }"
            role="option"
            :aria-selected="active === i"
            :to="toDetail(p.id)"
            @click="close"
            @mousemove="active = i"
          >
            <img v-if="p.img" :src="p.img" alt="" loading="lazy">
            <span v-else class="sp-ph" />
            <span class="sp-name">{{ parts[0] }}<mark>{{ parts[1] }}</mark>{{ parts[2] }}</span>
            <span class="sp-price"><b>{{ fmt(p.price) }}</b><s v-if="p.old">{{ fmt(p.old) }}</s></span>
          </RouterLink>
          <button
            :id="'search-opt-' + items.length"
            type="button"
            class="sp-all"
            :class="{ on: active === items.length }"
            role="option"
            :aria-selected="active === items.length"
            @click="seeAll"
            @mousemove="active = items.length"
          >Xem tất cả kết quả cho “{{ term }}” →</button>
        </template>
        <p v-else-if="loading" class="sp-empty"><AppIcon name="spinner" class="spin" :size="16" />Đang tìm…</p>
        <p v-else class="sp-empty">Không tìm thấy sản phẩm cho “{{ term }}”</p>
      </div>
    </Transition>
  </div>
</template>
