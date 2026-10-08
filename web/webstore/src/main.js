import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import App from './App.vue'
import router from './router'
import './assets/styles.css'

// PrimeVue không dùng theme: component chỉ ra HTML, giao diện do styles.css quyết định (gắn class qua `pt`)
const app = createApp(App).use(createPinia()).use(router).use(PrimeVue, { unstyled: true })

// Lần điều hướng đầu chờ tải menu (và dữ liệu trang chủ nếu vào trang chủ) — mount sau đó để menu,
// khoảnh khắc không hiện trống rồi mới đổ dữ liệu vào
router.isReady().then(() => app.mount('#app'))
