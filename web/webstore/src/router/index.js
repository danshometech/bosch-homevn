import { createRouter, createWebHistory } from 'vue-router'
import { useCatalogStore } from '@/stores/catalog'
import HomeView from '@/views/HomeView.vue'

export const setTitle = title => (document.title = `BoschHomeVN - ${title || 'Trang chủ'}`)

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // Trang chủ chỉ cần GET /api/home (khoảnh khắc + sản phẩm), tải xong mới vào trang
    { path: '/', name: 'home', component: HomeView, beforeEnter: () => useCatalogStore().loadHome() },
    {
      path: '/san-pham/:cat?',
      name: 'list',
      component: () => import('@/views/ListingView.vue'),
      props: r => ({
        cat: r.params.cat || null,
        nhom: String(r.query.nhom || ''),
        loai: String(r.query.loai || ''),
        q: String(r.query.q || ''),
        flash: r.query.flash === '1',
        page: Math.max(1, parseInt(r.query.trang, 10) || 1),
      }),
    },
    { path: '/san-pham/chi-tiet/:id', name: 'detail', component: () => import('@/views/DetailView.vue'), props: true },
    // Link cũ /chi-tiet/:id
    { path: '/chi-tiet/:id', redirect: to => ({ name: 'detail', params: to.params }) },
    { path: '/gio-hang', name: 'cart', component: () => import('@/views/CartView.vue'), meta: { title: 'Giỏ hàng' } },
    { path: '/yeu-thich', name: 'wish', component: () => import('@/views/WishlistView.vue'), meta: { title: 'Sản phẩm đã thích' } },
    {
      path: '/thanh-toan',
      name: 'checkout',
      component: () => import('@/views/CheckoutView.vue'),
      props: r => ({ inst: r.query.inst === '1' }),
      meta: { title: 'Thanh toán' },
    },
    { path: '/tin-tuc/bai-viet/:slug', name: 'news-post', component: () => import('@/views/NewsPostView.vue'), props: true, meta: { title: 'Tin tức' } },
    {
      path: '/tin-tuc/:cat?',
      name: 'news',
      component: () => import('@/views/NewsView.vue'),
      props: r => ({ cat: r.params.cat || null }),
      meta: { title: 'Tin tức' },
    },
    { path: '/:pathMatch(.*)*', redirect: '/' },
  ],
  scrollBehavior: (to, from, saved) => saved || { top: 0 },
})

// Menu (danh mục) dùng ở mọi trang: tải một lần trước lần điều hướng đầu
router.beforeEach(async to => {
  const catalog = useCatalogStore()
  await catalog.loadMenu()
  if (to.name === 'list' && to.params.cat && !catalog.failed && !catalog.catOf(to.params.cat)) {
    return { name: 'list', query: to.query }
  }
})

// Trang chi tiết tự đặt tiêu đề sau khi tải xong sản phẩm (DetailView)
router.afterEach(to => {
  const { prodOf, catOf, subOf, groupOf } = useCatalogStore()
  const title = to.name === 'detail' ? prodOf(to.params.id)?.name
    : to.name === 'list' ? (to.query.flash === '1' ? 'Flash sale' : to.query.q ? `Tìm "${to.query.q}"`
      : subOf(to.params.cat, to.query.loai) || groupOf(to.params.cat, to.query.nhom)?.title || catOf(to.params.cat)?.name || 'Tất cả sản phẩm')
    : to.meta.title
  setTitle(title)
})

export default router
