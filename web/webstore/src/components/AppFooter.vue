<script setup>
import { computed } from 'vue'
import { useCatalogStore } from '@/stores/catalog'
import { useSettingsStore } from '@/stores/settings'
import { toList } from '@/router/links'
import AppLogo from './AppLogo.vue'

const catalog = useCatalogStore()
const settings = useSettingsStore()

const payments = [
  ['visa', 'Visa'],
  ['mastercard', 'Mastercard'],
  ['jcb', 'JCB'],
  ['momo', 'Ví MoMo'],
  ['vnpay', 'VNPAY'],
  ['zalopay', 'ZaloPay'],
  ['cod', 'Thanh toán khi nhận hàng (COD)'],
]
const policies = ['Bảo hành chính hãng', 'Đổi trả 30 ngày', 'Trả góp 0%', 'Giao hàng & lắp đặt', 'Bảo mật thông tin']
const contacts = computed(() => [
  { text: 'Hotline: ' + settings.hotline, href: settings.tel },
  settings.zaloUrl && { text: 'Zalo OA: bosch-homevn', href: settings.zaloUrl },
  { text: 'cskh@bosch-homevn.com' },
  { text: '8:00 – 21:00, cả CN' },
].filter(Boolean))
</script>

<template>
  <footer class="footer">
    <div class="wrap">
      <div class="footer-g">
        <div>
          <AppLogo />
          <p class="muted" style="margin-top:14px;max-width:300px">Phân phối thiết bị bếp, gia dụng và khóa cửa vân tay Bosch chính hãng. Hóa đơn VAT, bảo hành điện tử toàn quốc.</p>
          <div class="pay" aria-label="Phương thức thanh toán">
            <span v-for="[k, name] in payments" :key="k" :title="name"><img :src="`/images/payments/${k}.svg`" :alt="name" loading="lazy"></span>
          </div>
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
          <ul>
            <li v-for="x in contacts" :key="x.text">
              <a v-if="x.href" :href="x.href" :target="x.href.startsWith('http') ? '_blank' : undefined" rel="noopener">{{ x.text }}</a>
              <template v-else>{{ x.text }}</template>
            </li>
          </ul>
        </div>
      </div>
      <div class="footer-b">
        <span>© 2026 bosch-homevn.com · Đại lý phân phối được ủy quyền.</span>
        <span>MST 0123456789 · ĐKKD do Sở KH&amp;ĐT cấp</span>
      </div>
    </div>
  </footer>
</template>
