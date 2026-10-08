<script setup>
import { computed, onMounted, onUnmounted } from 'vue'
import { fmt } from '@/utils/format'
import { toDetail } from '@/router/links'
import { useCartStore } from '@/stores/cart'
import { useCompareStore } from '@/stores/compare'
import AppIcon from './AppIcon.vue'
import ImageBox from './ImageBox.vue'
import StarRating from './StarRating.vue'

const cart = useCartStore()
const compare = useCompareStore()

const ps = computed(() => compare.products)
const keys = computed(() => [...new Set(ps.value.flatMap(p => Object.keys(p.specs)))])
const best = computed(() => Math.min(...ps.value.map(p => p.price)))

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
                  <div v-if="p.price === best && ps.length > 1"><span class="tag ok pop" style="display:inline-block;margin-top:6px;animation-delay:200ms">Giá tốt nhất</span></div>
                </td>
              </tr>
              <tr>
                <th>Đánh giá</th>
                <td v-for="p in ps" :key="p.id"><StarRating v-if="p.rating != null" :rating="p.rating" :count="p.reviews" /><template v-else>—</template></td>
              </tr>
              <tr v-for="k in keys" :key="k">
                <th>{{ k }}</th>
                <td v-for="p in ps" :key="p.id">{{ p.specs[k] || '—' }}</td>
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
