<script setup>
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { useCartStore } from '@/stores/cart'
import { useCompareStore } from '@/stores/compare'
import { useSettingsStore } from '@/stores/settings'
import { replay } from '@/utils/motion'
import AppIcon from './AppIcon.vue'
import AppLogo from './AppLogo.vue'
import MainNav from './MainNav.vue'
import NavDrawer from './NavDrawer.vue'
import SearchBox from './SearchBox.vue'
const settings = useSettingsStore()

const cart = useCartStore()
const compare = useCompareStore()

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
      <span>Cửa hàng Online <b>chính hãng Bosch</b> · Bảo hành toàn quốc · 1800 8167 </span>
      <span class="hide-m">Hotline <b>{{ settings.hotline }}</b> · Hỗ trợ giao hàng và lắp đặt</span>
    </div>
  </div>
  <header ref="headerEl" class="header">
    <div class="wrap header-main">
      <button class="h-burger" type="button" aria-label="Mở menu danh mục" aria-haspopup="dialog" :aria-expanded="navOpen" @click="navOpen = true">
        <AppIcon name="menu" :size="22" />
      </button>
      <AppLogo />
      <SearchBox />
      <div class="h-actions">
        <RouterLink :to="{ name: 'cart' }" class="h-act" :aria-label="`Giỏ hàng, ${cart.count} sản phẩm`">
          <span ref="cartIcon" class="ic"><AppIcon name="cart" /></span>
          <span v-if="cart.count > 0" :key="cart.count" class="badge pop">{{ cart.count }}</span>
          <span class="lbl"><small>Giỏ hàng</small>{{ cart.count }} sản phẩm</span>
        </RouterLink>
        <button type="button" class="h-act" :aria-label="`So sánh sản phẩm, ${compare.ids.length} sản phẩm`" @click="compare.view()">
          <span class="ic"><AppIcon name="cmp" /></span>
          <span v-if="compare.ids.length" :key="compare.ids.length" class="badge pop">{{ compare.ids.length }}</span>
          <span class="lbl"><small>So sánh</small>{{ compare.ids.length }} sản phẩm</span>
        </button>
      </div>
    </div>
    <div class="wrap nav-wrap">
      <MainNav />
    </div>
  </header>
  <NavDrawer v-model:open="navOpen" />
</template>
