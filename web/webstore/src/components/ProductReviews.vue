<script setup>
import { ref, computed, watch, nextTick } from 'vue'
import Select from 'primevue/select'
import { fmtDate } from '@/utils/format'
import { useDrawer } from '@/composables/useDrawer'
import { getReviews, markHelpful } from '@/services/reviews'
import AppIcon from './AppIcon.vue'
import AppPager from './AppPager.vue'
import ReviewForm from './ReviewForm.vue'

const PAGE_SIZE = 5
// Thu gọn: cao chừng này px rồi mờ dần (như bảng thông số)
const CLIP = 300
const SORT_OPTS = [
  { value: 'new', label: 'Mới nhất' },
  { value: 'helpful', label: 'Hữu ích nhất' },
  { value: 'high', label: 'Điểm cao nhất' },
  { value: 'low', label: 'Điểm thấp nhất' },
]
const SELECT_PT = {
  root: ({ state }) => ({ class: ['sel', { open: state.overlayVisible }] }),
  label: { class: 'sel-label' },
  dropdown: { class: 'sel-icon' },
  overlay: { class: 'sel-panel' },
  list: { class: 'sel-list' },
  option: ({ context }) => ({ class: ['sel-opt', { on: context.selected, focus: context.focused }] }),
  transition: { name: 'menu' },
}

const props = defineProps({ product: { type: Object, required: true } })

// filter: 'all' | 'photo' | 'verified' | '1'…'5'
const filter = ref('all')
const sort = ref('new')
const summary = ref(null)
const items = ref([])
const total = ref(0)
const page = ref(1)
const loading = ref(true)
const failed = ref(false)
const writing = ref(false)
const sectionEl = ref(null)
const toolsEl = ref(null)
const listEl = ref(null)
// Như bảng thông số: lúc đầu danh sách chỉ hiện phần đầu rồi mờ dần, bấm "Xem thêm" mới hiện hết (kèm phân trang)
const open = ref(false)
const long = ref(false)
function measure() {
  long.value = !!listEl.value && listEl.value.scrollHeight > CLIP + 60
}

// Mỗi trang PAGE_SIZE đánh giá (GET /api/products/{id}/reviews?page=…); trang 1 lấy sẵn lúc vào để có tổng điểm
let request = 0
async function load(p = 1) {
  const req = ++request
  loading.value = true
  failed.value = false
  try {
    const res = await getReviews(props.product.id, {
      stars: /^\d$/.test(filter.value) ? filter.value : null,
      photos: filter.value === 'photo',
      verified: filter.value === 'verified',
      sort: sort.value === 'new' ? null : sort.value,
      page: p,
      pageSize: PAGE_SIZE,
    })
    if (req !== request) return
    summary.value = res.summary
    items.value = res.items
    total.value = res.total
    page.value = p
  } catch (err) {
    console.error(err)
    if (req === request && !summary.value) failed.value = true
  } finally {
    if (req === request) loading.value = false
  }
}
load()
// Chọn bộ lọc / cách sắp xếp là muốn xem kết quả: mở danh sách luôn, về trang 1
watch([filter, sort], () => {
  open.value = true
  load()
})
const pages = computed(() => Math.ceil(total.value / PAGE_SIZE))
const clipped = computed(() => (long.value || pages.value > 1) && !open.value)
watch(items, () => nextTick(measure), { flush: 'post' })
// Đổi trang khi đã cuộn quá hàng bộ lọc thì kéo về đầu danh sách
async function goPage(p) {
  await load(p)
  if (toolsEl.value?.getBoundingClientRect().top < 120) toolsEl.value.scrollIntoView({ behavior: 'smooth' })
}

function toggle() {
  open.value = !open.value
  // Thu gọn khi đã cuộn quá đầu mục thì kéo về đầu mục
  if (!open.value && sectionEl.value.getBoundingClientRect().top < 0) sectionEl.value.scrollIntoView()
}
const avg = computed(() => summary.value?.average ?? 0)
const pills = computed(() => {
  const s = summary.value
  if (!s) return []
  return [
    { k: 'all', label: 'Tất cả', n: s.total },
    ...(s.withPhotos ? [{ k: 'photo', label: 'Có hình ảnh', n: s.withPhotos }] : []),
    ...(s.verified ? [{ k: 'verified', label: 'Đã mua hàng', n: s.verified }] : []),
    ...[5, 4, 3, 2, 1].map(n => ({ k: String(n), label: n, star: true, n: s.stars[n] ?? 0 })),
  ]
})
const bars = computed(() => [5, 4, 3, 2, 1].map(n => {
  const c = summary.value?.stars[n] ?? 0
  return { n, c, w: summary.value?.total ? (c / summary.value.total) * 100 : 0 }
}))
const initials = name => name.trim().split(/\s+/).slice(-2).map(w => w[0]).join('').toUpperCase()

// Đổi ngay trên màn hình rồi lấy số chính xác từ server; lỗi thì trả lại như cũ
async function toggleHelpful(r) {
  if (r.busy) return
  r.busy = true
  const on = !r.voted
  r.voted = on
  r.helpfulCount += on ? 1 : -1
  try {
    Object.assign(r, await markHelpful(r.id, on))
  } catch (err) {
    console.error(err)
    r.voted = !on
    r.helpfulCount += on ? -1 : 1
  } finally {
    r.busy = false
  }
}

// Xem ảnh khách gửi: { photos, i }
const viewer = ref(null)
const viewerOpen = computed({ get: () => !!viewer.value, set: v => !v && (viewer.value = null) })
const viewerEl = ref(null)
useDrawer(viewerOpen, viewerEl)
const step = d => (viewer.value.i = (viewer.value.i + d + viewer.value.photos.length) % viewer.value.photos.length)
</script>

<template>
  <section id="danh-gia" ref="sectionEl" class="rv" aria-labelledby="rv-h">
    <div class="pd-info rv-card">
      <h2 id="rv-h" class="rv-title">Đánh giá từ khách hàng</h2>

      <div v-if="loading && !summary" class="sk-stack" style="gap:12px" aria-busy="true">
        <span v-for="w in [40, 100, 92, 70]" :key="w" class="sk sk-line" :style="{ width: w + '%' }" />
      </div>

      <div v-else-if="failed && !summary" class="rv-empty">
        <p class="muted">Không tải được đánh giá.</p>
        <button class="btn btn-ghost btn-sm" type="button" @click="load()">Thử lại</button>
      </div>

      <div v-else-if="!summary.total" class="rv-empty">
        <span class="rv-empty-ic"><AppIcon name="star" :size="28" /></span>
        <b>Chưa có đánh giá nào</b>
        <span class="muted">Hãy là người đầu tiên chia sẻ trải nghiệm về sản phẩm này.</span>
        <button class="btn btn-primary" type="button" @click="writing = true"><AppIcon name="pen" :size="16" />Viết đánh giá</button>
      </div>

      <template v-else>
        <div class="rv-sum">
          <div class="rv-score">
            <div><b>{{ avg.toFixed(1) }}</b><span>/5</span></div>
            <span class="rv-stars-lg" role="img" :aria-label="`${avg.toFixed(1)} trên 5 sao`">
              <span v-for="i in 5" :key="i"><AppIcon name="star" :size="20" /></span>
              <span class="rv-stars-fill" :style="{ width: (avg / 5) * 100 + '%' }"><span v-for="i in 5" :key="i"><AppIcon name="star" :size="20" /></span></span>
            </span>
            <small>{{ summary.total }} đánh giá</small>
          </div>
          <div class="rv-bars">
            <div v-for="b in bars" :key="b.n" class="rv-bar">
              <span class="rv-bar-k">{{ b.n }}<AppIcon name="star" :size="12" /></span>
              <span class="rv-bar-t"><span :style="{ width: b.w + '%' }" /></span>
              <span class="rv-bar-n">{{ b.c }}</span>
            </div>
          </div>
          <div class="rv-cta">
            <b>Bạn đã dùng sản phẩm này?</b>
            <span class="muted">Chia sẻ cảm nhận để giúp những người mua sau chọn đúng sản phẩm.</span>
            <button class="btn btn-primary" type="button" @click="writing = true"><AppIcon name="pen" :size="16" />Viết đánh giá</button>
          </div>
        </div>

        <div ref="toolsEl" class="rv-tools">
          <div class="rv-pills" role="group" aria-label="Lọc đánh giá">
            <button
              v-for="x in pills"
              :key="x.k"
              type="button"
              class="pill"
              :class="{ on: filter === x.k }"
              :aria-pressed="filter === x.k"
              :disabled="!x.n"
              @click="filter = x.k"
            >{{ x.label }}<AppIcon v-if="x.star" name="star" :size="12" class="rv-pill-star" /><small>({{ x.n }})</small></button>
          </div>
          <Select v-model="sort" :options="SORT_OPTS" option-label="label" option-value="value" aria-label="Sắp xếp đánh giá" :pt="SELECT_PT">
            <template #dropdownicon><AppIcon name="chevron" :size="16" /></template>
            <template #option="{ option, selected }">{{ option.label }}<AppIcon v-if="selected" name="check" :size="16" /></template>
          </Select>
        </div>

        <div class="rv-box" :class="{ clip: clipped }" :style="{ '--clip': CLIP + 'px' }" @click="clipped && toggle()">
          <div
            id="rv-list"
            ref="listEl"
            class="rv-list"
            :class="{ busy: loading }"
            :aria-busy="loading"
            :inert="clipped || undefined"
            @load.capture="measure"
          >
            <p v-if="!items.length" class="rv-none muted">Chưa có đánh giá nào phù hợp với bộ lọc này.</p>
            <article v-for="r in items" :key="r.id" class="rv-item">
              <span class="rv-av" aria-hidden="true">{{ initials(r.authorName) }}</span>
              <div class="rv-body">
                <div class="rv-who">
                  <b>{{ r.authorName }}</b>
                  <span v-if="r.isVerifiedPurchase" class="rv-ok"><AppIcon name="check" :size="13" />Đã mua hàng</span>
                </div>
                <div class="rv-line">
                  <span class="rv-stars" role="img" :aria-label="`${r.rating} trên 5 sao`">
                    <AppIcon v-for="i in 5" :key="i" name="star" :size="14" :class="{ off: i > r.rating }" />
                  </span>
                  <time class="muted" :datetime="r.createdAt">{{ fmtDate(r.createdAt) }}</time>
                </div>
                <p class="rv-text">{{ r.content }}</p>
                <div v-if="r.photos.length" class="rv-photos">
                  <button v-for="(src, i) in r.photos" :key="src" type="button" :aria-label="`Xem ảnh ${i + 1} của ${r.authorName}`" @click="viewer = { photos: r.photos, i }">
                    <img :src="src" alt="" loading="lazy">
                  </button>
                </div>
                <div>
                  <button class="rv-help" :class="{ on: r.voted }" type="button" :aria-pressed="r.voted" @click="toggleHelpful(r)">
                    <AppIcon name="like" :size="15" />Hữu ích<template v-if="r.helpfulCount"> ({{ r.helpfulCount }})</template>
                  </button>
                </div>
                <div v-if="r.reply" class="rv-reply">
                  <div class="rv-reply-h"><AppIcon name="chat" :size="15" /><b>Phản hồi từ cửa hàng</b><span v-if="r.repliedAt" class="muted">· {{ fmtDate(r.repliedAt) }}</span></div>
                  <p>{{ r.reply }}</p>
                </div>
              </div>
            </article>
            <AppPager v-if="open" :page="page" :pages="pages" local @update:page="goPage" />
          </div>
        </div>

        <button v-if="long || pages > 1" class="btn btn-ghost article-more" type="button" :aria-expanded="open" aria-controls="rv-list" @click="toggle">
          {{ open ? 'Thu gọn' : 'Xem thêm' }}<AppIcon name="chevron" :size="16" />
        </button>
      </template>
    </div>

    <ReviewForm v-model:open="writing" :product="product" />

    <Transition name="modal" :duration="{ enter: 260, leave: 180 }">
      <div v-if="viewer" class="modal-bg rv-viewer" @click="viewer = null">
        <div ref="viewerEl" class="rv-viewer-box" role="dialog" aria-modal="true" aria-label="Ảnh khách hàng gửi" @click.stop>
          <img :src="viewer.photos[viewer.i]" :alt="`Ảnh ${viewer.i + 1}/${viewer.photos.length}`">
          <button class="drawer-x rv-viewer-x" type="button" aria-label="Đóng" data-autofocus @click="viewer = null"><AppIcon name="close" /></button>
          <template v-if="viewer.photos.length > 1">
            <button class="gal-nav prev" type="button" aria-label="Ảnh trước" @click="step(-1)"><AppIcon name="chevron" :size="20" /></button>
            <button class="gal-nav next" type="button" aria-label="Ảnh sau" @click="step(1)"><AppIcon name="chevron" :size="20" /></button>
          </template>
        </div>
      </div>
    </Transition>
  </section>
</template>
