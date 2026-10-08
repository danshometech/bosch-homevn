<script setup>
import { fmt, pct } from '@/utils/format'
import { toDetail } from '@/router/links'
import { vReveal } from '@/directives/reveal'
import { useRowNav } from '@/composables/useRowNav'
import AppIcon from './AppIcon.vue'
import ProductCard from './ProductCard.vue'
import SceneShot from './SceneShot.vue'

// Một khoảnh khắc trong ngày: giờ lớn, ảnh khung cảnh kèm số liệu, phần chữ cạnh hàng thẻ sản phẩm
// (đè lên mép dưới ảnh, cuộn ngang), rồi dải "Mua kèm". Các khoảnh khắc lẻ/chẵn so le trái/phải.
// Màn nhỏ các khoảnh khắc thành dải trượt ngang: nút ‹ › và "Tiếp theo" báo `step` (±1) để trang chủ chuyển trang;
// `next` là khoảnh khắc kế tiếp (với khoảnh khắc cuối là đoạn kết 00:00).
// `m` lấy từ GET /api/home: products / extras.items là sản phẩm đang bán, fact { value, note } hoặc null.
defineProps({
  m: { type: Object, required: true },
  index: { type: Number, required: true },
  total: { type: Number, required: true },
  next: { type: Object, default: null },
})
const emit = defineEmits(['step'])

const pad = n => String(n).padStart(2, '0')

const shelf = useRowNav(300, { nudge: true })
const more = useRowNav()
const bar = nav => ({ '--w': nav.ratio, '--x': nav.progress })
</script>

<template>
  <section :id="m.id" class="moment" :class="{ flip: index % 2 }" :data-tone="m.tone" :data-moment="index" :aria-labelledby="m.id + '-h'">
    <div v-reveal class="moment-h">
      <p class="moment-t"><time>{{ m.time }}</time></p>
      <p class="moment-k"><small>{{ pad(index + 1) }} / {{ pad(total) }}</small><b>{{ m.name }}</b></p>
      <div class="moment-nav">
        <button type="button" aria-label="Khoảnh khắc trước" :disabled="index === 0" @click="emit('step', -1)"><AppIcon name="left" :size="18" /></button>
        <button type="button" aria-label="Khoảnh khắc sau" :disabled="!next" @click="emit('step', 1)"><AppIcon name="right" :size="18" /></button>
      </div>
    </div>
    <div v-reveal class="moment-stage">
      <SceneShot :tone="m.tone" :src="m.img" :still="m.still" :pos="m.pos" :alt="m.alt" />
      <p v-if="m.fact" class="moment-fact"><b>{{ m.fact.value }}</b>{{ m.fact.note }}</p>
      <!-- màn nhỏ (dải trượt): nút kính mờ báo trượt ngang được, bấm thì sang khoảnh khắc trước / sau -->
      <button v-if="index > 0" class="swipe-hint prev" type="button" aria-label="Khoảnh khắc trước" @click="emit('step', -1, false)">
        <AppIcon name="left" :size="22" />
      </button>
      <button v-if="next" class="swipe-hint next" type="button" :aria-label="`Sang ${next.time} · ${next.name}`" @click="emit('step', 1, false)">
        <AppIcon name="right" :size="22" />
      </button>
    </div>
    <div v-reveal class="moment-foot">
      <div class="moment-copy">
        <h2 :id="m.id + '-h'">{{ m.title }}</h2>
        <p>{{ m.lead }}</p>
        <div class="moment-links">
          <RouterLink v-for="l in m.links" :key="l.label" class="link" :to="l.to">{{ l.label }}</RouterLink>
        </div>
      </div>
      <div class="moment-shelf">
        <button class="shelf-btn prev" type="button" aria-label="Sản phẩm trước" :disabled="shelf.atStart" @click="shelf.scroll(-1)">
          <AppIcon name="left" :size="18" />
        </button>
        <div :ref="shelf.bind" class="moment-row" @scroll.passive="shelf.sync">
          <ProductCard v-for="p in m.products" :key="p.id" :p="p" />
        </div>
        <div v-show="!(shelf.atStart && shelf.atEnd)" class="row-bar" :style="bar(shelf)" aria-hidden="true"><i /></div>
        <button class="shelf-btn next" type="button" aria-label="Sản phẩm sau" :disabled="shelf.atEnd" @click="shelf.scroll(1)">
          <AppIcon name="right" :size="18" />
        </button>
      </div>
    </div>
    <div v-reveal class="extras">
      <div class="extras-h">
        <p>{{ m.extras.title }}</p>
        <div v-show="!(more.atStart && more.atEnd)" class="extras-nav">
          <button type="button" aria-label="Sản phẩm trước" :disabled="more.atStart" @click="more.scroll(-1)"><AppIcon name="left" :size="18" /></button>
          <button type="button" aria-label="Sản phẩm sau" :disabled="more.atEnd" @click="more.scroll(1)"><AppIcon name="right" :size="18" /></button>
        </div>
      </div>
      <div :ref="more.bind" class="extras-row" @scroll.passive="more.sync">
        <RouterLink v-for="p in m.extras.items" :key="p.id" class="xc" :to="toDetail(p.id)">
          <span class="xc-img"><img v-if="p.img" :src="p.img" alt="" loading="lazy"></span>
          <span class="xc-t">
            <span class="xc-sub">{{ p.sub }}</span>
            <b class="xc-name">{{ p.name }}</b>
            <span class="xc-price"><b>{{ fmt(p.price) }}</b><span v-if="pct(p)" class="xc-off">-{{ pct(p) }}%</span></span>
          </span>
        </RouterLink>
        <RouterLink v-if="m.extras.more" class="xc xc-more" :to="m.extras.more.to">{{ m.extras.more.label }}<span aria-hidden="true">→</span></RouterLink>
      </div>
      <div v-show="!(more.atStart && more.atEnd)" class="row-bar" :style="bar(more)" aria-hidden="true"><i /></div>
    </div>
    <button v-if="next" class="moment-next" type="button" @click="emit('step', 1)">
      <span>Tiếp theo</span><b>{{ next.time }} · {{ next.name }}</b><AppIcon name="right" :size="18" />
    </button>
  </section>
</template>
