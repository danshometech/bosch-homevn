<script setup>
import { ref, computed, watch, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { slugify } from '@/utils/format'
import { useCatalogStore } from '@/stores/catalog'
import { toList, toSub, toFlash } from '@/router/links'
import AppIcon from './AppIcon.vue'

const route = useRoute()
const catalog = useCatalogStore()
const open = ref(null)
let timer

// Danh mục có nhóm con → bảng nhiều cột; chỉ có danh sách loại → menu thả xuống; một loại → không có menu con
const kind = c => (c.groups ? 'mega' : c.subs.length > 1 ? 'drop' : null)

// Mở trễ một chút để lướt chuột ngang qua menu không làm bảng chớp lên; đóng trễ để kịp rê xuống bảng
function show(id) {
  clearTimeout(timer)
  timer = setTimeout(() => (open.value = id), open.value ? 0 : 80)
}
function hide(id) {
  clearTimeout(timer)
  timer = setTimeout(() => {
    if (open.value === id) open.value = null
  }, 150)
}
function leaveFocus(e, id) {
  if (!e.currentTarget.contains(e.relatedTarget)) hide(id)
}
function close() {
  clearTimeout(timer)
  open.value = null
}

watch(() => route.fullPath, close)
onUnmounted(() => clearTimeout(timer))

// Không có sản phẩm flash sale nào thì ẩn link
const hasFlash = computed(() => catalog.flashCount > 0)
const isFlash = computed(() => route.name === 'list' && route.query.flash === '1')
const isCat = c => route.name === 'list' && route.params.cat === c.id
const isSub = (c, s) => isCat(c) && route.query.loai === slugify(s)
</script>

<template>
  <nav class="nav" aria-label="Danh mục sản phẩm" @keydown.esc="close">
    <RouterLink v-if="hasFlash" class="nav-link hot" :class="{ on: isFlash }" :to="toFlash" :aria-current="isFlash ? 'page' : undefined">Flash sale</RouterLink>
    <div
      v-for="c in catalog.cats"
      :key="c.id"
      class="nav-item"
      :class="[kind(c) && 'has-' + kind(c), { open: open === c.id }]"
      @mouseenter="kind(c) && show(c.id)"
      @mouseleave="hide(c.id)"
      @focusin="kind(c) && show(c.id)"
      @focusout="leaveFocus($event, c.id)"
    >
      <RouterLink
        class="nav-link"
        :class="{ on: isCat(c) }"
        :to="toList(c.id)"
        :aria-haspopup="kind(c) ? 'true' : undefined"
        :aria-expanded="kind(c) ? open === c.id : undefined"
      >{{ c.name }}<AppIcon v-if="kind(c)" name="chevron" :size="14" class="nav-chev" /></RouterLink>

      <Transition name="menu" :duration="{ enter: 420, leave: 140 }">
        <div v-if="kind(c) === 'mega'" v-show="open === c.id" class="mega">
          <div v-for="(g, i) in c.groups" :key="g.title" class="mega-col" :style="{ animationDelay: i * 40 + 'ms' }">
            <h6>{{ g.title }}</h6>
            <ul class="menu-list">
              <li v-for="s in g.subs" :key="s"><RouterLink :to="toSub(c.id, s)" :class="{ on: isSub(c, s) }">{{ s }}</RouterLink></li>
            </ul>
          </div>
        </div>
        <ul v-else-if="kind(c) === 'drop'" v-show="open === c.id" class="drop menu-list">
          <li v-for="s in c.subs" :key="s"><RouterLink :to="toSub(c.id, s)" :class="{ on: isSub(c, s) }">{{ s }}</RouterLink></li>
        </ul>
      </Transition>
    </div>
    <button class="nav-link">Khuyến mại</button>
    <button class="nav-link">Tin tức</button>
  </nav>
</template>
