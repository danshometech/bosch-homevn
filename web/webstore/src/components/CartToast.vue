<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { fmt } from '@/utils/format'
import { useCartStore } from '@/stores/cart'
import { useToastStore } from '@/stores/toast'
import { useWishlistStore } from '@/stores/wishlist'
import AppIcon from './AppIcon.vue'

// Toast thêm vào giỏ (kind cart), thích / bỏ thích (kind wish) và thông báo ngắn (kind info)
const toast = useToastStore()
const cart = useCartStore()
const wish = useWishlistStore()
const route = useRoute()
const router = useRouter()
const t = computed(() => toast.current)
const off = computed(() => t.value?.kind === 'wish' && !t.value.on)
const title = computed(() => (t.value.kind === 'cart' ? 'Đã thêm vào giỏ hàng' : off.value ? 'Đã bỏ khỏi yêu thích' : 'Đã thêm vào yêu thích'))
// trang đích của nút "Xem…" đang mở thì ẩn hàng nút
const target = computed(() => (t.value.kind === 'cart' ? 'cart' : 'wish'))
const showActions = computed(() => off.value || route.name !== target.value)

function go() {
  const name = target.value
  toast.hide()
  router.push({ name })
}
const undo = () => wish.toggle(t.value.product.id)
</script>

<template>
  <Transition name="toast">
    <div
      v-if="t"
      :key="t.kind"
      class="toast"
      :class="['is-' + t.kind, { paused: toast.paused, off }]"
      role="status"
      aria-live="polite"
      @mouseenter="toast.pause()"
      @mouseleave="toast.resume()"
      @focusin="toast.pause()"
      @focusout="toast.resume()"
    >
      <template v-if="t.kind !== 'info'">
        <div class="toast-h">
          <span class="toast-ok"><AppIcon :name="t.kind === 'cart' ? 'check' : 'heart'" :size="14" /></span>
          <span :key="t.id" class="swap">{{ title }}</span>
          <button class="toast-x" type="button" aria-label="Đóng thông báo" @click="toast.hide()"><AppIcon name="close" :size="16" /></button>
        </div>
        <div :key="t.id" class="toast-item">
          <img v-if="t.product?.img" :src="t.product.img" alt="">
          <div class="toast-text">
            <p class="toast-name">{{ t.text }}</p>
            <p v-if="t.product" class="toast-price"><b>{{ fmt(t.product.price) }}</b><template v-if="t.qty > 1"> × {{ t.qty }}</template></p>
          </div>
        </div>
        <div v-if="showActions" class="toast-actions" :class="{ single: off }">
          <button v-if="off" class="btn btn-primary" type="button" @click="undo">Hoàn tác</button>
          <template v-else>
            <button class="btn btn-ghost" type="button" @click="toast.hide()">{{ t.kind === 'cart' ? 'Tiếp tục mua' : 'Tiếp tục xem' }}</button>
            <button class="btn btn-primary" type="button" @click="go">
              {{ t.kind === 'cart' ? `Xem giỏ hàng (${cart.count})` : `Xem đã thích (${wish.ids.length})` }}
            </button>
          </template>
        </div>
      </template>
      <div v-else class="toast-info">
        <AppIcon name="info" :size="18" />
        <span :key="t.id" class="swap">{{ t.text }}</span>
        <button class="toast-x" type="button" aria-label="Đóng thông báo" @click="toast.hide()"><AppIcon name="close" :size="16" /></button>
      </div>
      <span :key="t.id" class="toast-ring" aria-hidden="true" />
    </div>
  </Transition>
</template>
