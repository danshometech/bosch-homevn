<script setup>
import AppIcon from './AppIcon.vue'
</script>

<template>
  <div class="float">
    <!-- chữ bị ẩn trên màn nhỏ hơn 1720px nên đặt tên đầy đủ qua aria-label -->
    <button class="fab zalo" type="button" aria-label="Chat tư vấn qua Zalo"><span class="c">Zalo</span><span class="t">Chat tư vấn</span></button>
    <a class="fab hot" href="tel:19006868" aria-label="Gọi hotline 1900 6868"><span class="c"><AppIcon name="phone" :size="18" /></span><span class="t">1900 6868</span></a>
  </div>
</template>
