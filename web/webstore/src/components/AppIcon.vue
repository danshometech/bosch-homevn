<script setup>
defineProps({
  name: { type: String, required: true },
  size: { type: Number, default: 20 },
})
</script>

<template>
  <svg :width="size" :height="size" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
    <template v-if="name === 'cart'"><path d="M3 4h2l2.2 11h11L21 7H6.2" /><circle cx="9" cy="20" r="1.3" /><circle cx="17" cy="20" r="1.3" /></template>
    <template v-else-if="name === 'user'"><circle cx="12" cy="8" r="4" /><path d="M4 21c1.5-4 4.5-6 8-6s6.5 2 8 6" /></template>
    <template v-else-if="name === 'phone'"><path d="M5 4h4l2 5-2.5 1.5a11 11 0 0 0 5 5L15 13l5 2v4a2 2 0 0 1-2 2A16 16 0 0 1 3 6a2 2 0 0 1 2-2" /></template>
    <template v-else-if="name === 'pin'"><path d="M12 21s-7-6.5-7-12a7 7 0 0 1 14 0c0 5.5-7 12-7 12z" /><circle cx="12" cy="9" r="2.5" /></template>
    <template v-else-if="name === 'cmp'"><path d="M7 4v16M17 4v16M3 8l4-4 4 4M13 16l4 4 4-4" /></template>
    <template v-else-if="name === 'play'"><path d="M8 5.5v13l10.5-6.5z" fill="currentColor" /></template>
    <template v-else-if="name === 'chevron'"><path d="M6 9l6 6 6-6" /></template>
    <template v-else-if="name === 'left'"><path d="M15 5l-7 7 7 7" /></template>
    <template v-else-if="name === 'right'"><path d="M9 5l7 7-7 7" /></template>
    <template v-else-if="name === 'heart'"><path d="M12 20.5s-7.5-4.6-9.2-9.4C1.6 7.6 3.9 4.5 7.2 4.5c2 0 3.5 1.1 4.8 2.9 1.3-1.8 2.8-2.9 4.8-2.9 3.3 0 5.6 3.1 4.4 6.6-1.7 4.8-9.2 9.4-9.2 9.4z" /></template>
    <template v-else-if="name === 'star'"><path d="M12 2.8l2.8 5.9 6.4.8-4.7 4.4 1.2 6.4L12 17.2l-5.7 3.1 1.2-6.4L2.8 9.5l6.4-.8z" fill="currentColor" stroke="none" /></template>
    <template v-else-if="name === 'check'"><path d="M5 12.5l4.5 4.5L19 7" /></template>
    <template v-else-if="name === 'plus'"><path d="M12 5v14M5 12h14" /></template>
    <template v-else-if="name === 'close'"><path d="M6 6l12 12M18 6L6 18" /></template>
    <template v-else-if="name === 'menu'"><path d="M4 7h16M4 12h16M4 17h16" /></template>
    <template v-else-if="name === 'filter'"><path d="M4 6h16M7 12h10M10 18h4" /></template>
    <template v-else-if="name === 'info'"><circle cx="12" cy="12" r="9" /><path d="M12 11v5M12 8h.01" /></template>
    <template v-else-if="name === 'spinner'"><circle cx="12" cy="12" r="9" stroke-width="2.4" stroke-dasharray="38 100" /></template>
  </svg>
</template>
