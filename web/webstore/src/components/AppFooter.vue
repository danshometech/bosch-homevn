<script setup>
import { useCatalogStore } from '@/stores/catalog'
import { toList } from '@/router/links'
import AppLogo from './AppLogo.vue'

const catalog = useCatalogStore()

const payments = ['VISA', 'MASTERCARD', 'JCB', 'MOMO', 'VNPAY', 'ZALOPAY', 'COD']
const policies = ['Bảo hành chính hãng', 'Đổi trả 30 ngày', 'Trả góp 0%', 'Giao hàng & lắp đặt', 'Bảo mật thông tin']
const contacts = ['Hotline: 1900 6868', 'Zalo OA: bosch-homevn', 'cskh@bosch-homevn.com', '8:00 – 21:00, cả CN']
</script>

<template>
  <footer class="footer">
    <div class="wrap">
      <div class="footer-g">
        <div>
          <AppLogo />
          <p class="muted" style="margin-top:14px;max-width:300px">Phân phối thiết bị bếp, gia dụng và khóa cửa vân tay Bosch chính hãng. Hóa đơn VAT, bảo hành điện tử toàn quốc.</p>
          <div class="pay"><span v-for="x in payments" :key="x">{{ x }}</span></div>
        </div>
        <div>
          <h5>Sản phẩm</h5>
          <ul><li v-for="c in catalog.cats" :key="c.id"><RouterLink :to="toList(c.id)">{{ c.name }}</RouterLink></li></ul>
        </div>
        <div>
          <h5>Chính sách</h5>
          <ul><li v-for="x in policies" :key="x">{{ x }}</li></ul>
        </div>
        <div>
          <h5>Liên hệ</h5>
          <ul><li v-for="x in contacts" :key="x">{{ x }}</li></ul>
        </div>
      </div>
      <div class="footer-b">
        <span>© 2026 bosch-homevn.com · Đại lý phân phối được ủy quyền.</span>
        <span>MST 0123456789 · ĐKKD do Sở KH&amp;ĐT cấp</span>
      </div>
    </div>
  </footer>
</template>
