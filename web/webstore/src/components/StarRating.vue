<script setup>
import { computed } from 'vue'

const props = defineProps({
  rating: { type: Number, required: true },
  count: { type: Number, default: null },
})
const full = computed(() => Math.round(props.rating))
</script>

<template>
  <div class="stars" :aria-label="`${rating.toFixed(1)} trên 5 sao`">
    <span class="s">{{ '★'.repeat(full) }}<span style="color:#ddd">{{ '★'.repeat(5 - full) }}</span></span>{{ rating.toFixed(1) }}<span v-if="count != null">({{ count }})</span>
  </div>
</template>
