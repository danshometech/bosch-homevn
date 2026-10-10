<script setup>
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { reducedMotion } from '@/utils/motion'
import { useSettingsStore } from '@/stores/settings'
import AppIcon from './AppIcon.vue'

const settings = useSettingsStore()
const root = ref(null)
const open = ref(false)
const showTop = ref(false)

const onScroll = () => (showTop.value = window.scrollY > 600)
const onDoc = e => !root.value?.contains(e.target) && (open.value = false)
const onKey = e => e.key === 'Escape' && (open.value = false)
watch(open, v => {
  const fn = v ? 'addEventListener' : 'removeEventListener'
  document[fn]('pointerdown', onDoc)
  document[fn]('keydown', onKey)
})
onMounted(() => {
  onScroll()
  window.addEventListener('scroll', onScroll, { passive: true })
})
onUnmounted(() => {
  window.removeEventListener('scroll', onScroll)
  open.value = false
})

const toTop = () => window.scrollTo({ top: 0, behavior: reducedMotion() ? 'auto' : 'smooth' })
</script>

<template>
  <div ref="root" class="float" :class="{ open }">
    <Transition name="fab-pop">
      <button v-if="showTop && !open" class="fab-top" type="button" aria-label="Lên đầu trang" @click="toTop">
        <AppIcon name="up" :size="20" />
      </button>
    </Transition>
    <div class="fab-wrap">
      <Transition name="fab-menu" :duration="{ enter: 360, leave: 220 }">
        <ul v-if="open" id="fab-menu" class="fab-menu">
          <li v-if="settings.zaloUrl" style="--i:1">
            <a class="fab-item" :href="settings.zaloUrl" target="_blank" rel="noopener" @click="open = false"><span class="t">Chat tư vấn Zalo</span><span class="c zalo">Zalo</span></a>
          </li>
          <li style="--i:0">
            <a class="fab-item" :href="settings.tel" @click="open = false"><span class="t">Gọi {{ settings.hotline }}</span><span class="c hot"><AppIcon name="phone" :size="18" /></span></a>
          </li>
        </ul>
      </Transition>
      <button
        class="fab-main"
        type="button"
        :aria-label="open ? 'Đóng liên hệ' : 'Liên hệ tư vấn'"
        :aria-expanded="open"
        aria-controls="fab-menu"
        @click="open = !open"
      >
        <AppIcon class="i" name="chat" :size="20" />
        <AppIcon class="x" name="close" :size="20" />
      </button>
    </div>
  </div>
</template>
