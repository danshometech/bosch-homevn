import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import Aura from '@primeuix/themes/aura'
import App from './App.vue'
import router from './router'
import 'primeicons/primeicons.css'
import './assets/admin.css'

// Chữ mặc định của các component PrimeVue (hộp xác nhận, bảng trống…) bằng tiếng Việt
const locale = {
  accept: 'Đồng ý',
  reject: 'Hủy',
  emptyMessage: 'Không có dữ liệu',
  emptyFilterMessage: 'Không tìm thấy',
  emptySearchMessage: 'Không tìm thấy',
  emptySelectionMessage: 'Chưa chọn',
  searchMessage: '{0} kết quả',
  choose: 'Chọn',
  upload: 'Tải lên',
  cancel: 'Hủy',
}

// Admin dùng PrimeVue 4 có theme (Aura) — bảng/form dựng sẵn, không tự viết CSS như site bán hàng
createApp(App)
  .use(createPinia())
  .use(router)
  .use(PrimeVue, { theme: { preset: Aura, options: { darkModeSelector: 'none' } }, locale })
  .use(ToastService)
  .use(ConfirmationService)
  .mount('#app')
