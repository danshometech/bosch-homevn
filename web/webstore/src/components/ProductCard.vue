<script setup>
import { computed } from 'vue'
import { fmt, pct, monthly, warrantyOf } from '@/utils/format'
import { toDetail } from '@/router/links'
import { useWishlistStore } from '@/stores/wishlist'
import AppIcon from './AppIcon.vue'
import ImageBox from './ImageBox.vue'

// Bấm cả thẻ để xem chi tiết (link phủ kín thẻ); nút tim nằm trên lớp link
const props = defineProps({
  p: { type: Object, required: true },
  flash: { type: Boolean, default: false },
})
const wish = useWishlistStore()

const to = computed(() => toDetail(props.p.id))
const inst = computed(() => props.p.inst)
const fav = computed(() => wish.has(props.p.id))
const off = computed(() => pct(props.p))
</script>

<template>
  <article class="pc">
    <span v-if="off" class="pc-off">Giảm {{ off }}%</span>
    <span v-if="p.isNew" class="pc-off pc-new">Mới</span>
    <span v-if="inst" class="pc-inst">Trả góp <b>0%</b></span>
    <div class="pc-img">
      <img v-if="p.img" :src="p.img" alt="" loading="lazy">
      <ImageBox v-else :label="'ảnh ' + p.sub.toLowerCase()" />
    </div>
    <h4 class="pc-name"><RouterLink class="pc-link" :to="to">{{ p.name }}</RouterLink></h4>
    <p class="pc-price"><b>{{ fmt(p.price) }}</b><s v-if="p.old">{{ fmt(p.old) }}</s></p>
    <p v-if="flash && p.stock != null" class="stock"><small>Còn {{ p.stock }} sản phẩm</small></p>
    <ul v-if="p.chips.length" class="pc-chips"><li v-for="c in p.chips" :key="c">{{ c }}</li></ul>
    <div class="pc-perks">
      <p class="pc-perk blue">Miễn phí giao &amp; lắp đặt nội thành</p>
      <p class="pc-perk violet">Bảo hành chính hãng {{ warrantyOf(p) }}</p>
    </div>
    <p v-if="inst" class="pc-pay">Trả góp 0% - 0đ trả trước - chỉ <b>{{ fmt(monthly(p.price, inst.displayMonths)) }}</b>/tháng</p>
    <div class="pc-foot">
      <span v-if="p.rating != null" class="pc-rate" role="img" :aria-label="`${p.rating.toFixed(1)} trên 5 sao`"><AppIcon name="star" :size="16" />{{ p.rating.toFixed(1) }}</span>
      <button class="pc-fav" type="button" :aria-pressed="fav" :aria-label="`Yêu thích ${p.name}`" @click="wish.toggle(p.id)">
        <AppIcon name="heart" :size="22" />
      </button>
    </div>
  </article>
</template>
