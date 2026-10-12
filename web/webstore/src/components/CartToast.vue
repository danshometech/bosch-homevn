<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { fmt } from '@/utils/format'
import { useCartStore } from '@/stores/cart'
import { useToastStore } from '@/stores/toast'
import AppIcon from './AppIcon.vue'

// Toast thêm vào giỏ (kind cart) và thông báo ngắn (kind info)
const toast = useToastStore()
const cart = useCartStore()
const route = useRoute()
const router = useRouter()
const t = computed(() => toast.current)
// đang ở giỏ hàng thì ẩn hàng nút
const showActions = computed(() => route.name !== 'cart')

function go() {
  toast.hide()
  router.push({ name: 'cart' })
}
</script>

<template>
  <Transition name="toast">
    <div
      v-if="t"
      :key="t.kind"
      class="toast"
      :class="['is-' + t.kind, { paused: toast.paused }]"
      role="status"
      aria-live="polite"
      @mouseenter="toast.pause()"
      @mouseleave="toast.resume()"
      @focusin="toast.pause()"
      @focusout="toast.resume()"
    >
      <template v-if="t.kind === 'cart'">
        <div class="toast-h">
          <span class="toast-ok"><AppIcon name="check" :size="14" /></span>
          <span :key="t.id" class="swap">Đã thêm vào giỏ hàng</span>
          <button class="toast-x" type="button" aria-label="Đóng thông báo" @click="toast.hide()"><AppIcon name="close" :size="16" /></button>
        </div>
        <div :key="t.id" class="toast-item">
          <img v-if="t.product?.img" :src="t.product.img" alt="">
          <div class="toast-text">
            <p class="toast-name">{{ t.text }}</p>
            <p v-if="t.product" class="toast-price"><b>{{ fmt(t.product.price) }}</b><template v-if="t.qty > 1"> × {{ t.qty }}</template></p>
          </div>
        </div>
        <div v-if="showActions" class="toast-actions">
          <button class="btn btn-ghost" type="button" @click="toast.hide()">Tiếp tục mua</button>
          <button class="btn btn-primary" type="button" @click="go">Xem giỏ hàng ({{ cart.count }})</button>
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
