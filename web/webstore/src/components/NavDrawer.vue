<script setup>
import { ref, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { slugify } from '@/utils/format'
import { useCatalogStore } from '@/stores/catalog'
import { toList, toGroup, toSub, toFlash } from '@/router/links'
import { useDrawer } from '@/composables/useDrawer'
import AppIcon from './AppIcon.vue'
import AppLogo from './AppLogo.vue'

// Menu danh mục dạng ngăn kéo trượt từ trái, thay thanh menu ngang ở màn ≤1080 (nơi menu con không dùng được).
// Danh mục mở ra xem nhóm / loại con, mỗi lúc một danh mục; mở ngăn kéo thì bung sẵn danh mục đang xem.
const open = defineModel('open', { type: Boolean, default: false })
const route = useRoute()
const catalog = useCatalogStore()
const panel = ref(null)
useDrawer(open, panel)

// Danh mục chỉ có một loại (Khóa cửa vân tay) không có gì để mở ra
const hasKids = c => c.subs.length > 1
const expanded = ref(null)
watch(open, v => {
  if (!v) return
  const c = route.name === 'list' && catalog.catOf(route.params.cat)
  expanded.value = c && hasKids(c) ? c.id : null
})
watch(() => route.fullPath, () => (open.value = false))
// bấm link trùng trang đang xem thì route không đổi, vẫn phải đóng
const onNavClick = e => e.target.closest('a') && (open.value = false)

const toggle = id => (expanded.value = expanded.value === id ? null : id)
const inCat = c => route.name === 'list' && route.params.cat === c.id
// Không có sản phẩm flash sale nào thì ẩn link
const hasFlash = computed(() => catalog.flashCount > 0)
const isFlash = () => route.name === 'list' && route.query.flash === '1'
const isCat = c => inCat(c) && !route.query.loai && !route.query.nhom
const isGroup = (c, g) => inCat(c) && route.query.nhom === slugify(g.title)
const isSub = (c, s) => inCat(c) && route.query.loai === slugify(s)
</script>

<template>
  <Teleport to="body">
    <Transition name="drawer">
      <div v-if="open" class="drawer-wrap">
        <div class="drawer-mask" @click="open = false" />
        <div ref="panel" class="drawer left" role="dialog" aria-modal="true" aria-label="Menu danh mục" @click="onNavClick">
          <div class="drawer-h">
            <AppLogo />
            <button class="drawer-x" type="button" aria-label="Đóng menu" data-autofocus @click="open = false"><AppIcon name="close" /></button>
          </div>
          <nav class="drawer-body dn" aria-label="Danh mục sản phẩm">
            <RouterLink v-if="hasFlash" class="dn-link hot" :class="{ on: isFlash() }" :to="toFlash">Flash sale</RouterLink>
            <div v-for="c in catalog.cats" :key="c.id" class="dn-cat">
              <div class="dn-row">
                <RouterLink class="dn-link" :class="{ on: isCat(c) }" :to="toList(c.id)">{{ c.name }}</RouterLink>
                <button
                  v-if="hasKids(c)"
                  class="dn-tog"
                  type="button"
                  :aria-expanded="expanded === c.id"
                  :aria-label="(expanded === c.id ? 'Thu gọn ' : 'Mở ') + c.name"
                  @click="toggle(c.id)"
                ><AppIcon name="chevron" :size="16" /></button>
              </div>
              <div v-if="hasKids(c) && expanded === c.id" class="dn-sub">
                <template v-if="c.groups">
                  <div v-for="g in c.groups" :key="g.title" class="dn-group">
                    <RouterLink class="dn-g" :class="{ on: isGroup(c, g) }" :to="toGroup(c.id, g.title)">{{ g.title }}</RouterLink>
                    <RouterLink v-for="s in g.subs" :key="s" class="dn-s" :class="{ on: isSub(c, s) }" :to="toSub(c.id, s)">{{ s }}</RouterLink>
                  </div>
                </template>
                <template v-else>
                  <RouterLink v-for="s in c.subs" :key="s" class="dn-s" :class="{ on: isSub(c, s) }" :to="toSub(c.id, s)">{{ s }}</RouterLink>
                </template>
              </div>
            </div>
            <button class="dn-link" type="button">Khuyến mại</button>
            <button class="dn-link" type="button">Tin tức</button>
          </nav>
          <div class="drawer-f">
            <a class="btn btn-primary btn-block" href="tel:19006868"><AppIcon name="phone" :size="18" />Gọi 1900 6868</a>
            <button class="btn btn-ghost btn-block" type="button"><AppIcon name="user" :size="18" />Đăng nhập</button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>
