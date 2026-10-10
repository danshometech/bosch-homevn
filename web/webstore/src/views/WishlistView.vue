<script setup>
import { ref, computed } from 'vue'
import { useCatalogStore } from '@/stores/catalog'
import { useWishlistStore } from '@/stores/wishlist'
import { useSettingsStore } from '@/stores/settings'
import ProductCard from '@/components/ProductCard.vue'
import ProductCardSkeleton from '@/components/ProductCardSkeleton.vue'
const settings = useSettingsStore()

const catalog = useCatalogStore()
const wish = useWishlistStore()

const ready = ref(false)
const failed = ref(false)
wish.sync()
  .catch(err => {
    console.error(err)
    failed.value = true
  })
  .finally(() => (ready.value = true))

const items = computed(() => wish.ids.map(id => catalog.prodOf(id)).filter(Boolean).reverse())
</script>

<template>
  <div class="wrap">
    <div class="crumb"><RouterLink to="/">Trang chủ</RouterLink>/<span>Sản phẩm đã thích</span></div>
    <h1 style="font-size:28px;margin-bottom:20px">
      Sản phẩm đã thích <span v-if="ready && items.length" class="muted" style="font-weight:400;font-size:18px">({{ items.length }})</span>
    </h1>
    <div v-if="!ready" class="grid" aria-busy="true" aria-label="Đang tải sản phẩm">
      <ProductCardSkeleton v-for="i in Math.min(wish.ids.length, 8)" :key="i" />
    </div>
    <div v-else-if="failed" class="box empty">
      <h3>Không tải được sản phẩm</h3>
      <p class="muted" style="margin-top:8px">Vui lòng thử lại sau ít phút hoặc gọi hotline {{ settings.hotline }}.</p>
    </div>
    <div v-else-if="items.length" class="grid">
      <ProductCard v-for="p in items" :key="p.id" :p="p" />
    </div>
    <div v-else class="box empty rise">
      <h2>Chưa có sản phẩm nào</h2>
      <p class="muted" style="margin:8px 0 24px">Bấm biểu tượng tim trên sản phẩm để lưu lại xem sau.</p>
      <RouterLink class="btn btn-primary" :to="{ name: 'list' }">Xem sản phẩm</RouterLink>
    </div>
  </div>
</template>
