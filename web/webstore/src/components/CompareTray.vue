<script setup>
import { useCompareStore, MAX_COMPARE } from '@/stores/compare'
import AppIcon from './AppIcon.vue'

const compare = useCompareStore()
</script>

<template>
  <Transition name="tray">
    <div v-if="compare.ids.length" class="cmp-tray" aria-live="polite">
      <b style="font-size:13px;white-space:nowrap">So sánh (<span :key="compare.ids.length" class="pop" style="display:inline-block">{{ compare.ids.length }}</span>/{{ MAX_COMPARE }})</b>
      <TransitionGroup name="chip" tag="div" class="chips">
        <span v-for="p in compare.products" :key="p.id" class="chip">
          <span style="overflow:hidden;text-overflow:ellipsis">{{ p.name }}</span>
          <button class="chip-x" :aria-label="'Bỏ ' + p.name" @click="compare.toggle(p.id)"><AppIcon name="close" :size="12" /></button>
        </span>
      </TransitionGroup>
      <button class="btn btn-sm" style="color:#aaa" @click="compare.clear()">Xóa</button>
      <button
        class="btn btn-primary btn-sm"
        :disabled="compare.ids.length < 2"
        :style="{ opacity: compare.ids.length < 2 ? 0.5 : 1 }"
        @click="compare.open = true"
      >So sánh ngay</button>
    </div>
  </Transition>
</template>
