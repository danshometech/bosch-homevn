<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useToastStore } from '@/stores/toast'
import AppIcon from './AppIcon.vue'

const toast = useToastStore()
const router = useRouter()
const t = computed(() => toast.current)
const short = computed(() => (t.value.text.length > 40 ? t.value.text.slice(0, 40) + '…' : t.value.text))

function openCart() {
  toast.hide()
  router.push({ name: 'cart' })
}
</script>

<template>
  <Transition name="toast">
    <div v-if="t" class="toast" role="status">
      <span class="toast-msg">
        <AppIcon :name="t.kind === 'cart' ? 'check' : 'info'" :size="16" class="toast-ic" />
        <span v-if="t.kind === 'cart'">Đã thêm vào giỏ: <b :key="t.id" class="swap" style="font-weight:600">{{ short }}</b></span>
        <span v-else :key="t.id" class="swap">{{ t.text }}</span>
      </span>
      <button v-if="t.kind === 'cart'" @click="openCart">Xem giỏ</button>
      <span :key="t.id" class="toast-bar" />
    </div>
  </Transition>
</template>
