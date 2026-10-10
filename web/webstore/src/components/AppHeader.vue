<script setup>
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useCartStore } from '@/stores/cart'
import { useWishlistStore } from '@/stores/wishlist'
import { useSettingsStore } from '@/stores/settings'
import { replay } from '@/utils/motion'
import AppIcon from './AppIcon.vue'
import AppLogo from './AppLogo.vue'
import MainNav from './MainNav.vue'
import NavDrawer from './NavDrawer.vue'
const settings = useSettingsStore()

const route = useRoute()
const router = useRouter()
const cart = useCartStore()
const wish = useWishlistStore()

const q = ref('')
watch(() => route.query.q, v => (q.value = v || ''), { immediate: true })

function submit() {
  const term = q.value.trim()
  router.push({ name: 'list', query: term ? { q: term } : {} })
}

const navOpen = ref(false)

const cartIcon = ref(null)
watch(() => cart.count, (now, before) => {
  if (now > before) replay(cartIcon.value, 'wiggle')
})

const headerEl = ref(null)
let raf = 0
function syncBottom() {
  cancelAnimationFrame(raf)
  raf = requestAnimationFrame(() => {
    const bottom = headerEl.value?.getBoundingClientRect().bottom
    if (bottom != null) document.documentElement.style.setProperty('--header-bottom', Math.round(bottom) + 'px')
  })
}
onMounted(() => {
  syncBottom()
  window.addEventListener('scroll', syncBottom, { passive: true })
  window.addEventListener('resize', syncBottom)
})
onUnmounted(() => {
  cancelAnimationFrame(raf)
  window.removeEventListener('scroll', syncBottom)
  window.removeEventListener('resize', syncBottom)
})
</script>

<template>
  <div class="topbar">
    <div class="wrap">
      <span>Đại lý phân phối <b>chính hãng Bosch</b> · Bảo hành toàn quốc</span>
      <span class="hide-m">Hotline <b>{{ settings.hotline }}</b> · Miễn phí giao lắp nội thành</span>
    </div>
  </div>
  <header ref="headerEl" class="header">
    <div class="wrap header-main">
      <button class="h-burger" type="button" aria-label="Mở menu danh mục" aria-haspopup="dialog" :aria-expanded="navOpen" @click="navOpen = true">
        <AppIcon name="menu" :size="22" />
      </button>
      <AppLogo />
      <form class="search" role="search" @submit.prevent="submit">
        <input v-model="q" placeholder="Tìm bếp từ, máy rửa bát, máy giặt…" aria-label="Tìm sản phẩm">
        <button type="submit">Tìm</button>
      </form>
      <div class="h-actions">
        <RouterLink :to="{ name: 'cart' }" class="h-act" :aria-label="`Giỏ hàng, ${cart.count} sản phẩm`">
          <span ref="cartIcon" class="ic"><AppIcon name="cart" /></span>
          <span v-if="cart.count > 0" :key="cart.count" class="badge pop">{{ cart.count }}</span>
          <span class="lbl"><small>Giỏ hàng</small>{{ cart.count }} sản phẩm</span>
        </RouterLink>
        <RouterLink :to="{ name: 'wish' }" class="h-act" :aria-label="`Sản phẩm đã thích, ${wish.ids.length} sản phẩm`">
          <span class="ic"><AppIcon name="heart" /></span>
          <span v-if="wish.ids.length" :key="wish.ids.length" class="badge pop">{{ wish.ids.length }}</span>
          <span class="lbl"><small>Đã thích</small>{{ wish.ids.length }} sản phẩm</span>
        </RouterLink>
      </div>
    </div>
    <div class="wrap nav-wrap">
      <MainNav />
    </div>
  </header>
  <NavDrawer v-model:open="navOpen" />
</template>
