<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import AppHeader from '@/components/AppHeader.vue'
import AppFooter from '@/components/AppFooter.vue'
import FloatContact from '@/components/FloatContact.vue'
import CompareTray from '@/components/CompareTray.vue'
import CompareModal from '@/components/CompareModal.vue'
import CartToast from '@/components/CartToast.vue'
import { useCatalogStore } from '@/stores/catalog'
import { useSettingsStore } from '@/stores/settings'

const route = useRoute()
const catalog = useCatalogStore()
const settings = useSettingsStore()
settings.load()
const showTray = computed(() => route.name !== 'cart' && route.name !== 'checkout')
</script>

<template>
  <AppHeader />
  <main>
    <div v-if="catalog.failed" class="wrap">
      <div class="box empty" role="alert" style="margin-top:24px">
        <h3>Không tải được danh sách sản phẩm</h3>
        <p class="muted" style="margin-top:8px">Vui lòng tải lại trang sau ít phút hoặc gọi hotline {{ settings.hotline }}.</p>
      </div>
    </div>
    <RouterView />
  </main>
  <AppFooter />
  <FloatContact />
  <CompareTray v-if="showTray" />
  <CompareModal />
  <CartToast />
</template>
