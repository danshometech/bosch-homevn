<script setup>
import { computed, onMounted, onUnmounted } from 'vue'
import { fmt, pct, monthly, STOCK, specsOf } from '@/utils/format'
import { toDetail } from '@/router/links'
import { useCartStore } from '@/stores/cart'
import { useCompareStore } from '@/stores/compare'
import AppIcon from './AppIcon.vue'
import ImageBox from './ImageBox.vue'
import StarRating from './StarRating.vue'

const cart = useCartStore()
const compare = useCompareStore()

const ps = computed(() => compare.products)
const best = computed(() => Math.min(...ps.value.map(p => p.price)))
const rated = computed(() => ps.value.some(p => p.rating != null))
// Các dòng [tiêu đề, giá trị theo từng sản phẩm]: thông tin chung, rồi mọi thông số admin nhập của các sản phẩm
// (mỗi tên thông số một dòng, sản phẩm thiếu thì "Không có"), cuối cùng màu / xuất xứ / bảo hành.
// Dòng thông tin chung mà sản phẩm nào cũng trống thì ẩn
const rows = computed(() => {
  const list = ps.value
  const specs = list.map(p => Object.fromEntries(specsOf(p)))
  const own = list.flatMap(p => Object.keys(p.specs))
  const keys = [...new Set([...own, ...specs.flatMap(s => Object.keys(s))])]
  return [
    ['Loại sản phẩm', list.map(p => p.sub)],
    ['Mã sản phẩm', list.map(p => p.model)],
    ['Series', list.map(p => p.series && `Serie ${p.series}`)],
    ['Trả góp 0%', list.map(p => p.inst && `chỉ ${fmt(monthly(p.price, p.inst.displayMonths))}/tháng`)],
    ['Tình trạng', list.map(p => STOCK[p.stockStatus]?.[0])],
    ...keys.map(k => [k, specs.map(s => s[k])]),
  ].filter(([, values]) => values.some(Boolean))
})

const close = () => (compare.open = false)
const onKey = e => e.key === 'Escape' && close()
onMounted(() => window.addEventListener('keydown', onKey))
onUnmounted(() => window.removeEventListener('keydown', onKey))
</script>

<template>
  <Transition name="modal" :duration="{ enter: 260, leave: 180 }">
    <div v-if="compare.open && ps.length" class="modal-bg" @click="close">
      <div class="modal" role="dialog" aria-modal="true" aria-label="So sánh sản phẩm" @click.stop>
        <div class="modal-h">
          <h2 style="font-size:22px">So sánh sản phẩm</h2>
          <button class="btn btn-ghost btn-sm" @click="close">Đóng<AppIcon name="close" :size="14" /></button>
        </div>
        <div style="overflow-x:auto">
          <table class="cmp-table">
            <tbody>
              <tr>
                <th />
                <td v-for="(p, i) in ps" :key="p.id" class="rise" :style="{ animationDelay: 80 + i * 60 + 'ms' }">
                  <ImageBox :src="p.img" :alt="p.name" :label="p.sub.toLowerCase()" />
                  <RouterLink :to="toDetail(p.id)" style="font-size:14px;font-weight:600" @click="close">{{ p.name }}</RouterLink>
                </td>
              </tr>
              <tr>
                <th>Giá</th>
                <td v-for="p in ps" :key="p.id">
                  <b style="color:var(--accent)">{{ fmt(p.price) }}</b>
                  <div v-if="p.old" class="muted" style="font-size:13px"><s>{{ fmt(p.old) }}</s> · -{{ pct(p) }}%</div>
                  <div v-if="p.price === best && ps.length > 1"><span class="tag ok pop" style="display:inline-block;margin-top:6px;animation-delay:200ms">Giá tốt nhất</span></div>
                </td>
              </tr>
              <tr v-if="rated">
                <th>Đánh giá</th>
                <td v-for="p in ps" :key="p.id"><StarRating v-if="p.rating != null" :rating="p.rating" :count="p.reviews" /><template v-else>—</template></td>
              </tr>
              <tr v-for="[label, values] in rows" :key="label">
                <th>{{ label }}</th>
                <td v-for="(v, i) in values" :key="ps[i].id" :class="{ muted: !v }">{{ v || 'Không có' }}</td>
              </tr>
              <tr>
                <th />
                <td v-for="p in ps" :key="p.id"><button class="btn btn-dark btn-sm" @click="cart.add(p.id)">Thêm vào giỏ</button></td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </Transition>
</template>
