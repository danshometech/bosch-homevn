<script setup>
defineProps({
  name: { type: String, required: true },
  size: { type: Number, default: 20 },
})
</script>

<template>
  <svg :width="size" :height="size" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
    <template v-if="name === 'cart'"><path d="M3 4h2l2.2 11h11L21 7H6.2" /><circle cx="9" cy="20" r="1.3" /><circle cx="17" cy="20" r="1.3" /></template>
    <template v-else-if="name === 'chat'"><path d="M7.9 20A9 9 0 1 0 4 16.1L2 22z" /><path d="M8 12h.01M12 12h.01M16 12h.01" stroke-width="2.4" /></template>
    <template v-else-if="name === 'up'"><path d="M12 19V5M5.5 11.5L12 5l6.5 6.5" /></template>
    <template v-else-if="name === 'phone'"><path d="M5 4h4l2 5-2.5 1.5a11 11 0 0 0 5 5L15 13l5 2v4a2 2 0 0 1-2 2A16 16 0 0 1 3 6a2 2 0 0 1 2-2" /></template>
    <template v-else-if="name === 'pin'"><path d="M12 21s-7-6.5-7-12a7 7 0 0 1 14 0c0 5.5-7 12-7 12z" /><circle cx="12" cy="9" r="2.5" /></template>
    <template v-else-if="name === 'cmp'"><path d="M7 4v16M17 4v16M3 8l4-4 4 4M13 16l4 4 4-4" /></template>
    <template v-else-if="name === 'play'"><path d="M8 5.5v13l10.5-6.5z" fill="currentColor" /></template>
    <template v-else-if="name === 'chevron'"><path d="M6 9l6 6 6-6" /></template>
    <template v-else-if="name === 'left'"><path d="M15 5l-7 7 7 7" /></template>
    <template v-else-if="name === 'right'"><path d="M9 5l7 7-7 7" /></template>
    <template v-else-if="name === 'star'"><path d="M12 2.8l2.8 5.9 6.4.8-4.7 4.4 1.2 6.4L12 17.2l-5.7 3.1 1.2-6.4L2.8 9.5l6.4-.8z" fill="currentColor" stroke="none" /></template>
    <template v-else-if="name === 'check'"><path d="M5 12.5l4.5 4.5L19 7" /></template>
    <template v-else-if="name === 'plus'"><path d="M12 5v14M5 12h14" /></template>
    <template v-else-if="name === 'close'"><path d="M6 6l12 12M18 6L6 18" /></template>
    <template v-else-if="name === 'menu'"><path d="M4 7h16M4 12h16M4 17h16" /></template>
    <template v-else-if="name === 'filter'"><path d="M4 6h16M7 12h10M10 18h4" /></template>
    <template v-else-if="name === 'info'"><circle cx="12" cy="12" r="9" /><path d="M12 11v5M12 8h.01" /></template>
    <template v-else-if="name === 'facebook'"><path d="M14 13.5h2.5l1-4H14v-2c0-1.03 0-2 2-2h1.5V2.14c-.33-.04-1.56-.14-2.86-.14C11.93 2 10 3.66 10 6.7v2.8H7v4h3V22h4z" fill="currentColor" stroke="none" /></template>
    <template v-else-if="name === 'tiktok'"><path d="M16 8.25V15.5C16 19.09 13.09 22 9.5 22S3 19.09 3 15.5 5.91 9 9.5 9c.52 0 1.02.06 1.5.17v3.17A3.5 3.5 0 0 0 9.5 12C7.57 12 6 13.57 6 15.5S7.57 19 9.5 19 13 17.43 13 15.5V2h3c0 2.76 2.24 5 5 5v3c-1.89 0-3.63-.66-5-1.75z" fill="currentColor" stroke="none" /></template>
    <template v-else-if="name === 'spinner'"><circle cx="12" cy="12" r="9" stroke-width="2.4" stroke-dasharray="38 100" /></template>
  </svg>
</template>
