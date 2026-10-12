<script setup>
import { onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import { useAuthStore } from '@/stores/auth'
import { useOrderBadgeStore } from '@/stores/orders'

// Mục chưa có màn hình để `to: null` — hiện mờ (Mã giảm giá chờ có API). `badge`: số hiện cạnh tên mục
// `pages`: các route thuộc mục đó (để mục sáng cả khi đang ở trang sửa / thêm)
const menu = [
  { label: 'Tổng quan', icon: 'pi pi-chart-bar', to: { name: 'dashboard' }, pages: ['dashboard'] },
  { label: 'Sản phẩm', icon: 'pi pi-box', to: { name: 'products' }, pages: ['products', 'product-new', 'product-edit'] },
  { label: 'Khoảnh khắc trang chủ', icon: 'pi pi-clock', to: { name: 'moments' }, pages: ['moments', 'moment-edit', 'hero-edit'] },
  { label: 'Danh mục', icon: 'pi pi-sitemap', to: { name: 'categories' }, pages: ['categories'] },
  { label: 'Tin tức', icon: 'pi pi-book', to: { name: 'news' }, pages: ['news', 'news-new', 'news-edit'] },
  { label: 'Đơn hàng', icon: 'pi pi-shopping-cart', to: { name: 'orders' }, pages: ['orders', 'order-detail'], badge: 'orders' },
  { label: 'Mã giảm giá', icon: 'pi pi-ticket', to: null },
  { label: 'Giao hàng & thanh toán', icon: 'pi pi-truck', to: { name: 'checkout' }, pages: ['checkout'] },
  { label: 'Cập nhật liên hệ', icon: 'pi pi-phone', to: { name: 'settings' }, pages: ['settings'] },
]

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

// Đơn mới: tải khi mở trang quản trị rồi mỗi phút một lần
const orders = useOrderBadgeStore()
let poll = 0
onMounted(() => {
  orders.refresh()
  poll = setInterval(orders.refresh, 60_000)
})
onUnmounted(() => clearInterval(poll))

async function logout() {
  await auth.logout()
  router.push({ name: 'login' })
}
</script>

<template>
  <div class="admin">
    <aside class="side">
      <div class="brand">bosch-homevn <small>Quản trị</small></div>
      <nav>
        <template v-for="m in menu" :key="m.label">
          <RouterLink v-if="m.to" :to="m.to" class="nav-item" :class="{ on: m.pages.includes(route.name) }">
            <i :class="m.icon" />{{ m.label }}
            <span v-if="m.badge === 'orders' && orders.newCount" class="nav-badge" :title="`${orders.newCount} đơn mới`">{{ orders.newCount }}</span>
          </RouterLink>
          <span v-else class="nav-item off" title="Sắp có"><i :class="m.icon" />{{ m.label }}</span>
        </template>
      </nav>
      <div class="side-user">
        <small>{{ auth.user?.userName }}</small>
        <Button label="Đăng xuất" icon="pi pi-sign-out" severity="secondary" text size="small" @click="logout" />
      </div>
    </aside>
    <main class="content">
      <RouterView />
    </main>
  </div>
</template>
