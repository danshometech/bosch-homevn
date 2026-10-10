import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { setUnauthorizedHandler } from '@/services/api'
import AdminLayout from '@/layouts/AdminLayout.vue'

const routes = [
  { path: '/dang-nhap', name: 'login', component: () => import('@/views/LoginView.vue'), meta: { public: true, title: 'Đăng nhập' } },
  {
    path: '/',
    component: AdminLayout,
    children: [
      { path: '', name: 'dashboard', component: () => import('@/views/DashboardView.vue'), meta: { title: 'Tổng quan' } },
      { path: 'san-pham', name: 'products', component: () => import('@/views/ProductsView.vue'), meta: { title: 'Sản phẩm' } },
      { path: 'san-pham/moi', name: 'product-new', component: () => import('@/views/ProductEditView.vue'), meta: { title: 'Thêm sản phẩm' } },
      { path: 'san-pham/:id', name: 'product-edit', component: () => import('@/views/ProductEditView.vue'), props: true, meta: { title: 'Sửa sản phẩm' } },
      { path: 'danh-muc', name: 'categories', component: () => import('@/views/CategoriesView.vue'), meta: { title: 'Danh mục' } },
      { path: 'khoanh-khac', name: 'moments', component: () => import('@/views/MomentsView.vue'), meta: { title: 'Khoảnh khắc trang chủ' } },
      { path: 'khoanh-khac/:id', name: 'moment-edit', component: () => import('@/views/MomentEditView.vue'), props: true, meta: { title: 'Sửa khoảnh khắc' } },
      { path: 'tin-tuc', name: 'news', component: () => import('@/views/NewsView.vue'), meta: { title: 'Tin tức' } },
      { path: 'tin-tuc/moi', name: 'news-new', component: () => import('@/views/NewsEditView.vue'), meta: { title: 'Viết bài' } },
      { path: 'tin-tuc/:id', name: 'news-edit', component: () => import('@/views/NewsEditView.vue'), props: true, meta: { title: 'Sửa bài viết' } },
      { path: 'chuyen-muc-tin', redirect: { name: 'news', query: { tab: 'chuyen-muc' } } },
      { path: 'giao-hang-thanh-toan', name: 'checkout', component: () => import('@/views/CheckoutSettingsView.vue'), meta: { title: 'Giao hàng & thanh toán' } },
      { path: 'lien-he', name: 'settings', component: () => import('@/views/SettingsView.vue'), meta: { title: 'Cập nhật liên hệ' } },
      { path: 'cai-dat', redirect: { name: 'settings' } },
    ],
  },
  { path: '/:pathMatch(.*)*', redirect: '/' },
]

const router = createRouter({ history: createWebHistory(), routes })

// Trang nào cũng cần đăng nhập, trừ trang đăng nhập
router.beforeEach(async to => {
  const user = await useAuthStore().ensure()
  if (to.meta.public) return user && to.name === 'login' ? { name: 'dashboard' } : true
  if (!user) return { name: 'login', query: to.fullPath !== '/' ? { next: to.fullPath } : {} }
})

router.afterEach(to => {
  document.title = `${to.meta.title ? to.meta.title + ' — ' : ''}Quản trị bosch-homevn`
})

// API trả 401 giữa chừng (hết phiên): về trang đăng nhập, đăng nhập xong quay lại trang đang xem
setUnauthorizedHandler(() => {
  useAuthStore().expire()
  const current = router.currentRoute.value
  if (current.name !== 'login') router.push({ name: 'login', query: { next: current.fullPath } })
})

export default router
