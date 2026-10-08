<script setup>
import { RouterLink } from 'vue-router'

// Có `src` thì hiện ảnh, không thì hiện khung placeholder sọc kèm nhãn
defineProps({
  label: { type: String, default: '' },
  src: { type: String, default: '' },
  alt: { type: String, default: '' },
  to: { type: [String, Object], default: null },
})
</script>

<template>
  <component :is="to ? RouterLink : 'div'" :to="to || undefined" class="ph">
    <img v-if="src" :src="src" :alt="alt || label" loading="lazy">
    <span v-else>{{ label }}</span>
  </component>
</template>
