<script setup>
import { ref, watch, nextTick, onMounted, onUnmounted } from 'vue'
import { useCatalogStore } from '@/stores/catalog'
import { DAWN, FLASH_AFTER, TONES, VIA, CAT_SHOTS } from '@/data/day'
import { reducedMotion } from '@/utils/motion'
import { toList, toFlash } from '@/router/links'
import DayFlash from '@/components/DayFlash.vue'
import DayMoment from '@/components/DayMoment.vue'
import DayRail from '@/components/DayRail.vue'
import SceneShot from '@/components/SceneShot.vue'

const [hh, mm] = DAWN.time.split(':')
const catalog = useCatalogStore()
// Dữ liệu trang chủ đã tải trước khi vào trang (router: GET /api/home) — chỉ gồm khoảnh khắc còn sản phẩm đang bán.
// `flash`: { count, maxOff, subs } hoặc null khi không có sản phẩm flash sale.
const { moments, flash } = catalog.home
const last = moments.at(-1) ?? { tone: 'night' }
// Đoạn kết là trang cuối của dải trượt (màn nhỏ): nút "Tiếp theo" của 23:15 trỏ tới nó
const END = { time: '00:00', name: 'Ngày mai' }
// Sao trên bầu trời đoạn kết: [trái %, trên %, trễ nhịp lấp lánh (s), cỡ (px)]
const STARS = [
  [8, 14, 0, 2], [18, 32, 1.2, 1.5], [27, 9, 2.4, 2], [36, 24, 0.6, 1.5], [47, 12, 3.1, 2.5], [58, 30, 1.8, 1.5],
  [66, 8, 0.3, 2], [74, 22, 2.7, 1.5], [88, 34, 1.1, 2], [92, 12, 3.6, 1.5], [12, 52, 2.2, 1.5], [84, 56, 0.9, 2],
  [5, 78, 3.3, 1.5], [95, 74, 1.6, 2], [40, 66, 2.9, 1.5], [72, 92, 0.4, 1.5],
]
const SLIDE_TONES = [...moments.map(m => m.tone), last.tone]
const flashTone = (moments.find(m => m.id === FLASH_AFTER) || last).tone

const trust = [
  ['✓', 'Chính hãng 100%', 'Tem & hóa đơn VAT'],
  ['24', 'Bảo hành đến 24 tháng', 'Bảo hành điện tử'],
  ['0%', 'Trả góp 0% lãi suất', 'Duyệt online 5 phút'],
  ['30', 'Đổi trả 30 ngày', 'Lỗi do nhà sản xuất'],
]

// Màu nền theo giờ: pha dần giữa hai khoảnh khắc liền nhau theo vị trí cuộn, lấy banner (ảnh) của khoảnh khắc
// làm mốc: banner ló lên ở đáy màn hình thì bắt đầu pha, lên tới 40% màn hình thì xong. Mốc giờ bên trái
// sáng lên và chữ đổi màu (sáng/tối) đúng lúc màu đã pha được một nửa.
const smooth = t => t * t * (3 - 2 * t)
const mix = (a, b, e) => (e <= 0 ? a : e >= 1 ? b : `color-mix(in oklab, ${b} ${(e * 100).toFixed(1)}%, ${a})`)
function blend(from, to, e) {
  if (from === to) return TONES[to]
  const a = TONES[from], b = TONES[to], via = VIA[`${from}>${to}`]
  if (!via) return mix(a, b, e)
  return e < 0.5 ? mix(a, via, e * 2) : mix(via, b, e * 2 - 1)
}

// Màn nhỏ (<1200, khớp CSS): các khoảnh khắc thành dải trượt ngang (.day-track), mỗi lần vuốt một khoảnh khắc;
// dải flash nằm dưới dải trượt thay vì giữa 12:30 và 18:40
const SMALL = '(max-width:1199px)'
const small = ref(window.matchMedia(SMALL).matches)
let mq = null

const dayEl = ref(null)
const trackEl = ref(null)
// Dải trượt (màn nhỏ): lần đầu hiện ra trong phiên thì nhích cho trang kế ló vào rồi về chỗ
const NUDGED = 'bh_day_nudged'
let nudgeObserver = null
function nudgeTrack() {
  const track = trackEl.value
  let nudged = false
  try { nudged = sessionStorage.getItem(NUDGED) === '1' } catch { /* private mode */ }
  if (!small.value || nudged || reducedMotion() || !track) return
  nudgeObserver = new IntersectionObserver(([entry]) => {
    if (!entry.isIntersecting) return
    nudgeObserver.disconnect()
    if (track.scrollLeft > 0) return
    try { sessionStorage.setItem(NUDGED, '1') } catch { /* private mode */ }
    track.classList.add('nudge')
    const done = e => {
      if (e.target.parentElement !== track) return
      track.classList.remove('nudge')
      track.removeEventListener('animationend', done)
    }
    track.addEventListener('animationend', done)
  }, { threshold: 0.4 })
  nudgeObserver.observe(track)
}
const tone = ref(moments[0]?.tone ?? 'dawn')
const now = ref(0)
const inDay = ref(false)
let stops = [] // các khối có data-tone trong dòng chảy của ngày (khoảnh khắc, dải flash, đoạn kết)
let marks = [] // mốc đo của từng khối: ảnh khoảnh khắc, hoặc chính khối đó
let header = null
let raf = 0

function collect() {
  stops = [...dayEl.value.querySelectorAll('.day-track > [data-tone], .day-flow > [data-tone]')]
  marks = stops.map(el => el.querySelector('.moment-stage') || el)
  fitTrack()
  sync()
}

// Dải trượt cao bằng trang đang xem (không để trang ngắn như đoạn kết chừa khoảng trống dưới chân);
// màn rộng thì bỏ chiều cao cố định
function fitTrack() {
  const track = trackEl.value
  if (!track) return
  if (!small.value) { track.style.height = ''; return }
  const slide = track.children[Math.round(track.scrollLeft / track.clientWidth)]
  if (slide) track.style.height = slide.offsetHeight + 'px'
}
const fitter = new ResizeObserver(() => fitTrack())

// Vị trí trong ngày: dải trượt thì theo vị trí vuốt ngang, còn lại theo cuộn dọc.
// Trả về [khối trước, khối sau, tiến độ chuyển 0..1, khoảnh khắc đang xem]
function position(H) {
  const track = trackEl.value
  if (small.value && track) {
    const max = SLIDE_TONES.length - 1
    const p = Math.min(Math.max(track.scrollLeft / track.clientWidth, 0), max)
    const k = Math.floor(p)
    if (Math.abs(p - Math.round(p)) < 0.02) fitTrack()
    return [SLIDE_TONES[k], SLIDE_TONES[Math.min(k + 1, max)], p - k, Math.min(Math.round(p), moments.length - 1)]
  }
  // banner cuối cùng đã ló lên, nó đi được bao nhiêu trong vùng chuyển màu, và khoảnh khắc đang sáng
  let k = 0, t = 0, cur = 0
  marks.forEach((el, j) => {
    const v = (H - el.getBoundingClientRect().top) / (H * 0.6)
    if (j && v > 0) { k = j; t = Math.min(1, v) }
    if (v >= 0.5 && stops[j].dataset.moment) cur = +stops[j].dataset.moment
  })
  return [stops[Math.max(0, k - 1)].dataset.tone, stops[k].dataset.tone, t, cur]
}

function sync() {
  cancelAnimationFrame(raf)
  raf = requestAnimationFrame(() => {
    const day = dayEl.value
    if (!day || !stops.length) return
    const H = window.innerHeight
    const [from, to, t, cur] = position(H)
    const e = smooth(t)
    // Đặt thẳng lên phần tử (không qua state) vì giá trị đổi theo từng khung hình khi cuộn
    day.style.setProperty('--tone-bg', blend(from, to, e))
    tone.value = e >= 0.5 ? to : from
    now.value = cur
    const track = trackEl.value
    atEnd.value = small.value && !!track && track.scrollLeft / track.clientWidth >= moments.length - 0.02
    // Lề trái và viên đồng hồ chỉ hiện khi banner hero đã cuộn khuất hẳn sau header
    const r = day.getBoundingClientRect()
    inDay.value = r.top <= (header?.getBoundingClientRect().bottom ?? 0) + 1 && r.bottom > H / 2
    document.body.classList.toggle('side-on', inDay.value)
  })
}

// Cuộn bằng JS thay vì link #hash, vì scrollBehavior của router sẽ đưa trang về đầu
// Sang trang thứ i của dải trượt (0..số khoảnh khắc; trang cuối là đoạn kết)
// align = false (nút ‹ › trên ảnh): chỉ trượt ngang, giữ nguyên vị trí cuộn dọc
function goSlide(i, align = true) {
  const behavior = reducedMotion() ? 'auto' : 'smooth'
  const track = trackEl.value
  track.scrollTo({ left: i * track.clientWidth, behavior })
  if (!align) return
  // đưa đầu dải trượt lên ngay dưới header + thanh thời gian (khi đang ở giữa một khoảnh khắc dài, hoặc còn ở hero)
  const limit = (header?.getBoundingClientRect().bottom ?? 0) + (document.querySelector('.day-side')?.offsetHeight ?? 0)
  const top = track.getBoundingClientRect().top
  if (Math.abs(top - limit) > 2) window.scrollTo({ top: window.scrollY + top - limit, behavior })
}
function go(id) {
  const i = moments.findIndex(m => m.id === id)
  if (small.value && trackEl.value && i >= 0) return goSlide(i)
  document.getElementById(id)?.scrollIntoView({ behavior: reducedMotion() ? 'auto' : 'smooth', block: 'start' })
}
function step(i, d, align = true) {
  const to = Math.min(Math.max(i + d, 0), moments.length)
  // đoạn kết ngắn hơn khoảnh khắc nên luôn đưa lên đầu, không thì có thể đang đứng dưới đoạn kết
  if (small.value && trackEl.value) goSlide(to, align || to === moments.length)
  else if (moments[i + d]) go(moments[i + d].id)
}
// Dải trượt dừng ở đoạn kết: thanh loading chạy 10 giây rồi tự về khoảnh khắc đầu (vuốt đi chỗ khác thì hủy)
const END_DELAY = 10000
const atEnd = ref(false)
let endTimer = 0
watch(atEnd, on => {
  clearTimeout(endTimer)
  if (on) endTimer = setTimeout(() => goSlide(0), END_DELAY)
})

function onMedia(e) {
  small.value = e.matches
  nextTick(collect)
}

onMounted(() => {
  header = document.querySelector('.header')
  if (!moments.length) return // API lỗi / chưa có khoảnh khắc nào: không có phần "Ngày"
  ;[...trackEl.value.children].forEach(el => fitter.observe(el))
  collect()
  nudgeTrack()
  mq = window.matchMedia(SMALL)
  mq.addEventListener('change', onMedia)
  window.addEventListener('scroll', sync, { passive: true })
  window.addEventListener('resize', sync)
})
onUnmounted(() => {
  cancelAnimationFrame(raf)
  clearTimeout(endTimer)
  fitter.disconnect()
  nudgeObserver?.disconnect()
  mq?.removeEventListener('change', onMedia)
  window.removeEventListener('scroll', sync)
  window.removeEventListener('resize', sync)
  document.body.classList.remove('side-on')
})
</script>

<template>
  <section class="dawn" aria-labelledby="dawn-h">
    <SceneShot tone="dawn" :src="DAWN.img" :alt="DAWN.alt" eager />
    <div class="wrap dawn-in">
      <div>
        <p class="dawn-time"><i class="live" /><b>{{ hh }}<span class="blink">:</span>{{ mm }}</b><span>A Day with Bosch</span></p>
        <h1 id="dawn-h"><span>Ngày mới</span><span>bắt đầu từ đây.</span></h1>
        <p class="dawn-lead">Một ngày trong căn nhà có Bosch — từ ly cà phê đầu tiên đến mẻ giặt cuối cùng trước giờ ngủ.</p>
        <div class="dawn-cta">
          <RouterLink class="btn btn-light btn-lg" :to="toList('thiet-bi-bep')">Khám phá căn bếp <span aria-hidden="true">→</span></RouterLink>
          <a v-if="moments.length" class="dawn-scroll" :href="'#' + moments[0].id" @click.prevent="go(moments[0].id)"><i />Cuộn để bắt đầu ngày</a>
        </div>
      </div>
      <RouterLink v-if="flash" class="dawn-promo" :to="toFlash"><b><span class="dot" />Flash sale</b>Giảm đến <em>{{ flash.maxOff }}%</em> · chỉ trong hôm nay →</RouterLink>
    </div>
  </section>

  <div v-if="moments.length" ref="dayEl" class="day" :data-tone="tone">
    <div class="wrap day-in">
      <aside class="day-side">
        <DayRail :moments="moments" :now="now" @go="go" />
        <div class="consult">
          <p class="consult-k"><i />Tư vấn miễn phí</p>
          <p class="consult-h">Cần tư vấn chọn thiết bị?</p>
          <p class="consult-p">Gửi kích thước căn bếp, chuyên viên sẽ gợi ý thiết bị Bosch phù hợp.</p>
          <button class="consult-btn zalo" type="button">Chat Zalo</button>
          <a class="consult-btn call" href="tel:19006868">Gọi 1900 6868</a>
          <button class="consult-link" type="button">12 showroom · tìm gần bạn →</button>
        </div>
      </aside>
      <div class="day-flow">
        <div ref="trackEl" class="day-track" @scroll.passive="sync">
          <template v-for="(m, i) in moments" :key="m.id">
            <DayMoment :m="m" :index="i" :total="moments.length" :next="moments[i + 1] || END" @step="(d, align) => step(i, d, align)" />
            <DayFlash v-if="!small && flash && m.id === FLASH_AFTER" :tone="m.tone" :count="flash.count" :max-off="flash.maxOff" :subs="flash.subs" />
          </template>
          <!-- màn rộng: đoạn kết cuối dòng chảy dọc · màn nhỏ: trang cuối của dải trượt, ngay sau 23:15 -->
          <section class="epilogue" :data-tone="last.tone" aria-labelledby="epi-h">
            <div class="night-sky" aria-hidden="true">
              <i v-for="([x, y, d, s], n) in STARS" :key="n" :style="{ left: x + '%', top: y + '%', '--d': d + 's', '--s': s + 'px' }" />
              <span class="moon" />
            </div>
            <p class="epilogue-t"><time>{{ END.time }}</time><span class="zzz" aria-hidden="true"><i>z</i><i>z</i><i>z</i></span></p>
            <h2 id="epi-h">Ngày mai, căn bếp lại thức dậy cùng bạn.</h2>
            <div v-if="small" class="epilogue-bar" :class="{ run: atEnd }" :style="{ '--delay': END_DELAY + 'ms' }" aria-hidden="true"><i /></div>
          </section>
        </div>
        <DayFlash v-if="small && flash" :tone="flashTone" :count="flash.count" :max-off="flash.maxOff" :subs="flash.subs" />
      </div>
    </div>
    <div v-if="!small" class="day-sunrise" aria-hidden="true" />
  </div>
  <div class="wrap after-day">
    <section class="sec">
      <div class="sec-h"><div><h2>Chọn thiết bị cho một ngày của bạn</h2><p>Chính hãng Bosch · giao và lắp đặt miễn phí nội thành</p></div></div>
      <div class="cats">
        <RouterLink v-for="c in catalog.cats" :key="c.id" class="cat" :to="toList(c.id)">
          <SceneShot v-if="CAT_SHOTS[c.id]" v-bind="CAT_SHOTS[c.id].shot" />
          <b>{{ c.name }}</b><small v-if="CAT_SHOTS[c.id]">{{ CAT_SHOTS[c.id].desc }}</small>
        </RouterLink>
      </div>
    </section>

    <div class="trust">
      <div v-for="[n, b, s] in trust" :key="b">
        <span class="n">{{ n }}</span>
        <div><b>{{ b }}</b><small>{{ s }}</small></div>
      </div>
    </div>
  </div>
</template>
