<script setup>
// Ảnh khung cảnh trong ngày: ảnh bối cảnh `src` (phủ màu theo giờ qua .shot-grade), hoặc ảnh tĩnh sản phẩm `still`
// đặt trên mảng tường có bóng nắng ô cửa. Nền gradient theo giờ (`tone`) hiện trong lúc ảnh đang tải.
// Không dùng ImageBox vì component đó dành cho ảnh sản phẩm (contain, nền sọc).
defineProps({
  tone: { type: String, required: true },
  src: { type: String, default: '' },
  still: { type: String, default: '' },
  pos: { type: String, default: '' },
  alt: { type: String, default: '' },
  eager: { type: Boolean, default: false },
})
</script>

<template>
  <figure class="shot" :data-tone="tone" :style="pos ? { '--pos': pos } : null">
    <template v-if="still">
      <i class="shot-light" />
      <img class="shot-prod" :src="still" :alt="alt" loading="lazy">
    </template>
    <template v-else>
      <img :src="src" :alt="alt" :loading="eager ? 'eager' : 'lazy'" :fetchpriority="eager ? 'high' : undefined">
      <i class="shot-grade" />
    </template>
  </figure>
</template>
