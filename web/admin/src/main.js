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
  // Lịch (DatePicker)
  dayNames: ['Chủ nhật', 'Thứ hai', 'Thứ ba', 'Thứ tư', 'Thứ năm', 'Thứ sáu', 'Thứ bảy'],
  dayNamesShort: ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'],
  dayNamesMin: ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'],
  monthNames: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6', 'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'],
  monthNamesShort: ['Th1', 'Th2', 'Th3', 'Th4', 'Th5', 'Th6', 'Th7', 'Th8', 'Th9', 'Th10', 'Th11', 'Th12'],
  firstDayOfWeek: 1,
  dateFormat: 'dd/mm/yy',
  today: 'Hôm nay',
  clear: 'Xóa',
  weekHeader: 'Tuần',
}

// Admin dùng PrimeVue 4 có theme (Aura) — bảng/form dựng sẵn, không tự viết CSS như site bán hàng
createApp(App)
  .use(createPinia())
  .use(router)
  .use(PrimeVue, { theme: { preset: Aura, options: { darkModeSelector: 'none' } }, locale })
  .use(ToastService)
  .use(ConfirmationService)
  .mount('#app')
